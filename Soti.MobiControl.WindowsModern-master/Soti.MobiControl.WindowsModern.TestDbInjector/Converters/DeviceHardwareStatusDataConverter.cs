using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.TestDbInjector.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Soti.MobiControl.WindowsModern.TestDbInjector.Converters;

/// <summary>
/// DeviceHardwareStatusData Converter
/// </summary>
[ExcludeFromCodeCoverage]
internal static class DeviceHardwareStatusDataConverter
{
    /// <summary>
    /// Convert DeviceHardwareStatusData to DeviceHardwareStatus
    /// </summary>
    /// <param name="deviceHardwareData">Parameter of type DeviceHardwareStatusData class</param>
    /// <returns>DeviceHardwareStatus object</returns>
    public static DeviceHardwareStatus ToDeviceHardwareStatus(DeviceHardwareStatusData deviceHardwareData)
    {
        return deviceHardwareData == null
            ? throw new ArgumentNullException(nameof(deviceHardwareData))
            : new DeviceHardwareStatus
            {
                DeviceHardwareId = deviceHardwareData.DeviceHardwareId,
                DeviceHardwareSerialNumber = deviceHardwareData.DeviceHardwareSerialNumber,
            };
    }
}
