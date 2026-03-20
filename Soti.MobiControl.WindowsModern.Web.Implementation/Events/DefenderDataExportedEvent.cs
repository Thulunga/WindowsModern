using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Events
{
    /// <summary>
    /// Represents Defender Data Export Success Event.
    /// </summary>
    [EventMetadata("DefenderDataExported", "Windows Defender Data exported", "Windows Defender Data exported", AlertManagerType.System)]
    internal sealed class DefenderDataExportedEvent : UserOriginatedEvent, IDeviceBoundEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefenderDataExportedEvent"/> class.
        /// </summary>
        /// <param name="userPrincipalId">Id of the user who requested defender report.</param>
        /// <param name="userName">Name of the user who requested defender report.</param>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="devId">The Dev Id.</param>
        public DefenderDataExportedEvent(int userPrincipalId, string userName, int deviceId, string devId)
            : base(userPrincipalId, userName, EventSeverity.Information)
        {
            DeviceId = deviceId;
            DevId = devId;
        }

        public int DeviceId { get; }

        public string DevId { get; }
    }
}
