using System;

namespace PFE.Core.Scripts.DataMapping.Interfaces
{
    /// <summary>
    /// Non-generic root handle. The store is keyed by the domain container type;
    /// this root carries only the data type for the inner lookup. Domains derive
    /// a richer interface that adds business ops — never exposing the behaviour,
    /// which is what keeps a struct behaviour unboxed.
    /// </summary>
    public interface IContainer
    {
        Type DataType { get; }
    }

    public interface IContainer<in TData> : IContainer where TData : IData { }
}
