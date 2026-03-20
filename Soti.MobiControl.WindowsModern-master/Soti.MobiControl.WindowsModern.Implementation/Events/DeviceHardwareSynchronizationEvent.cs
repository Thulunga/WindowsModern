using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;
using Soti.MobiControl.Events;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

/// <summary>
/// DeviceHardwareSynchronizationEvent.
/// </summary>
[EventMetadata("DeviceHardwareSynchronization", "Device hardware synchronization event", "{0} serial number {1} has been {2}", AlertManagerType.Device)]
internal sealed class DeviceHardwareSynchronizationEvent : DeviceOriginatedEvent, IParameterizedEvent
{
    /// <summary>
    /// DeviceHardwareSynchronizationEvent.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="devId">Dev Id.</param>
    /// <param name="syncAction">Sync Action.</param>
    /// <param name="deviceHardwareSerialNumber">Device Hardware Serial Number.</param>
    /// <param name="action">Synchronization action Newly added or Removed</param>
    internal DeviceHardwareSynchronizationEvent(int deviceId, string devId, string syncAction, string deviceHardwareSerialNumber, string action)
        : base(deviceId, devId, EventSeverity.Information)
    {
        EventAdditionalParameters = [syncAction, deviceHardwareSerialNumber, action];
    }

    /// <summary>
    /// EventAdditionalParameters.
    /// </summary>
    public string[] EventAdditionalParameters { get; }
}
