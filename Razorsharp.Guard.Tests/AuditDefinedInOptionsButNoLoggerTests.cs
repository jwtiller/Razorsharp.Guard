using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard.Tests
{
    [TestFixture]
    public class AuditDefinedInOptionsButNoLoggerTests
    {
        [Test]
        public void AuditDefinedInOptionsButNoLogger_ShouldThrowException()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                new ClassificationFilter(
            new GuardOptions()
            {
                GuardMode = GuardMode.Audit,
                Audit = (logger, context, evt) => { }
            }, null);
            });

            Assert.That(exception.Message, Is.EqualTo($"Razorsharp Guard has auditing enabled in options, but no logger is registered in the dependency injection container."));
        }
    }
}
