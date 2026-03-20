using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Events
{
    [EventMetadata("ViewedBitLockerRecoveryKeys", "Administrator has requested to view the BitLocker Recovery Key(s)", "Administrator has requested to view the BitLocker Recovery Key(s)", AlertManagerType.Device)]
    internal sealed class ViewedBitLockerRecoveryKeysEvent : UserOriginatedEvent, IDeviceBoundEvent, IParameterizedEvent
    {
        internal ViewedBitLockerRecoveryKeysEvent(int userPrincipalId, string userName, int deviceId, string devId)
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
