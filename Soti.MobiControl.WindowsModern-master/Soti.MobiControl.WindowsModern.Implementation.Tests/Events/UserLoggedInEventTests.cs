using System;
using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Events;

[TestFixture]
internal class UserLoggedInEventTests
{
    [Test]
    public void UserLoggedInEvent_Test()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string previousUserName = "UserName";
        var testEvent = new UserLoggedInEvent(deviceId, devId, previousUserName, null);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(previousUserName));
    }

    [Test]
    public void UserLoggedInEvent_FormatNameTest()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string previousUserName = "UserName";
        const string previousUserFullName = "UserFullName";
        const string formattedName = previousUserName + " (" + previousUserFullName + ")";
        var testEvent = new UserLoggedInEvent(deviceId, devId, previousUserName, previousUserFullName);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(formattedName));
    }
}
