using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Events;

/// <summary>
/// Represents the Peripheral Download Report Success Event.
/// </summary>
[EventMetadata(
    "PeripheralDownloadReportSuccess",
    "Peripheral Download Report Success Event",
    "Device groups peripheral listing CSV downloaded successfully.",
    AlertManagerType.System)]
internal sealed class PeripheralDownloadReportSuccessEvent : UserOriginatedEvent, IDeviceGroupBoundEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PeripheralDownloadReportSuccessEvent" /> class.
    /// </summary>
    /// <param name="userId">User Id.</param>
    /// <param name="userName"> User Name.</param>
    /// <param name="groupId">The device group id.</param>
    public PeripheralDownloadReportSuccessEvent(int userId, string userName, int groupId)
        : base(userId, userName, EventSeverity.Information)
    {
        DeviceGroupId = groupId;
    }

    public int DeviceGroupId { get; }
}
