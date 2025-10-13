namespace Razorsharp.Guard
{

    public class RazorsharpGuardException : Exception
    {
        public IReadOnlyList<ClassificationResult> Evaluations { get; }

        public RazorsharpGuardException(string message,
            IReadOnlyList<ClassificationResult> evaluations)
            : base(message)
        {
            Evaluations = evaluations;
        }
    }
}
