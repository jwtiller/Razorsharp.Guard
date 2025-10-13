using HelloWorld.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HelloWorld.Controllers
{
    public class EmployeesController : ControllerBase
    {
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

            return Ok(employee);
        }
    }
}
