using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Soti.MobiControl.WindowsModern.Implementation.Converters;

/// <summary>
/// Windows Device Configuration Converter.
/// </summary>
internal static class WindowsDeviceConfigurationConverter
{
    /// <summary>
    /// Deserialize settings string to type T.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T ToModel<T>(this string settings)
    {
        if (string.IsNullOrWhiteSpace(settings))
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        return JsonSerializer.Deserialize<T>(settings, options);
    }
}
