using Soti.MobiControl.WindowsModern.Web.Contracts;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

/// <summary>
/// DeviceGroupsPeripheralConverter.
/// </summary>
internal static class DeviceGroupsPeripheralConverter
{
    /// <summary>
    /// Convert DeviceGroupPeripheralSummary data to DeviceGroupsPeripheralHeaders.
    /// </summary>
    /// <param name="data">The Device Group Peripheral Summary data.</param>
    /// <returns>The Device Groups Peripheral Headers data.</returns>
    public static DeviceGroupsPeripheralHeaders ToHeader(this DeviceGroupPeripheralSummary data)
    {
        return new DeviceGroupsPeripheralHeaders
        {
            Name = data.Name,
            PeripheralType = data.PeripheralType,
            DeviceCount = data.DeviceCount,
            Manufacturer = data.Manufacturer,
            Status = data.Status,
            Version = data.Version,
        };
    }
}
