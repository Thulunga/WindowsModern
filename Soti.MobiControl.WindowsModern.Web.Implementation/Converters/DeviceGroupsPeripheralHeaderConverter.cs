using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Web.Contracts;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters;

/// <summary>
/// DeviceGroupsPeripheralHeaderConverter.
/// </summary>
internal static class DeviceGroupsPeripheralHeaderConverter
{
    /// <summary>
    /// Convert it to exportable dictionary.
    /// </summary>
    /// <param name="report">Device group peripheral Record.</param>
    /// <param name="timeZoneOffset">Time Zone offset.</param>
    public static IDictionary<string, string> ToExportableDictionary(DeviceGroupsPeripheralHeaders report, int timeZoneOffset = 0)
    {
        ArgumentNullException.ThrowIfNull(report);

        var serializableDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        AddProperty(serializableDictionary, nameof(report.Name), report.Name, DefaultConverter);
        AddProperty(serializableDictionary, nameof(report.Status), report.Status, DefaultConverter);
        AddProperty(serializableDictionary, nameof(report.PeripheralType), report.PeripheralType, DefaultConverter);
        AddProperty(serializableDictionary, nameof(report.Manufacturer), report.Manufacturer, DefaultConverter);
        AddProperty(serializableDictionary, nameof(report.Version), report.Version, DefaultConverter);
        AddProperty(serializableDictionary, nameof(report.DeviceCount), report.DeviceCount, DefaultConverter);

        return serializableDictionary;
    }

    private static string DefaultConverter<TValue>(TValue value)
    {
        return value.ToString();
    }

    private static void AddProperty<TValue>(IDictionary<string, string> dictionary, string node, TValue value, Func<TValue, string> converter)
    {
        if (!EqualityComparer<TValue>.Default.Equals(value, default(TValue)))
        {
            dictionary.Add(node, converter(value));
        }
    }
}
