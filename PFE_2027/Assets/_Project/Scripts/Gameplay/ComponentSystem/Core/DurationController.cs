using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    //TODO faire en sorte qu'il devienne un manager du temps et uniquement pdt la BattlePhase
    public class DurationController
    {
        public int Remaining => remainingCount;
        
        private int remainingCount;

        public DurationController() : this(0)
        {
        }

        public DurationController(int remainingCount)
        {
            this.remainingCount = remainingCount;
        }

        public void AddOrRemove(int newValue)
        {
            remainingCount += newValue;
        }
    }
}