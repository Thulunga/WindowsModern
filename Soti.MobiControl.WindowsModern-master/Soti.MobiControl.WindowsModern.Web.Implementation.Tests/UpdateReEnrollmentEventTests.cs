using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests
{
    [TestFixture]
    internal class UpdateReEnrollmentEventTests
    {
        [Test]
        public void UpdateReEnrollmentEventTests_Constructor()
        {
            var testEvent = new UpdateReEnrollmentEvent(1, "admin", 0, "enabled");
            Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
            Assert.That(testEvent.UserName, Is.EqualTo("admin"));
            Assert.That(testEvent.UserPrincipalId, Is.EqualTo(1));
            Assert.That(testEvent.EventAdditionalParameters, Is.EqualTo(new[] { "enabled" }));
        }
    }
}
