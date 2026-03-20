using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.Utilities.Extensions;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Device Group Peripheral Service class.
/// </summary>
internal sealed class DeviceGroupPeripheralService : IDeviceGroupPeripheralService
{
    private readonly IDeviceGroupPeripheralDataProvider _deviceGroupPeripheralDataProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceGroupPeripheralService"/> class.
    /// </summary>
    public DeviceGroupPeripheralService(IDeviceGroupPeripheralDataProvider deviceGroupPeripheralDataProvider)
    {
        _deviceGroupPeripheralDataProvider = deviceGroupPeripheralDataProvider
                                             ?? throw new ArgumentNullException(nameof(deviceGroupPeripheralDataProvider));
    }

    /// <inheritdoc cref = "IDeviceGroupPeripheralService" />
    public IReadOnlyList<DeviceGroupPeripheralSummary> GetDeviceGroupsPeripheralSummary(
        IReadOnlyCollection<int> deviceGroupIds)
    {
        ArgumentNullException.ThrowIfNull(deviceGroupIds);
        deviceGroupIds.DoForEach(id => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(deviceGroupIds)));

        return _deviceGroupPeripheralDataProvider.GetDeviceGroupPeripherals(deviceGroupIds);
    }

    /// <inheritdoc cref = "IDeviceGroupPeripheralService" />
    public IReadOnlyList<DeviceGroupPeripheralSummary> GetPeripheralSummaryByFamilyIdAndGroupIds(int deviceFamilyId, IReadOnlyCollection<int> deviceGroupIds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceFamilyId);
        ArgumentNullException.ThrowIfNull(deviceGroupIds);
        deviceGroupIds.DoForEach(id => ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id, nameof(deviceGroupIds)));

        return _deviceGroupPeripheralDataProvider.GetPeripheralSummaryByFamilyIdAndGroupIds(deviceFamilyId, deviceGroupIds);
    }
}
