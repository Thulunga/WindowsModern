using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events
{
    [EventMetadata("LocalUserDeletedFromDevice", "Local user deleted from device", "Local user \"{0}\" deleted from the device.", AlertManagerType.Device)]

    internal sealed class LocalUserDeletedFromDeviceEvent : DeviceOriginatedEvent, IDeviceBoundEvent, IParameterizedEvent
    {
        internal LocalUserDeletedFromDeviceEvent(int deviceId, string devId, string userName)
            : base(deviceId, devId, EventSeverity.Information)
        {
            EventAdditionalParameters = new[] { userName };
        }

        public string[] EventAdditionalParameters { get; }
    }
}
