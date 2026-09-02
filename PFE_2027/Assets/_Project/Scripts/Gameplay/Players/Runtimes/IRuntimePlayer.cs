namespace PFE.Gameplay.Scripts.Phases.Runtimes
{
    public interface IRuntimePlayer<out T>
    {
        public T Player { get; }
    }
}