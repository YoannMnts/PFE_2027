using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PFE.DataMapping.Generator
{
    /// <summary>
    /// Drives the whole "write one decorated behaviour interface, get everything
    /// else" model. For each interface annotated with
    /// <c>[GenerateContainer]</c> it emits the container interface and the
    /// concrete container; for each struct implementing such an interface it
    /// emits the <c>ISelfMapping</c> partial; and once per assembly it emits a
    /// <c>RuntimeInitializeOnLoadMethod</c> bootstrap that registers them all.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class ContainerGenerator : IIncrementalGenerator
    {
        private const string GenAttr = "PFE.Core.DataMapping.GenerateContainerAttribute";
        private const string AddAttr = "PFE.Core.DataMapping.AddToContainerAttribute";
        private const string BehaviourIface = "PFE.Core.DataMapping.IBehaviour`1";

        // ---- diagnostics ------------------------------------------------------------------

        private const string Category = "PFE.DataMapping";

        /// <summary>The decorated behaviour interface's TData must constrain to exactly one data-root type (class or interface).</summary>
        private static readonly DiagnosticDescriptor DataRootRule = new(
            id: "HTX001",
            title: "Behaviour interface must have exactly one data-root constraint",
            messageFormat: "'{0}' is marked [GenerateContainer] but its type parameter '{1}' resolves {2} candidate data-root constraints; exactly one is required (a single class or interface, e.g. 'where {1} : PatternData' or 'where {1} : IEffectData')",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        /// <summary>A method marked [AddToContainer] must take the data type as its first parameter.</summary>
        private static readonly DiagnosticDescriptor FirstParamRule = new(
            id: "HTX002",
            title: "Method marked [AddToContainer] must take the data type as its first parameter",
            messageFormat: "Method '{0}' on '{1}' is marked [AddToContainer] but does not take '{2}' as its first parameter; only methods whose first parameter is the behaviour's data type can be forwarded by the generated container",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        // ---- model carried between phases -------------------------------------------------

        /// <summary>A domain described by one decorated behaviour interface.</summary>
        private sealed class DomainModel
        {
            public INamedTypeSymbol BehaviourInterface;   // e.g. ICapacityEffect<TData>
            public ITypeSymbol DataRoot;                  // e.g. IEffectData (the TData constraint)
            public string Namespace;                      // null => global
            public string Root;                           // e.g. "CapacityEffect"
            public string ContainerIface => "I" + Root + "Container";
            public string ContainerClass => Root + "Container";
            public ImmutableArray<IMethodSymbol> ForwardedMethods; // first param == TData
        }

        /// <summary>A struct that implements a decorated behaviour interface.</summary>
        private sealed class MappingModel
        {
            public INamedTypeSymbol Struct;
            public ITypeSymbol DataType;        // closed TData of the implemented behaviour
            public DomainModel Domain;
            public string Namespace;            // of the struct
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Phase 1: decorated behaviour interfaces -> DomainModel
            var domains = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (node, _) =>
                        node is InterfaceDeclarationSyntax i && i.AttributeLists.Count > 0,
                    transform: static (ctx, _) =>
                        ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol)
                .Where(static s => s is not null);

            // Phase 2: candidate behaviour implementers — partial struct OR partial
            // class, with a base list. Abstract classes are excluded here (they are
            // not instantiable, so they get no self-mapping); an intermediate
            // abstract base is filtered out. Concrete leaf classes that inherit the
            // decorated interface transitively are kept.
            var structs = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (node, _) =>
                        node is TypeDeclarationSyntax t &&
                        (t is StructDeclarationSyntax || t is ClassDeclarationSyntax) &&
                        t.BaseList is not null &&
                        t.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) &&
                        !t.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                    transform: static (ctx, _) =>
                        ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol)
                .Where(static s => s is not null);

            var all = context.CompilationProvider
                .Combine(domains.Collect())
                .Combine(structs.Collect());

            context.RegisterSourceOutput(all, static (spc, triple) =>
            {
                var ((compilation, domainSyms), structSyms) = triple;
                Emit(spc, compilation, domainSyms, structSyms);
            });
        }

        private static void Emit(
            SourceProductionContext spc,
            Compilation compilation,
            ImmutableArray<INamedTypeSymbol?> domainSyms,
            ImmutableArray<INamedTypeSymbol?> structSyms)
        {
            var genAttr = compilation.GetTypeByMetadataName(GenAttr);
            var addAttr = compilation.GetTypeByMetadataName(AddAttr);
            var behaviourIface = compilation.GetTypeByMetadataName(BehaviourIface);
            if (genAttr is null || addAttr is null || behaviourIface is null) return;

            // --- build domain models ---
            var domains = new Dictionary<INamedTypeSymbol, DomainModel>(SymbolEqualityComparer.Default);
            foreach (var iface in domainSyms.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
            {
                var model = BuildDomain(spc, iface, genAttr, addAttr);
                if (model is null) continue;
                domains[iface.OriginalDefinition] = model;
                EmitContainerInterface(spc, model);
                EmitConcreteContainer(spc, model);
            }
            if (domains.Count == 0) return;

            // --- self-mapping for each implementing struct, grouped per assembly ---
            var registrations = new List<MappingModel>();
            foreach (var st in structSyms.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
            {
                var m = MatchStruct(st, domains);
                if (m is null) continue;
                EmitSelfMapping(spc, m);
                registrations.Add(m);
            }

            EmitBootstrap(spc, compilation, registrations);
        }

        // -------------------------------------------------------------------- domain

        private static DomainModel BuildDomain(
            SourceProductionContext spc, INamedTypeSymbol iface,
            INamedTypeSymbol genAttr, INamedTypeSymbol addAttr)
        {
            var attr = iface.GetAttributes().FirstOrDefault(a =>
                SymbolEqualityComparer.Default.Equals(a.AttributeClass, genAttr));
            if (attr is null) return null;
            if (iface.TypeParameters.Length != 1) return null;

            var tData = iface.TypeParameters[0];

            // Data root = the single explicit constraint on TData, which may be a
            // class (e.g. abstract PatternData) or an interface (e.g. IEffectData).
            // We deliberately ignore System.Object / System.ValueType implied by a
            // 'class' / 'struct' constraint keyword, and ignore other type params.
            // Anything other than exactly one such constraint is a hard error
            // (HTX001): we will not silently pick "the first" and generate a
            // container against the wrong root.
            var rootCandidates = tData.ConstraintTypes
                .Where(t => t.TypeKind == TypeKind.Class || t.TypeKind == TypeKind.Interface)
                .Where(t => t.SpecialType != SpecialType.System_Object &&
                            t.SpecialType != SpecialType.System_ValueType)
                .ToImmutableArray();

            if (rootCandidates.Length != 1)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    DataRootRule,
                    iface.Locations.FirstOrDefault(),
                    iface.Name, tData.Name, rootCandidates.Length));
                return null;
            }

            ITypeSymbol dataRoot = rootCandidates[0];

            // Root name: explicit ctor-arg override, else strip leading 'I'.
            string root = attr.ConstructorArguments.Length > 0
                ? attr.ConstructorArguments[0].Value as string
                : null;
            if (string.IsNullOrEmpty(root))
            {
                root = iface.Name;
                if (root.Length > 1 && root[0] == 'I' && char.IsUpper(root[1]))
                    root = root.Substring(1);
            }

            // Only methods marked [AddToContainer] are surfaced on the container.
            // Each such method MUST take TData first (HTX002, hard error). Unmarked
            // methods (helpers, DIM bodies) are left untouched.
            var marked = iface.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary)
                .Where(m => m.GetAttributes().Any(a =>
                    SymbolEqualityComparer.Default.Equals(a.AttributeClass, addAttr)))
                .ToImmutableArray();

            bool hadError = false;
            foreach (var m in marked)
            {
                bool firstIsData = m.Parameters.Length >= 1 &&
                    SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, tData);
                if (!firstIsData)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        FirstParamRule,
                        m.Locations.FirstOrDefault() ?? iface.Locations.FirstOrDefault(),
                        m.Name, iface.Name, tData.Name));
                    hadError = true;
                }
            }
            if (hadError) return null;

            return new DomainModel
            {
                BehaviourInterface = iface,
                DataRoot = dataRoot,
                Namespace = iface.ContainingNamespace.IsGlobalNamespace
                    ? null : iface.ContainingNamespace.ToDisplayString(),
                Root = root,
                ForwardedMethods = marked, // only [AddToContainer]; all validated TData-first
            };
        }

        // -------------------------------------------------------------------- emit: iface

        private static void EmitContainerInterface(SourceProductionContext spc, DomainModel d)
        {
            var scope = new Scope(d.Namespace);
            scope.Collect(d.DataRoot);
            foreach (var m in d.ForwardedMethods)
            {
                if (!m.ReturnsVoid) scope.Collect(m.ReturnType);
                for (int i = 1; i < m.Parameters.Length; i++) scope.Collect(m.Parameters[i].Type);
            }

            string rootName = scope.Display(d.DataRoot);
            var sb = Header();
            HeaderWithUsings(sb, scope, d.Namespace, out bool hasNs);

            sb.Append("    public interface ").Append(d.ContainerIface)
              .Append(" : global::PFE.Core.DataMapping.IContainer<").Append(rootName).AppendLine(">");
            sb.AppendLine("    {");
            foreach (var m in d.ForwardedMethods)
                sb.Append("        ").Append(SignatureWidened(m, rootName, scope)).AppendLine(";");
            sb.AppendLine("    }");

            CloseNs(sb, hasNs);
            spc.AddSource($"{d.ContainerIface}.Container", sb.ToString());
        }

        // -------------------------------------------------------------------- emit: concrete

        private static void EmitConcreteContainer(SourceProductionContext spc, DomainModel d)
        {
            var scope = new Scope(d.Namespace);
            scope.Collect(d.DataRoot);
            scope.Collect(d.BehaviourInterface);
            foreach (var m in d.ForwardedMethods)
            {
                if (!m.ReturnsVoid) scope.Collect(m.ReturnType);
                for (int i = 1; i < m.Parameters.Length; i++) scope.Collect(m.Parameters[i].Type);
            }

            string rootName = scope.Display(d.DataRoot);
            // Behaviour interface short name without its arity suffix.
            string behName = scope.Display(d.BehaviourInterface.ConstructUnboundGenericType());
            int lt = behName.IndexOf('<');
            if (lt >= 0) behName = behName.Substring(0, lt);

            var sb = Header();
            HeaderWithUsings(sb, scope, d.Namespace, out bool hasNs);

            sb.Append("    public sealed class ").Append(d.ContainerClass)
              .AppendLine("<TData, TBehaviour>");
            sb.Append("        : global::PFE.Core.DataMapping.Container<TData, TBehaviour>, ")
              .AppendLine(d.ContainerIface);
            sb.Append("        where TData : ").AppendLine(rootName);
            sb.Append("        where TBehaviour : ").Append(behName).AppendLine("<TData>");
            sb.AppendLine("    {");
            sb.Append("        public ").Append(d.ContainerClass)
              .AppendLine("(TBehaviour behaviour) : base(behaviour) { }");
            sb.AppendLine();

            foreach (var m in d.ForwardedMethods)
                EmitForwardBody(sb, m, rootName, scope);

            sb.AppendLine("    }");
            CloseNs(sb, hasNs);
            spc.AddSource($"{d.ContainerClass}.Container.Impl", sb.ToString());
        }

        /// <summary>
        /// Emits the downcast-and-forward implementation of one behaviour method.
        /// The first parameter is widened to the data root; the body does
        /// <c>if (data is TData typed)</c> then forwards to the concrete
        /// <c>behaviour</c> field. The call site is a constrained call on the
        /// struct's address — no box. Non-void returns yield <c>default</c> on a
        /// type mismatch.
        /// </summary>
        private static void EmitForwardBody(StringBuilder sb, IMethodSymbol m, string rootName, Scope scope)
        {
            string ret = scope.Display(m.ReturnType);
            bool isVoid = m.ReturnsVoid;

            sb.Append("        public ").Append(isVoid ? "void" : ret).Append(' ')
              .Append(m.Name).Append('(');
            AppendParamList(sb, m, widenFirstTo: rootName, scope);
            sb.AppendLine(")");
            sb.AppendLine("        {");

            string firstName = m.Parameters[0].Name;
            sb.Append("            if (").Append(firstName).AppendLine(" is TData typed)");
            sb.Append("            ").Append(isVoid ? "    " : "    return ")
              .Append("behaviour.").Append(m.Name).Append('(');
            AppendArgList(sb, m, firstReplacement: "typed");
            sb.AppendLine(");");

            if (!isVoid)
                sb.AppendLine("            return default;");

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        // -------------------------------------------------------------------- emit: self-mapping

        private static MappingModel MatchStruct(
            INamedTypeSymbol st, Dictionary<INamedTypeSymbol, DomainModel> domains)
        {
            // Abstract types are not instantiable and must not be mapped. The
            // syntactic predicate already drops 'abstract', but an intermediate
            // base could reach here through other means — guard semantically too.
            if (st.IsAbstract) return null;

            foreach (var iface in st.AllInterfaces)
            {
                if (!domains.TryGetValue(iface.OriginalDefinition, out var domain)) continue;
                if (iface.TypeArguments.Length != 1) continue;

                // The data type argument must be fully closed (a real type), not an
                // open type parameter inherited from a still-generic base.
                if (iface.TypeArguments[0] is ITypeParameterSymbol) continue;

                return new MappingModel
                {
                    Struct = st,
                    DataType = iface.TypeArguments[0],
                    Domain = domain,
                    Namespace = st.ContainingNamespace.IsGlobalNamespace
                        ? null : st.ContainingNamespace.ToDisplayString(),
                };
            }
            return null;
        }

        private static void EmitSelfMapping(SourceProductionContext spc, MappingModel m)
        {
            var scope = new Scope(m.Namespace);
            scope.Collect(m.DataType);
            // The container class + bucket interface live in the domain namespace;
            // collect that namespace so a using is emitted when it differs.
            if (m.Domain.Namespace != null)
                scope.AddNamespace(m.Domain.Namespace);

            string dataName = scope.Display(m.DataType);
            string containerClosed = $"{m.Domain.ContainerClass}<{dataName}, {m.Struct.Name}>";
            string bucket = m.Domain.ContainerIface;

            var sb = Header();
            HeaderWithUsings(sb, scope, m.Namespace, out bool hasNs);

            string keyword = BuildPartialKeyword(m.Struct);
            sb.Append("    ").Append(keyword).Append(' ').Append(m.Struct.Name)
              .Append(" : global::PFE.Core.DataMapping.ISelfMapping<").Append(dataName).AppendLine(">");
            sb.AppendLine("    {");
            sb.AppendLine("        public void BuildAndRegister()");
            sb.Append("            => global::PFE.Core.DataMapping.DomainBucket<")
              .Append(bucket).Append(">.Add(new ").Append(containerClosed).AppendLine("(this));");
            sb.AppendLine("    }");

            CloseNs(sb, hasNs);
            spc.AddSource($"{m.Struct.Name}.Mapping", sb.ToString());
        }

        // -------------------------------------------------------------------- emit: bootstrap

        private static void EmitBootstrap(
            SourceProductionContext spc, Compilation compilation, List<MappingModel> regs)
        {
            if (regs.Count == 0) return;

            string asm = compilation.AssemblyName ?? "Assembly";
            string typeName = SafeIdent(asm) + "_MappingBootstrap";

            // The bootstrap aggregates the whole assembly, so simple-name
            // collisions are most likely here; let the Scope resolve them.
            var scope = new Scope("PFE.Core.DataMapping.Generated");
            foreach (var r in regs)
            {
                scope.Collect(r.DataType);
                scope.Collect(r.Struct);
                if (r.Domain.Namespace != null) scope.AddNamespace(r.Domain.Namespace);
            }

            var buckets = regs
                .Select(r => r.Domain.ContainerIface)
                .Distinct()
                .ToList();

            var sb = Header();
            string usings = scope.UsingsBlock();
            if (usings.Length > 0) sb.Append(usings).AppendLine();
            sb.AppendLine("namespace PFE.Core.DataMapping.Generated");
            sb.AppendLine("{");
            sb.Append("    internal static class ").AppendLine(typeName);
            sb.AppendLine("    {");
            sb.AppendLine("        [global::UnityEngine.RuntimeInitializeOnLoadMethod(global::UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]");
            sb.AppendLine("        private static void Initialize()");
            sb.AppendLine("        {");
            foreach (var b in buckets)
                sb.Append("            global::PFE.Core.DataMapping.DomainBucket<")
                  .Append(b).AppendLine(">.Clear();");
            sb.AppendLine();
            foreach (var r in regs)
            {
                string dataName = scope.Display(r.DataType);
                string selfName = scope.Display(r.Struct);
                sb.Append("            global::PFE.Core.DataMapping.Mapper.Register<")
                  .Append(dataName).Append(", ").Append(selfName).AppendLine(">();");
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            spc.AddSource($"{SafeIdent(asm)}.Bootstrap", sb.ToString());
        }

        // -------------------------------------------------------------------- helpers

        /// <summary>
        /// Per-file name resolver. Collects the namespaces of every referenced
        /// type, emits one <c>using</c> per namespace, and renders types by their
        /// short name — falling back to a <c>global::</c>-qualified name only when
        /// a simple name is ambiguous across the collected namespaces. This keeps
        /// generated code readable without risking the wrong type binding.
        /// </summary>
        private sealed class Scope
        {
            // simple name -> set of full namespaces that declare a referenced type with that name
            private readonly Dictionary<string, HashSet<string>> bySimple = new();
            private readonly HashSet<string> namespaces = new();
            private readonly string ownNamespace;

            public Scope(string ownNamespace) => this.ownNamespace = ownNamespace;

            /// <summary>Adds a namespace to the using set without registering a type.</summary>
            public void AddNamespace(string ns)
            {
                if (!string.IsNullOrEmpty(ns)) namespaces.Add(ns);
            }

            /// <summary>Records a type (and its generic args) as referenced in this file.</summary>
            public void Collect(ITypeSymbol type)
            {
                switch (type)
                {
                    case INamedTypeSymbol named:
                        if (!named.ContainingNamespace.IsGlobalNamespace)
                        {
                            string ns = named.ContainingNamespace.ToDisplayString();
                            namespaces.Add(ns);
                            if (!bySimple.TryGetValue(named.Name, out var set))
                                bySimple[named.Name] = set = new HashSet<string>();
                            set.Add(ns);
                        }
                        foreach (var arg in named.TypeArguments) Collect(arg);
                        break;
                    case IArrayTypeSymbol arr:
                        Collect(arr.ElementType);
                        break;
                    // type parameters (TData, TController, …) need no using
                }
            }

            /// <summary>True if the simple name resolves unambiguously among collected namespaces.</summary>
            private bool IsAmbiguous(INamedTypeSymbol named) =>
                bySimple.TryGetValue(named.Name, out var set) && set.Count > 1;

            /// <summary>Renders a type: short name if safe, else global::-qualified.</summary>
            public string Display(ITypeSymbol type)
            {
                if (type is IArrayTypeSymbol arr) return Display(arr.ElementType) + "[]";
                if (type is ITypeParameterSymbol) return type.Name;

                if (type is INamedTypeSymbol named)
                {
                    string name = IsAmbiguous(named)
                        ? named.ToDisplayString(GlobalFormat)        // global:: + full ns
                        : named.Name;

                    if (named.TypeArguments.Length > 0 && !named.IsUnboundGenericType)
                    {
                        var args = string.Join(", ", named.TypeArguments.Select(Display));
                        // strip any generic suffix the display added, then re-close
                        int lt = name.IndexOf('<');
                        if (lt >= 0) name = name.Substring(0, lt);
                        return name + "<" + args + ">";
                    }
                    return name;
                }
                return type.ToDisplayString(GlobalFormat);
            }

            /// <summary>The using block (sorted, excluding own namespace), or empty.</summary>
            public string UsingsBlock()
            {
                var sb = new StringBuilder();
                foreach (var ns in namespaces.Where(n => n != ownNamespace).OrderBy(n => n))
                    sb.Append("using ").Append(ns).AppendLine(";");
                return sb.ToString();
            }
        }

        // global:: + fully qualified, used only for ambiguous or fallback cases.
        private static readonly SymbolDisplayFormat GlobalFormat =
            SymbolDisplayFormat.FullyQualifiedFormat;

        private static StringBuilder Header()
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine();
            return sb;
        }

        /// <summary>Writes header + usings + namespace open. Call after the Scope is fully collected.</summary>
        private static void HeaderWithUsings(StringBuilder sb, Scope scope, string ns, out bool hasNs)
        {
            string usings = scope.UsingsBlock();
            if (usings.Length > 0) sb.Append(usings).AppendLine();
            OpenNs(sb, ns, out hasNs);
        }

        private static void OpenNs(StringBuilder sb, string ns, out bool hasNs)
        {
            hasNs = ns != null;
            if (hasNs) { sb.Append("namespace ").AppendLine(ns); sb.AppendLine("{"); }
        }

        private static void CloseNs(StringBuilder sb, bool hasNs)
        {
            if (hasNs) sb.AppendLine("}");
        }

        /// <summary>Method signature with the first parameter widened to the data root.</summary>
        private static string SignatureWidened(IMethodSymbol m, string rootName, Scope scope)
        {
            var sb = new StringBuilder();
            sb.Append(m.ReturnsVoid ? "void" : scope.Display(m.ReturnType)).Append(' ').Append(m.Name).Append('(');
            AppendParamList(sb, m, widenFirstTo: rootName, scope);
            sb.Append(')');
            return sb.ToString();
        }

        private static void AppendParamList(StringBuilder sb, IMethodSymbol m, string widenFirstTo, Scope scope)
        {
            for (int i = 0; i < m.Parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var p = m.Parameters[i];
                sb.Append(RefPrefix(p.RefKind));
                sb.Append(i == 0 ? widenFirstTo : scope.Display(p.Type));
                sb.Append(' ').Append(p.Name);
            }
        }

        private static void AppendArgList(StringBuilder sb, IMethodSymbol m, string firstReplacement)
        {
            for (int i = 0; i < m.Parameters.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                var p = m.Parameters[i];
                sb.Append(RefArgPrefix(p.RefKind));
                sb.Append(i == 0 ? firstReplacement : p.Name);
            }
        }

        private static string RefPrefix(RefKind k) => k switch
        {
            RefKind.In => "in ",
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            _ => "",
        };

        // At the call site, 'in' may be passed implicitly, but being explicit is safe for ref/out.
        private static string RefArgPrefix(RefKind k) => k switch
        {
            RefKind.In => "in ",
            RefKind.Out => "out ",
            RefKind.Ref => "ref ",
            _ => "",
        };

        /// <summary>
        /// The partial declaration keyword matching the concrete type: 'partial
        /// struct', 'readonly partial struct', 'partial class', or a record form.
        /// The generated partial must agree with the user's own declaration.
        /// </summary>
        private static string BuildPartialKeyword(INamedTypeSymbol t)
        {
            if (t.TypeKind == TypeKind.Struct)
            {
                if (t.IsRecord) return t.IsReadOnly ? "readonly partial record struct" : "partial record struct";
                return t.IsReadOnly ? "readonly partial struct" : "partial struct";
            }
            // class
            if (t.IsRecord) return "partial record";
            return "partial class";
        }

        private static string SafeIdent(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var c in s) sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            return sb.ToString();
        }
    }
}
