using HelloWorld.Common.Entities;
using HelloWorld.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HelloWorld.Controllers
{
    public class EmployeesController : ControllerBase
    {
        private readonly ILogger<EmployeesController> _logger;
        public EmployeesController(ILogger<EmployeesController> logger)
        {
            _logger = logger;
        }

        // basic data
        [HttpGet("public")]
        public ActionResult<BaseEmployeeDTO> GetPublicEmployee()
        {
            var employee = new BaseEmployeeDTO
            {
                Name = "Alice Jensen",
                Department = "Engineering"
            };

            return Ok(employee);
        }

        // contains sensitive data
        [HttpGet("internal")]
        public ActionResult<FullEmployeeDTO> GetFullEmployee()
        {
            var employee = new FullEmployeeDTO
            {
                Name = "Alice Jensen",
                Department = "Engineering",
                Salary = 850000,
                Email = "alice@company.no"
            };

            _logger.LogInformation("Returning employee {@employee}", employee);

            return Ok(employee);
        }

        [HttpGet("internalasync")]
        public async Task<ActionResult<FullEmployeeDTO>> GetPublicEmployeAsync()
        {
            await Task.Delay(10);
            var employee = new FullEmployeeDTO
            {
                Name = "Alice Jensen",
                Department = "Engineering",
                Salary = 850000,
                Email = "alice@company.no"
            };

            return employee;
        }
    }
}
