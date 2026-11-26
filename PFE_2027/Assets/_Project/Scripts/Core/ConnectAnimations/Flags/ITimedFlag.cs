namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Flag active only while the normalized clip time is inside its Window (Hitbox, Combo, Displacement...)
    public interface ITimedFlag : IAnimationFlag
    {
        TimeWindow Window { get; }
    }
}
