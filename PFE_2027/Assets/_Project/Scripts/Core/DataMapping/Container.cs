using System;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Core.Scripts.DataMapping
{
    /// <summary>
    /// Generic container: the behaviour is stored in its CONCRETE type, so a
    /// struct lives inline and is never boxed. The reference held by the store is
    /// this container (a class), not the behaviour.
    /// </summary>
    public abstract class Container<TData, TBehaviour> : IContainer<TData>
        where TData : IData
        where TBehaviour : IBehaviour<TData>
    {
        protected TBehaviour behaviour; // concrete type -> inline, no box
        protected Container(TBehaviour behaviour) => this.behaviour = behaviour;
        public Type DataType => typeof(TData);
    }
}
