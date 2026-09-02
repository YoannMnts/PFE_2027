using Helteix.Tools.Phases;

namespace PFE.Core.Scripts.GameModes
{
    public interface IGameMode : IPhase{}
    
    public abstract class GameMode<T> : Phase<T>, IGameMode
    {
        
    }
}