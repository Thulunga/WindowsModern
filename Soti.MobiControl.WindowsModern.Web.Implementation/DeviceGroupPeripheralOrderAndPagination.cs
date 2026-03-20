using System;
using System.Collections.Generic;
using System.Linq;
using Soti.Api.Metadata.DataRetrieval;
using Soti.MobiControl.WindowsModern.Web.Contracts;

namespace Soti.MobiControl.WindowsModern.Web.Implementation;

internal sealed class DeviceGroupPeripheralOrderAndPagination
{
    private const string DefaultOrderByName = nameof(DeviceGroupPeripheralSummary.PeripheralType);
    private const bool DefaultDescOrder = false;
    private const int DefaultSkip = 0;
    private const int DefaultTake = 50;

    /// <summary>Applies the data retrieval options.</summary>
    /// <param name="policies">The rules.</param>
    /// <param name="options">The options.</param>
    /// <returns>
    ///  Returns a list of device peripherals.
    /// </returns>
    /// <exception cref="ArgumentException">Skip value should be greater than or equal to zero
    /// or
    /// Take value should be greater than or equal to zero.</exception>
    public static IEnumerable<DeviceGroupPeripheralSummary> ApplyDataRetrievalOptions(IEnumerable<DeviceGroupPeripheralSummary> policies,
        DataRetrievalOptions options)
    {
        var order = options?.Order ?? new[]
        {
            new DataRetrievalOrder
            {
                By = DefaultOrderByName,
                Descending = DefaultDescOrder
            }
        };

        var skip = options?.Skip ?? DefaultSkip;
        var take = options?.Take ?? DefaultTake;

        return policies
            .OrderMultipleBy(order.ToDictionary(o => o.By, o => o.Descending))
            .Skip(skip)
            .Take(take);
    }
}
