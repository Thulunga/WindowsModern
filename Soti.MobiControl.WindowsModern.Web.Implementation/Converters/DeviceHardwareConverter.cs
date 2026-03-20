using System;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

/// <summary>
/// Device Hardware Converter
/// </summary>
internal static class DeviceHardwareConverter
{
    /// <summary>
    /// Converter Class DeviceHardwareModel to DeviceHardwareSummary.
    /// </summary>
    /// <param name="deviceHardwareModel">Device Hardware Model.</param>
    /// <returns>DeviceHardwareSummary.</returns>
    public static Contracts.DeviceHardwareSummary ToDeviceHardwareSummary(Models.DeviceHardwareSummary deviceHardwareModel)
    {
        return deviceHardwareModel == null
            ? throw new ArgumentNullException(nameof(deviceHardwareModel))
            : new Contracts.DeviceHardwareSummary
            {
                HardwareName = deviceHardwareModel.HardwareName,
                HardwareManufacturerName = deviceHardwareModel.HardwareManufacturerName,
                HardwareStatus = (HardwareStatus)deviceHardwareModel.HardwareStatus,
                DeviceHardwareType = (DeviceHardwareType)deviceHardwareModel.DeviceHardwareType,
                HardwareSerialNumber = deviceHardwareModel.HardwareSerialNumber
            };
    }
}
