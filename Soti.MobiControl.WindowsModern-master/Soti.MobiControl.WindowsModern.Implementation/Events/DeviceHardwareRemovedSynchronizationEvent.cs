using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

/// <summary>
/// DeviceHardwareRemovedSynchronizationEvent.
/// </summary>
[EventMetadata("DeviceHardwareRemovedSynchronization", "Device hardware removed synchronization", "{0} serial number {1} has been removed", AlertManagerType.Device)]
internal sealed class DeviceHardwareRemovedSynchronizationEvent : DeviceOriginatedEvent, IParameterizedEvent
{
    /// <summary>
    /// DeviceHardwareRemovedSynchronizationEvent.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="devId">Dev Id.</param>
    /// <param name="hardwareType">Hardware Type.</param>
    /// <param name="deviceHardwareSerialNumber">Device Hardware Serial Number.</param>
    internal DeviceHardwareRemovedSynchronizationEvent(int deviceId, string devId, string hardwareType, string deviceHardwareSerialNumber)
        : base(deviceId, devId, EventSeverity.Information)
    {
        EventAdditionalParameters = [hardwareType, deviceHardwareSerialNumber];
    }

    /// <summary>
    /// EventAdditionalParameters.
    /// </summary>
    public string[] EventAdditionalParameters { get; }
}
