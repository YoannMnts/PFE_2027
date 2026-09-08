namespace PFE.Gameplay.Scripts.Players.Runtime
{
    public interface IRuntimePlayer<out T>
    {
        public T Player { get; }
    }
}