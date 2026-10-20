namespace PFE.Gameplay.Scripts.Interactions
{
    public interface IInteractable<out T>
    {
        public T Interactable { get; }
    }
}