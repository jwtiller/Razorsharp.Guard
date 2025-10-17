using HelloWorld.Common.Entities;
using Razorsharp.Guard.Entities;

namespace HelloWorld.Entities
{
    [Confidential("Contains salary and private employee information.")]
    public class FullEmployeeDTO : BaseEmployeeDTO
    {
        [Restricted("Salary is confidential HR data.")]
        public decimal Salary { get; set; }

        [Confidential("Personal email address under GDPR.")]
        public string Email { get; set; } = string.Empty;
    }
}
