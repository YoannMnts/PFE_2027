using System;

namespace PFE.Core.DataMapping
{
    /// <summary>
    /// Marks a method on a <c>[GenerateContainer]</c> behaviour interface as one
    /// that should be exposed on the generated container.
    /// <para>
    /// Only methods carrying this attribute are surfaced on the container
    /// interface and forwarded by the concrete container; every other method on
    /// the behaviour interface (helpers, default-interface-method logic) stays
    /// internal to the behaviour and is neither exposed nor validated.
    /// </para>
    /// <para>
    /// A marked method must take the behaviour's data type as its first parameter
    /// (so the container can widen it to the data root and downcast). Unmarked
    /// methods are free to take any signature.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AddToContainerAttribute : Attribute
    {
    }
}
