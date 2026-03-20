using System;
using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using Soti.MobiControl.WindowsModern.Web.Enums;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

/// <summary>
/// The Device Group Peripheral Filter Helper.
/// </summary>
internal sealed class DeviceGroupPeripheralFilterHelper
{
    /// <summary>
    /// Filter the device group peripheral summary.
    /// </summary>
    /// <param name="entities">The device group peripheral summary.</param>
    /// <param name="searchString">Peripheral name search filter.e</param>
    /// <param name="statuses">Peripheral status search filter.</param>
    /// <returns></returns>
    public static IEnumerable<DeviceGroupPeripheralSummary> Filter(
        IEnumerable<DeviceGroupPeripheralSummary> entities,
        string searchString,
        IEnumerable<DevicePeripheralStatus> statuses)
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (!string.IsNullOrEmpty(searchString))
        {
            entities = entities.Where(e => e.Name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase));
        }

        var statusList = statuses?.ToArray();
        if (statusList != null && statusList?.Length != 0)
        {
            return entities.Where(peripheral =>
                statusList.Contains(peripheral.Status));
        }

        return entities;
    }
}
