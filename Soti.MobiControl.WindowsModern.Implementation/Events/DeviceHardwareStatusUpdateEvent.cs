using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;
using Soti.MobiControl.Events;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

/// <summary>
/// DeviceHardwareStatusUpdateEvent.
/// </summary>
[EventMetadata("DeviceHardwareStatusUpdate", "Device hardware status updated", "{0} {1} {2} serial number {3}", AlertManagerType.Device)]
internal sealed class DeviceHardwareStatusUpdateEvent : DeviceOriginatedEvent, IParameterizedEvent
{
    /// <summary>
    /// DeviceHardwareStatusUpdateEvent.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="devId">Dev Id.</param>
    /// <param name="userName">Mobi Control User Name.</param>
    /// <param name="userAction">Update Device Hardware User Action (Acknowledged/Rejected).</param>
    /// <param name="hardwareType">Device Hardware Type.</param>
    /// <param name="deviceHardwareSerialNumber">Device Hardware Serial Number.</param>
    internal DeviceHardwareStatusUpdateEvent(int deviceId, string devId, string userName, string userAction, string hardwareType, string deviceHardwareSerialNumber)
        : base(deviceId, devId, EventSeverity.Information)
    {
        EventAdditionalParameters = [userName, userAction, hardwareType, deviceHardwareSerialNumber];
    }

    /// <summary>
    /// EventAdditionalParameters.
    /// </summary>
    public string[] EventAdditionalParameters { get; }
}
