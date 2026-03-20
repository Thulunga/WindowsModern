using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Windows Device Groups Service.
/// </summary>
public interface IWindowsDeviceGroupsService
{
    /// <summary>
    /// Gets last sync status for a device group.
    /// </summary>
    /// <param name="deviceGroupId">The device group id.</param>
    /// <param name="syncRequestType">The sync request type.</param>
    /// <returns>Last sync status of the device group.</returns>
    DeviceGroupSyncInfo GetGroupSyncStatus(int deviceGroupId, SyncRequestType syncRequestType);
}
