using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks.Sources;
using Microsoft.Extensions.Logging.Abstractions;
using Snapshooter.NUnit;
using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard.Tests
{
    [TestFixture]
    public class PropertyPathTests
    {
        private List<string> _audits = new();
        private ClassificationFilter CreateFilter() => new ClassificationFilter(
            new GuardOptions() { 
            GuardMode = GuardMode.ThrowExceptionAndCallback,
            Callback = (logger,context,evt) => {
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
        public class ProductDTO
        {
            [Public]
            public string Name { get; set; } = string.Empty;

            [Public]
            public Manufacturer Manufacturer { get; set; } = new();
        }

        [Internal("Contains supplier contact information.")]
        public class Manufacturer
        {
            [Public]
            public string CompanyName { get; set; } = string.Empty;

            [Restricted("Supplier address may reveal internal logistics location.")]
            public FactoryAddress FactoryAddress { get; set; } = new();
        }

        [Restricted("Physical factory address classified as internal data.")]
        public class FactoryAddress
        {
            [Confidential("Street information is not for public disclosure.")]
            public string Street { get; set; } = string.Empty;

            [Public]
            public string City { get; set; } = string.Empty;
        }


        [Test]
        public void Propertyname()
        {
            var filter = CreateFilter();
            var context = CreateContext(new ProductDTO());

            Assert.Throws<RazorsharpGuardException>(() => filter.OnResultExecuting(context));

            filter.Classifications.MatchSnapshot();
        }
    }
}
