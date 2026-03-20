using NUnit.Framework;
using Soti.MobiControl.Events;
using System;
using Soti.MobiControl.WindowsModern.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Events;

internal sealed class DeviceHardwareSynchronizationEventTest
{
    [Test]
    public void HardwareConnectedToDeviceEvent_Test()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string syncAction = "A new hardware RAM";
        const string deviceHardwareSerialNumber = "RAM001";
        const string action = "added";
        var testEvent = new DeviceHardwareSynchronizationEvent(deviceId, devId, syncAction, deviceHardwareSerialNumber, action);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(syncAction));
        Assert.That(testEvent.EventAdditionalParameters[1], Is.EqualTo(deviceHardwareSerialNumber));
    }
}
