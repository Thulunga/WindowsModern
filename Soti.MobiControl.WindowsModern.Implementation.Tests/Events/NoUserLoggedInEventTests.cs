using System;
using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Events;

[TestFixture]
internal class NoUserLoggedInEventTests
{
    [Test]
    public void NoUserLoggedInEvent_Test()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string username = "UserName";
        var testEvent = new NoUserLoggedInEvent(deviceId, devId, username, null);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(username));
    }

    [Test]
    public void NoUserLoggedInEvent_FormatNameTest()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string username = "UserName";
        const string userFullName = "UserFullName";
        const string formattedName = username + " (" + userFullName + ")";
        var testEvent = new NoUserLoggedInEvent(deviceId, devId, username, userFullName);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(formattedName));
    }
}
