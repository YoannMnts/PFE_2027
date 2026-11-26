namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Any flag of a AnimationData, all in its single Flags list. A flag without window (LoopAnimation, ChargeUp,
    // ChargeTime) is active during the whole action; an ITimedFlag only while the clip time is inside its window.
    public interface IAnimationFlag
    {
    }
}
