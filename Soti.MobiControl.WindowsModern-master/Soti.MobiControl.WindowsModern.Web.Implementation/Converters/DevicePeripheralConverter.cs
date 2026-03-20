using System;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

/// <summary>
/// Converts to device peripheral summary.
/// </summary>
internal static class DevicePeripheralConverter
{
    /// <summary>
    /// DevicePeripheralSummary Converter.
    /// </summary>
    /// <param name="devicePeripheralSummary">The Local User Model.</param>
    /// <returns>DevicePeripheralSummary.</returns>
    public static Contracts.DevicePeripheralSummary ToDevicePeripheralSummary(this Models.DevicePeripheralSummary devicePeripheralSummary)
    {
        if (devicePeripheralSummary == null)
        {
            throw new ArgumentNullException(nameof(devicePeripheralSummary));
        }

        return new Contracts.DevicePeripheralSummary
        {
            PeripheralName = devicePeripheralSummary.Name,
            PeripheralManufacturer = devicePeripheralSummary.Manufacturer,
            PeripheralVersion = devicePeripheralSummary.Version,
            PeripheralStatus = (DevicePeripheralStatus)devicePeripheralSummary.Status,
            PeripheralType = (PeripheralType)devicePeripheralSummary.PeripheralType
        };
    }
}
