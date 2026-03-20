using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

/// <summary>
/// WindowsMultiLocalUsersActionEvent.
/// </summary>
[EventMetadata("WindowsMultiLocalUsersAction", "Windows Multi Local Users Action", "{0} action will be performed on the following users: {1}.", AlertManagerType.Device)]
internal sealed class WindowsMultiLocalUsersActionEvent : DeviceOriginatedEvent, IParameterizedEvent
{
    /// <summary>
    /// WindowsMultiLocalUsersActionEvent.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="devId">Dev Id.</param>
    /// <param name="action">Action.</param>
    /// <param name="users">Users.</param>
    internal WindowsMultiLocalUsersActionEvent(int deviceId, string devId, string action, string users)
        : base(deviceId, devId, EventSeverity.Information)
    {
        EventAdditionalParameters = [action, users];
    }

    /// <summary>
    /// EventAdditionalParameters.
    /// </summary>
    public string[] EventAdditionalParameters { get; }
}
