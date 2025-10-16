namespace Razorsharp.Guard.Entities
{
    public record GuardEvent
    (
        IReadOnlyList<ClassificationResult> Classifications
    );
}
