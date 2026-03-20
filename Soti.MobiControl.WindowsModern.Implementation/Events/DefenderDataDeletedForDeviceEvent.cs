using System;
using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events;

[EventMetadata("DefenderDataDeletedForDevice", "Defender Data deleted for the device", "Defender Data deleted for the device \"{0}\"", AlertManagerType.Device)]
internal sealed class DefenderDataDeletedForDeviceEvent : SystemOriginatedEvent, IDeviceBoundEvent, IParameterizedEvent
{
    internal DefenderDataDeletedForDeviceEvent(int deviceId, string devId)
        : base(EventSeverity.Information)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId), @"Device Id must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(devId))
        {
            throw new ArgumentException(@"DevId must be specified.", nameof(devId));
        }

        DevId = devId;
        DeviceId = deviceId;
        EventAdditionalParameters = new[] { devId };
    }

    public string[] EventAdditionalParameters { get; }

    public int DeviceId { get; }

    public string DevId { get; }
}
