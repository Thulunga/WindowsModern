using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

[EventMetadata("LoggedInUserChangeDetected", "Detected the change in the Logged-In User", "Current user {0} has logged-in and last user checked was {1}", AlertManagerType.Device)]
internal sealed class LoggedInUserChangeDetectedEvent : DeviceOriginatedEvent, IParameterizedEvent
{
    /// <summary>
    /// Logged-In User Change Detected Event.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="devId">The dev id.</param>
    /// <param name="currentUserName">The current username.</param>
    /// <param name="currentUserFullName">The current user full name.</param>
    /// <param name="previousUserName">The previous username.</param>
    /// <param name="previousUserFullName">The previous user full name.</param>
    internal LoggedInUserChangeDetectedEvent(int deviceId, string devId, string currentUserName, string currentUserFullName, string previousUserName, string previousUserFullName)
        : base(deviceId, devId, EventSeverity.Information)
    {
        var currentName = string.IsNullOrEmpty(currentUserFullName) ? currentUserName : string.Format("{0} ({1})", currentUserName, currentUserFullName);
        var previousName = string.IsNullOrEmpty(previousUserFullName) ? previousUserName : string.Format("{0} ({1})", previousUserName, previousUserFullName);
        EventAdditionalParameters = new[] { currentName, previousName };
    }

    public string[] EventAdditionalParameters { get; }
}
