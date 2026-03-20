using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.TestDbInjector.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Soti.MobiControl.WindowsModern.TestDbInjector.Converters;

/// <summary>
/// DevicePeripheralData Converter
/// </summary>
[ExcludeFromCodeCoverage]
internal static class DevicePeripheralDataConverter
{
    /// <summary>
    /// Convert DevicePeripheralsData to WindowsDevicePeripheralData
    /// </summary>
    /// <param name="devicePeripheralsData">devicePeripheralsData object of type DevicePeripheralsData class</param>
    /// <returns>WindowsDevicePeripheralData object</returns>
    public static WindowsDevicePeripheralData ToWindowsDevicePeripheralData(DevicePeripheralsData devicePeripheralsData)
    {
        return devicePeripheralsData == null
            ? throw new ArgumentNullException(nameof(devicePeripheralsData))
            : new WindowsDevicePeripheralData
            {
                PeripheralId = devicePeripheralsData.PeripheralId,
                Version = devicePeripheralsData.Version,
            };
    }
}
