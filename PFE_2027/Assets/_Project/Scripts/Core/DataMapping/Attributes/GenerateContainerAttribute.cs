using System;

namespace PFE.Core.Scripts.DataMapping.Attributes
{
    /// <summary>
    /// Placed on a domain's <b>behaviour interface</b>, e.g.
    /// <c>[GenerateContainer] interface ICapacityEffect&lt;in TData&gt; : IBehaviour&lt;TData&gt; where TData : IEffectData</c>.
    /// <para>
    /// This single annotation is all a domain author writes. From the decorated
    /// interface the source generator infers, by convention, every supporting
    /// type that would otherwise be hand-written:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Data root</b> = the <c>where TData :</c> constraint (e.g. <c>IEffectData</c>),
    ///     used as the type argument of the generated <c>IContainer&lt;TRoot&gt;</c>.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Domain root name</b> = the behaviour interface name with a leading
    ///     <c>I</c> stripped (e.g. <c>ICapacityEffect</c> → <c>CapacityEffect</c>).
    ///   </description></item>
    ///   <item><description>
    ///     <b>Container interface</b> <c>I{Root}Container</c>, re-declaring every
    ///     behaviour method with the data parameter widened to the data root.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Concrete container</b> <c>{Root}Container&lt;TData, TBehaviour&gt;</c>
    ///     deriving <c>Container&lt;TData, TBehaviour&gt;</c> and implementing the
    ///     container interface with a downcast-and-forward body per method.
    ///   </description></item>
    /// </list>
    /// <para>
    /// Only methods whose <b>first parameter type is exactly the behaviour's
    /// <c>TData</c></b> are forwarded with a downcast; other methods are outside
    /// the mapping contract and are not emitted on the container.
    /// </para>
    /// <para>
    /// The container interface is also used as the <see cref="DomainBucket{T}"/>
    /// key, so dispatch stays a single closed-generic lookup.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class GenerateContainerAttribute : Attribute
    {
        /// <summary>
        /// Optional explicit domain root name, overriding the
        /// strip-leading-<c>I</c> convention. Leave null to use the convention.
        /// </summary>
        public readonly string name;

        public GenerateContainerAttribute(string name = null)
        {
            this.name = name;
        }
    }
}
