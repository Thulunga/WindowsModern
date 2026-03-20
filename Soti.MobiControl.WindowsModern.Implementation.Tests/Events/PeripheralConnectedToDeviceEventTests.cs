using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Events;
using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Events;

[TestFixture]
internal sealed class PeripheralConnectedToDeviceEventTests
{
    [Test]
    public void PeripheralConnectedToDeviceEvent_Tests()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string peripheralName = "Name";
        const string manufacturer = "manufacturer";
        var testEvent = new PeripheralConnectedToDeviceEvent(deviceId, devId, peripheralName, manufacturer);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(manufacturer));
        Assert.That(testEvent.EventAdditionalParameters[1], Is.EqualTo(peripheralName));
    }
}
