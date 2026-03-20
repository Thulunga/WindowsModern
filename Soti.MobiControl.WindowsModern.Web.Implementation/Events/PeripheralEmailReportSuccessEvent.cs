using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Events;

/// <summary>
/// Represents the Peripheral Listing Email Success Event.
/// </summary>
[EventMetadata(
    "PeripheralEmailReportSuccess",
    "Peripheral Email Report Success Event",
    "Device groups peripheral listing CSV file emailed successfully to {0}.",
    AlertManagerType.System)]
internal sealed class PeripheralEmailReportSuccessEvent : UserOriginatedEvent, IParameterizedEvent, IDeviceGroupBoundEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PeripheralEmailReportSuccessEvent" /> class.
    /// </summary>
    /// <param name="recipients">Emails of Recipient.</param>
    /// <param name="userId">User Id.</param>
    /// <param name="userName"> User Name.</param>
    /// <param name="groupId">The device group id.</param>
    public PeripheralEmailReportSuccessEvent(string recipients, int userId, string userName, int groupId)
        : base(userId, userName, EventSeverity.Information)
    {
        EventAdditionalParameters = new[] { recipients };
        DeviceGroupId = groupId;
    }

    /// <summary>
    /// Gets additional parameters array.
    /// </summary>
    public string[] EventAdditionalParameters { get; }

    public int DeviceGroupId { get; }
}
