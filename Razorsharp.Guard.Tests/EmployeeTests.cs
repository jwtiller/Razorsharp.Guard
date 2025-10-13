using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks.Sources;
using Microsoft.Extensions.Logging.Abstractions;

namespace Razorsharp.Guard.Tests
{
    [TestFixture]
    public class EmployeeTests
    {
        private List<string> _audits = new();
        private ClassificationFilter CreateFilter() => new ClassificationFilter(
            new GuardOptions() { 
            GuardMode = GuardMode.Audit,
            Audit = (logger,context,evt) => {
                var details = string.Join(',',evt.Classifications.Select(c => $"{c.Type}[{c.SensitivityLevel}={c.Reason}]"));
                _audits.Add($"Sensitive data accessed: {details}"); }
            }, new NullLogger<ClassificationFilter>());

        private ResultExecutingContext CreateContext(object dto)
        {
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor(),
                new ModelStateDictionary()
            );

            return new ResultExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new ObjectResult(dto),
                controller: null
            );
        }

        [Public]
        class BaseEmployeeDTO
        {
            [Public]
            public string Name { get; set; } = string.Empty;

            [Public]
            public string Department { get; set; } = string.Empty;
        }

        [Confidential("Contains salary and private employee information.")]
        class FullEmployeeDTO : BaseEmployeeDTO
        {
            [Restricted("Salary is confidential HR data.")]
            public decimal Salary { get; set; }

            [Confidential("Personal email address under GDPR.")]
            public string Email { get; set; } = string.Empty;
        }

        [Test]
        public void BasicEmployeeDto_ShouldNotAudit()
        {
            var filter = CreateFilter();
            var context = CreateContext(new BaseEmployeeDTO());

            Assert.DoesNotThrow(() => filter.OnResultExecuting(context));
            Assert.That(filter.Classifications.Any(c => c.SensitivityLevel > SensitivityLevel.Public), Is.False);
            Assert.That(_audits.Count, Is.EqualTo(0));
        }

        [Test]
        public void FullEmployeeDto_ShouldAudit()
        {
            var filter = CreateFilter();
            var context = CreateContext(new FullEmployeeDTO());

            Assert.DoesNotThrow(() => filter.OnResultExecuting(context));
            Assert.That(filter.Classifications.Any(c => c.SensitivityLevel > SensitivityLevel.Public), Is.True);
            Assert.That(_audits.Count, Is.GreaterThan(0));
        }
    }
}
