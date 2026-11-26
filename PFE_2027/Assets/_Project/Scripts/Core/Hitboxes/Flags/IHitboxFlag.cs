namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Any flag of a HitboxData. It is only data: the code using it decides when it applies.
    // IWindowFlag: placed in a window, active while the clip time is inside it.
    // IDataFlag: placed on the whole data, active during the whole action.
    // A flag implementing both can be placed in either (HitboxRunner.Has looks at both).
    public interface IHitboxFlag
    {
    }
}
