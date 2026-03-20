using NUnit.Framework;
using Soti.MobiControl.Events;
using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Events;

internal sealed class DeviceHardwareRemovedSynchronizationEventTest
{
    [Test]
    public void DeviceHardwareRemovedSynchronizationEvent_Test()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string hardwareType = "RAM";
        const string deviceHardwareSerialNumber = "RAM001";
        var testEvent = new Implementation.Events.DeviceHardwareRemovedSynchronizationEvent(deviceId, devId, hardwareType, deviceHardwareSerialNumber);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(hardwareType));
        Assert.That(testEvent.EventAdditionalParameters[1], Is.EqualTo(deviceHardwareSerialNumber));
    }
}
