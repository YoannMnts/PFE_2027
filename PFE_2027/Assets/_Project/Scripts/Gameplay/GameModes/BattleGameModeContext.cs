namespace PFE.Gameplay.Scripts.GameModes
{
    public struct BattleGameModeContext
    {
        public readonly BattleGameMode battleGameMode;

        public BattleGameModeContext(BattleGameMode battleGameMode)
        {
            this.battleGameMode = battleGameMode;
        }
    }
}