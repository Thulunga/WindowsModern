using System;
using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Events;

internal sealed class DefenderDataDeletedForDeviceEventTests
{
    [Test]
    public void DefenderDataDeletedForDeviceEvent_Test()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        var testEvent = new DefenderDataDeletedForDeviceEvent(deviceId, devId);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(devId));
    }
}
