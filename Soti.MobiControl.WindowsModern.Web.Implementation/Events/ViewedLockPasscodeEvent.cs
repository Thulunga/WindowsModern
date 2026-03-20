using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Events
{
    [EventMetadata("ViewedLockPasscode", "Requested to View Lock Passcode", "{0} has requested to view the Lock Passcode", AlertManagerType.Device)]
    internal sealed class ViewedLockPasscodeEvent : UserOriginatedEvent, IDeviceBoundEvent, IParameterizedEvent
    {
        internal ViewedLockPasscodeEvent(int userPrincipalId, string userName, int deviceId, string devId)
            : base(userPrincipalId, userName, EventSeverity.Information)
        {
            EventAdditionalParameters = new[] { userName };
            DeviceId = deviceId;
            DevId = devId;
        }

        public string[] EventAdditionalParameters { get; }

        public int DeviceId { get; }

        public string DevId { get; }
    }
}
