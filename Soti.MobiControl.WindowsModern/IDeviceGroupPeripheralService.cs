using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// The Device Group Peripheral Service interface.
/// </summary>
public interface IDeviceGroupPeripheralService
{
    /// <summary>
    /// Return the summary of device group peripheral for all type of device family.
    /// </summary>
    /// <param name="deviceGroupIds">The unique identifier of device group.</param>
    IReadOnlyList<DeviceGroupPeripheralSummary> GetDeviceGroupsPeripheralSummary(IReadOnlyCollection<int> deviceGroupIds);

    /// <summary>
    /// Return the summary of device group peripheral for a specific device family.
    /// </summary>
    /// <param name="deviceGroupIds">The unique identifier of device groups.</param>
    /// <param name="deviceFamilyId">The device family id.</param>
    IReadOnlyList<DeviceGroupPeripheralSummary> GetPeripheralSummaryByFamilyIdAndGroupIds(int deviceFamilyId, IReadOnlyCollection<int> deviceGroupIds);
}
