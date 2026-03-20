using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

/// <summary>
/// PeripheralDisconnectedFromDeviceEvent.
/// </summary>
[EventMetadata("PeripheralDisconnectedFromDevice", "Peripheral disconnected from device", "'{0}', '{1}' has been disconnected from the Windows device.", AlertManagerType.Device)]

internal sealed class PeripheralDisconnectedFromDeviceEvent : DeviceOriginatedEvent, IParameterizedEvent
{
    /// <summary>
    /// PeripheralDisconnectedFromDeviceEvent.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="devId">The dev id.</param>
    /// <param name="peripheralName">The peripheral name.</param>
    /// <param name="manufacturer">The manufacturer.</param>
    internal PeripheralDisconnectedFromDeviceEvent(int deviceId, string devId, string peripheralName, string manufacturer)
        : base(deviceId, devId, EventSeverity.Information)
    {
        EventAdditionalParameters = new[] { manufacturer, peripheralName };
    }

    public string[] EventAdditionalParameters { get; }
}
