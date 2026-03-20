using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

[EventMetadata("UserLoggedIn", "User has Logged-In", "Current user {0} has logged-in", AlertManagerType.Device)]
internal sealed class UserLoggedInEvent : DeviceOriginatedEvent, IParameterizedEvent
{
    /// <summary>
    /// User Logged-In Event.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="devId">The dev id.</param>
    /// <param name="userName">The username.</param>
    /// <param name="userFullName">The user full name.</param>
    internal UserLoggedInEvent(int deviceId, string devId, string userName, string userFullName)
        : base(deviceId, devId, EventSeverity.Information)
    {
        var username = string.IsNullOrEmpty(userFullName) ? userName : string.Format("{0} ({1})", userName, userFullName);
        EventAdditionalParameters = new[] { username };
    }

    public string[] EventAdditionalParameters { get; }
}
