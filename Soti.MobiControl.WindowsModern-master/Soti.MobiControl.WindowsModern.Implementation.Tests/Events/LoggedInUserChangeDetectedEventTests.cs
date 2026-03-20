using System;
using NUnit.Framework;
using Soti.MobiControl.Events;
using Soti.MobiControl.WindowsModern.Implementation.Events;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests.Events;

[TestFixture]
internal class LoggedInUserChangeDetectedEventTests
{
    [Test]
    public void LoggedInUserChangeDetectedEvent_FormatTest()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string currentUserName = "CurrentName";
        const string currentUserFullName = "CurrentUserFullName";
        const string previousUserName = "PreviousUserName";
        const string previousUserFullName = "PreviousUserFullName";
        const string formattedPreviousName = previousUserName + " (" + previousUserFullName + ")";
        const string formattedCurrentName = currentUserName + " (" + currentUserFullName + ")";
        var testEvent = new LoggedInUserChangeDetectedEvent(deviceId, devId, currentUserName, currentUserFullName, previousUserName, previousUserFullName);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(formattedCurrentName));
        Assert.That(testEvent.EventAdditionalParameters[1], Is.EqualTo(formattedPreviousName));
    }

    [Test]
    public void LoggedInUserChangeDetectedEvent_Test()
    {
        const int deviceId = 100001;
        var devId = Guid.NewGuid().ToString();
        const string currentUserName = "CurrentName";
        const string previousUserName = "PreviousUserName";
        var testEvent = new LoggedInUserChangeDetectedEvent(deviceId, devId, currentUserName, null, previousUserName, null);
        Assert.That(testEvent.EventSeverity, Is.EqualTo(EventSeverity.Information));
        Assert.That(testEvent.DeviceId, Is.EqualTo(deviceId));
        Assert.That(testEvent.DevId, Is.EqualTo(devId));
        Assert.That(testEvent.EventAdditionalParameters[0], Is.EqualTo(currentUserName));
        Assert.That(testEvent.EventAdditionalParameters[1], Is.EqualTo(previousUserName));
    }
}
