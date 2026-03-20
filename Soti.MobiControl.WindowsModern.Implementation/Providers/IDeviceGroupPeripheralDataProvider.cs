using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for device group peripheral.
/// </summary>
internal interface IDeviceGroupPeripheralDataProvider
{
    /// <summary>
    /// Get device group peripherals.
    /// </summary>
    /// <param name="deviceGroupIds">The unique identifier of device group.</param>
    IReadOnlyList<DeviceGroupPeripheralSummary> GetDeviceGroupPeripherals(IEnumerable<int> deviceGroupIds);

    /// <summary>
    /// Get device group peripherals.
    /// </summary>
    /// <param name="deviceGroupIds">The unique identifier of device group.</param>
    /// <param name="deviceFamilyId">The device family id.</param>
    /// <returns></returns>
    IReadOnlyList<DeviceGroupPeripheralSummary> GetPeripheralSummaryByFamilyIdAndGroupIds(int deviceFamilyId, IEnumerable<int> deviceGroupIds);
}
