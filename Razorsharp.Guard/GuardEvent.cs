namespace Razorsharp.Guard
{
    public record GuardEvent
    (
        IReadOnlyList<ClassificationResult> Classifications
    );
}
