using System;
using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Events;

[TestFixture]
internal class LoggedInUserFailureEventTests
{
    [Test]
    public void LoggedInUserFailureEvent_Test()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        var testEvent = new LoggedInUserFailureEvent(deviceId, devId);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Error));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
    }
}
