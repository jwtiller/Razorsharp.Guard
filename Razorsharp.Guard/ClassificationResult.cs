namespace Razorsharp.Guard
{
    public class ClassificationResult
    {
        public string Type { get; set; }
        public string? Reason { get; set; }
        public SensitivityLevel SensitivityLevel { get; set; }
        public int Depth { get; set; }
        public AttributeLevel AttributeLevel { get; set; }
    }
}
