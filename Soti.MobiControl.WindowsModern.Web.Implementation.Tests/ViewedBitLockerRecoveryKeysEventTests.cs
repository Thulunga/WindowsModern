using System;
using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Web.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests
{
    [TestFixture]
    internal class ViewedBitLockerRecoveryKeysEventTests
    {
        [Test]
        public void ViewedBitLockerRecoveryKeyEventTests_Constructor()
        {
            var devId = Guid.NewGuid().ToString();
            var testEvent = new ViewedBitLockerRecoveryKeysEvent(1, "admin", 100001, devId);
            Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
            Assert.That(testEvent.UserName, Is.EqualTo("admin"));
            Assert.That(testEvent.UserPrincipalId, Is.EqualTo(1));
            Assert.That(testEvent.DevId, Is.EqualTo(devId));
            Assert.That(testEvent.DeviceId, Is.EqualTo(100001));
            Assert.That(testEvent.EventAdditionalParameters, Is.EqualTo(new[] { "admin" }));
        }

    }
}
