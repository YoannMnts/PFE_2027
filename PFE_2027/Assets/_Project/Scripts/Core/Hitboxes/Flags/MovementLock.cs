using System;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // In a window: no movement while it is open. In the Flags of a data: no movement during the whole action.
    [Serializable]
    public struct MovementLock : IWindowFlag, IDataFlag
    {
        
    }
}