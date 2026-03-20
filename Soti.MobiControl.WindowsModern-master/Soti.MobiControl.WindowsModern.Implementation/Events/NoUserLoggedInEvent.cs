using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

[EventMetadata("NoUserLoggedIn", "No user is Logged-In", "Last user checked was {0}", AlertManagerType.Device)]
internal sealed class NoUserLoggedInEvent : DeviceOriginatedEvent, IParameterizedEvent
{
    /// <summary>
    /// No User Logged-In Event.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="devId">The dev id.</param>
    /// <param name="previousUserName">The previous username.</param>
    /// <param name="previousUserFullName">The previous user full name.</param>
    internal NoUserLoggedInEvent(int deviceId, string devId, string previousUserName, string previousUserFullName)
        : base(deviceId, devId, EventSeverity.Information)
    {
        var previousName = string.IsNullOrEmpty(previousUserFullName) ? previousUserName : string.Format("{0} ({1})", previousUserName, previousUserFullName);
        EventAdditionalParameters = new[] { previousName };
    }

    public string[] EventAdditionalParameters { get; }
}
