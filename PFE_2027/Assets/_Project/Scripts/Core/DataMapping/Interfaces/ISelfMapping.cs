namespace PFE.Core.Scripts.DataMapping.Interfaces
{
    /// <summary>
    /// The structural link across every registrable behaviour. A behaviour that
    /// implements this knows how to build its own concrete container and push it
    /// into the right domain bucket. Because both are read from the behaviour
    /// TYPE, the global <see cref="Mapper.Register{TData,TBehaviour}()"/> infers
    /// both type arguments from its argument — the caller never names a domain.
    ///
    /// <para>Normally emitted by the source generator; you don't hand-write it.</para>
    /// </summary>
    public interface ISelfMapping<in TData> where TData : IData
    {
        void BuildAndRegister();
    }
}
