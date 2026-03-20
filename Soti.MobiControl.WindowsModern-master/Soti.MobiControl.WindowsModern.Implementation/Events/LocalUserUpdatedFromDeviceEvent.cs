using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Implementation.Events
{
    [EventMetadata("LocalUserUpdatedFromDevice", "Local user updated from device", "Local user \"{0}\" changed from the device.", AlertManagerType.Device)]
    internal sealed class LocalUserUpdatedFromDeviceEvent : DeviceOriginatedEvent, IDeviceBoundEvent, IParameterizedEvent
    {
        internal LocalUserUpdatedFromDeviceEvent(int deviceId, string devId, string userName)
            : base(deviceId, devId, EventSeverity.Information)
        {
            EventAdditionalParameters = new[] { userName };
        }

        public string[] EventAdditionalParameters { get; }
    }
}
