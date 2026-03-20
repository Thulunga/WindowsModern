using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

[EventMetadata("LoggedInUserFailure", "Failure in Logged-In user information", "Failed to process the logged-in user information", AlertManagerType.Device)]
internal sealed class LoggedInUserFailureEvent : DeviceOriginatedEvent
{
    /// <summary>
    /// Logged-In User Failure Event.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="devId">The dev id.</param>
    internal LoggedInUserFailureEvent(int deviceId, string devId)
        : base(deviceId, devId, EventSeverity.Error)
    {
    }
}
