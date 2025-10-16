using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Snapshooter.NUnit;
using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard.Tests
{
    [TestFixture]
    public class ClassificationFilterTests
    {
        private ClassificationFilter CreateFilter() => new ClassificationFilter(new GuardOptions() { GuardMode = GuardMode.ThrowException });

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


        public class UnclassifiedDTO
        {
            public string Value { get; set; } = "test";
        }

        [Restricted("All restricted")]
        public class RestrictedDTO
        {
            public string Value { get; set; } = "secret";
        }

        [Public]
        public class MixedDTO
        {
            [Restricted("Sensitive")]
            public string Secret { get; set; } = "123";

            [Public]
            public string Info { get; set; } = "OK";
        }

        [Confidential("Nested confidential data")]
        public class NestedConfidential
        {
            public string Secret { get; set; } = "conf";
        }

        [Public]
        public class WithNestedDTO
        {
            public NestedConfidential Nested { get; set; } = new();
        }

        [Public]
        public class WithListDTO
        {
            public List<NestedConfidential> Items { get; set; } = new() { new NestedConfidential() };
        }

        public class DeepNested
        {
            [Restricted("Deep restricted")]
            public string Hidden { get; set; } = "deep";
        }

        public class Contact
        {
            public DeepNested Inner { get; set; } = new();
        }

        [Public]
        public class DeepDTO
        {
            public Contact Contact { get; set; } = new();
        }

        [Public]
        public class NullableDTO
        {
            [Confidential("Optional data")]
            public string? Optional { get; set; } = null;
        }

        [Public]
        public class ClassPublicPropertyRestricted
        {
            [Restricted("Overrules class")]
            public string Secret { get; set; } = "123";
        }

        [Public]
        public class ClassPublicNoProperties
        {
        }

        public class Person { public string Name { get; set; } = "Test"; }

        [Public]
        public class ListUnclassifiedDTO
        {
            public List<Person> Persons { get; set; } = new() { new Person() };
        }

        [Public]
        public class AllPublicDTO
        {
            [Public]
            public string Name { get; set; } = "ok";
        }

        [Public]
        public class AllPublicNestedDTO
        {
            public AllPublicDTO Child { get; set; }
        }

        [Confidential("Class overrides")]
        public class ConfidentialClassDTO
        {
            [Public]
            public string Name { get; set; } = "ok";
        }

        [Public]
        public class EmptyListDTO
        {
            public List<NestedConfidential> Items { get; set; } = new(); // confidential list types
        }

        [Restricted("base")]
        public class BaseDTO
        {
        }

        [Public]
        public class SubDTO : BaseDTO
        {
        }

        [Public]
        public class PrimitiveTypesDTO
        {
            [Restricted("foo")]
            public int A { get; set; }

            public double B { get; set; }
        }

        [Restricted("foo")]
        public class RestrictedWithPublicPropertyDTO
        {
            [Public]
            public int A { get; set; }
        }

        [Restricted("foo")]
        public class RestrictedEmptyDTO
        {
        }

        [Confidential("foo")]
        public class ConfidentialEmptyDTO
        {
        }

        [Public]
        public class MixObjectsDTO
        {
            [Restricted("foo")]
            public List<object> A { get; set; }
        }

        [Restricted("foo")]
        public class CircularReferenceDTO
        {
            [Public]
            public CircularReferenceDTO A { get; set; }
        }

        [Public]
        public record RecordDTO
        {
            [Restricted("foo")]
            public string A { get; set; }
        }

        public class AllPropsArePublicClassShouldBeClassifiedPublicDTO
        {
            [Public]
            public string A { get; set; }

            [Public]
            public string B { get; set; }
        }

        public class OnePropIsPublicClassShouldBeClassifiedRestrictedDTO
        {
            [Public]
            public string A { get; set; }

            public string B { get; set; }
        }
            

        [TestCase(typeof(AllPublicNestedDTO))]
        [TestCase(typeof(AllPublicDTO))]
        [TestCase(typeof(ClassPublicNoProperties))]
        [TestCase(typeof(AllPropsArePublicClassShouldBeClassifiedPublicDTO))]
        public void Tests_ShouldNotThrowException(Type dto)
        {
            var filter = CreateFilter();
            var context = CreateContext(Activator.CreateInstance(dto));
            Assert.DoesNotThrow(() => filter.OnResultExecuting(context));
            filter.Classifications.MatchSnapshot();
        }



        [TestCase(typeof(RestrictedWithPublicPropertyDTO))]
        [TestCase(typeof(PrimitiveTypesDTO))]
        [TestCase(typeof(SubDTO))]
        [TestCase(typeof(EmptyListDTO))]
        [TestCase(typeof(ConfidentialClassDTO))]
        [TestCase(typeof(ListUnclassifiedDTO))]
        [TestCase(typeof(ClassPublicPropertyRestricted))]
        [TestCase(typeof(NullableDTO))]
        [TestCase(typeof(DeepDTO))]
        [TestCase(typeof(WithListDTO))]
        [TestCase(typeof(WithNestedDTO))]
        [TestCase(typeof(MixedDTO))]
        [TestCase(typeof(RestrictedDTO))]
        [TestCase(typeof(UnclassifiedDTO))]
        [TestCase(typeof(RestrictedEmptyDTO))]
        [TestCase(typeof(ConfidentialEmptyDTO))]
        [TestCase(typeof(MixObjectsDTO))]
        [TestCase(typeof(CircularReferenceDTO))]
        [TestCase(typeof(RecordDTO))]
        [TestCase(typeof(OnePropIsPublicClassShouldBeClassifiedRestrictedDTO))]
        public void Tests_ShouldThrowException(Type dto)
        {
            var filter = CreateFilter();
            var context = CreateContext(Activator.CreateInstance(dto));
            Assert.Throws<RazorsharpGuardException>(() => filter.OnResultExecuting(context));

            filter.Classifications.MatchSnapshot();
        }
    }
}