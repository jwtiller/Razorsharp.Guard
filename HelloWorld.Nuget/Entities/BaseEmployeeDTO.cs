using Razorsharp.Guard;

namespace HelloWorld.Nuget.Entities
{
    public class BaseEmployeeDTO
    {
        [Public]
        public string Name { get; set; } = string.Empty;

        [Public]
        public string Department { get; set; } = string.Empty;
    }
}
