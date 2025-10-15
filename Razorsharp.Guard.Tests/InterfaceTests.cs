using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using static Razorsharp.Guard.Tests.PropertyPathTests;
using Snapshooter.NUnit;

namespace Razorsharp.Guard.Tests
{
    [TestFixture]
    public class InterfaceTests
    {
        private List<string> _audits = new();
        private ClassificationFilter CreateFilter() => new ClassificationFilter(
            new GuardOptions()
            {
                GuardMode = GuardMode.ThrowException,
                Audit = (logger, context, evt) => {
                    var details = string.Join(',', evt.Classifications.Select(c => $"{c.Type}[{c.SensitivityLevel}={c.Reason}]"));
                    _audits.Add($"Sensitive data accessed: {details}");
                }
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

        [Test]
        public void Should_TriggerException_When_InterfaceImplementationIsRestricted()
        {
            var filter = CreateFilter();
            var context = CreateContext(new UserDTO
            {
                Name = "Alice Example",
                Contact = new ContactInfo
                {
                    Email = "alice@example.com",
                    Phone = "+47 900 00 000"
                },
                Address = new Address
                {
                    Street = "Testveien 42",
                    ZipCode = "1234"
                }
            });

            Assert.Throws<RazorsharpGuardException>(() => filter.OnResultExecuting(context));

            filter.Classifications.MatchSnapshot();
        }

        [Test]
        public void ShouldNot_TriggerException_WhenInterfaceAndImplementationPropertyIsPublic()
        {
            var filter = CreateFilter();
            var context = CreateContext(new Lorem());
            Assert.DoesNotThrow(() => filter.OnResultExecuting(context));
            filter.Classifications.MatchSnapshot();
        }

        [Test]
        public void Should_TriggerException_WhenImplementationHasRestrictedProperty()
        {
            var filter = CreateFilter();
            var context = CreateContext(new Lorem2());
            Assert.Throws<RazorsharpGuardException>(() => filter.OnResultExecuting(context));
            filter.Classifications.MatchSnapshot();
        }
    }

    public interface IContactInfo
    {
        [Restricted("email interface restricted")]
        string Email { get; set; }
        string Phone { get; set; }
    }

    [Public]
    public class Address
    {
        [Public]
        public string Street { get; set; }

        [Restricted("May reveal location")]
        public string ZipCode { get; set; }
    }

    [Public]
    public class UserDTO
    {
        [Public]
        public string Name { get; set; }

        [Restricted("Contains personal info")]
        public IContactInfo Contact { get; set; }

        [Public]
        public Address Address { get; set; }
    }

    [Restricted("Implements restricted contact data")]
    public class ContactInfo : IContactInfo
    {
        public string Email { get; set; }

        [Confidential("Phone is confidential")]
        public string Phone { get; set; }
    }

    public interface IHello
    {
        [Public]
        public string World { get; set; }
    }

    public class Lorem : IHello
    {
        [Public]
        public string World { get; set; }
    }

    public interface IHello2 : IHello
    {
    }

    public class Lorem2 : Lorem
    {
        [Restricted("this is restricted")]
        public string Restricted { get; set; }
    }
}
