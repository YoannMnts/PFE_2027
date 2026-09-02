using System;
using Helteix.Tools.Phases;

namespace PFE.Core.Scripts.GameModes
{
    public class GameModeController
    {
        public IGameMode Current { get; private set; }

        public event Action<IGameMode> OnGameModeBegins;
        public event Action<IGameMode> OnGameModeEnds;

        internal GameModeController()
        {
            
        }

        public void StartGameMode(IGameMode mode)
        {
            if (Current != null)
            {
                EndGameMode(mode);
                Current.Cancel();
            }
            Current = mode;
            OnGameModeBegins?.Invoke(mode);
        }

        public void EndGameMode(IGameMode mode)
        {
            if (Current == mode)
            {
                Current = null;
                OnGameModeEnds?.Invoke(mode);
            }
        }

        public bool CanStartGameMode() => Current == null;
    }
}