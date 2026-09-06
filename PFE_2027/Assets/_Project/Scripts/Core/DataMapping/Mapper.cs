namespace PFE.Core.DataMapping
{
    /// <summary>
    /// The single global entry point. No domain is named by the caller: the
    /// behaviour routes itself via <see cref="ISelfMapping{TData}"/>.
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Register a data->behaviour mapping. The struct behaviour is placed into
        /// a concrete-typed container field, never boxed.
        /// </summary>
        public static void Register<TData, TBehaviour>(TBehaviour behaviour)
            where TData : IData
            where TBehaviour : IBehaviour<TData>, ISelfMapping<TData>
            => behaviour.BuildAndRegister();

        /// <summary>Convenience for parameterless behaviours.</summary>
        public static void Register<TData, TBehaviour>()
            where TData : IData
            where TBehaviour : IBehaviour<TData>, ISelfMapping<TData>, new()
            => Register<TData, TBehaviour>(new TBehaviour());

        /// <summary>
        /// O(1) dispatch - a single dictionary lookup.
        /// </summary>
        public static bool TryGet<TContainer>(this IData data, out TContainer container)
            where TContainer : class, IContainer
            => DomainBucket<TContainer>.TryGet(data, out container);
    }
}
