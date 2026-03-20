using System;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Converters;

/// <summary>
/// DeviceHardwareDataConverter.
/// </summary>
internal static class DeviceHardwareDataConverter
{
    /// <summary>
    /// Converter class for Device Hardware Data to Device Hardware Model.
    /// </summary>
    /// <param name="deviceHardwareData">Device Hardware Data.</param>
    /// <returns>Device Hardware Model</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static DeviceHardwareSummary ToDeviceHardwareModel(DeviceHardwareData deviceHardwareData)
    {
        return deviceHardwareData == null
            ? throw new ArgumentNullException(nameof(deviceHardwareData))
            : new DeviceHardwareSummary
            {
                HardwareManufacturerName = deviceHardwareData.DeviceHardwareManufacturerName,
                HardwareName = deviceHardwareData.DeviceHardwareName,
                HardwareSerialNumber = deviceHardwareData.DeviceHardwareSerialNumber,
                HardwareStatus = (HardwareStatus)deviceHardwareData.HardwareStatusId,
                DeviceHardwareType = (DeviceHardwareType)deviceHardwareData.DeviceHardwareTypeId,
            };
    }
}
