using System;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

/// <summary>
/// Converter class to convert to DeviceLocalGroupSummary.
/// </summary>
internal static class WindowsDeviceLocalGroupConverter
{
    /// <summary>
    /// Converts to DeviceLocalGroupSummary contract.
    /// </summary>
    /// <param name="deviceLocalGroupSummary">The deviceLocalGroupSummary</param>
    /// <returns>DeviceLocalGroupSummary.</returns>
    internal static Contracts.DeviceLocalGroupSummary ToDeviceLocalGroupSummary(
        this Models.DeviceLocalGroupSummary deviceLocalGroupSummary)
    {
        return deviceLocalGroupSummary switch
        {
            null => throw new ArgumentNullException(nameof(deviceLocalGroupSummary)),
            _ => new Contracts.DeviceLocalGroupSummary { GroupName = deviceLocalGroupSummary.GroupName }
        };
    }
}
