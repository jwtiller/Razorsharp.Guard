using Razorsharp.Guard.Entities;

namespace HelloWorld.Common.Entities
{
    [Public]
    public class BaseEmployeeDTO
    {
        [Public]
        public string Name { get; set; } = string.Empty;

        [Public]
        public string Department { get; set; } = string.Empty;
    }
}
