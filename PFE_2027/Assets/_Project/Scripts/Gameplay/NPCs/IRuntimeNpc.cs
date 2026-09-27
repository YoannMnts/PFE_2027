namespace PFE.Gameplay.Scripts.NPCs
{
    public interface IRuntimeNpc<TInstance> where TInstance : class, INpcInstance
    {
        public TInstance Instance { get; }
        public void Setup(TInstance instance);
    }
}
