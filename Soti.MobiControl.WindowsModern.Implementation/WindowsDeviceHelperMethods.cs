using System;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation;

internal static class WindowsDeviceHelperMethods
{
    internal static void ValidateNetworkDetailsEnum(WindowsModernSnapshot windowsModernSnapshot)
    {
        IsValidEnumValue(windowsModernSnapshot.WifiModeId);
        IsValidEnumValue(windowsModernSnapshot.NetworkAuthenticationId);
        IsValidEnumValue(windowsModernSnapshot.WirelessLanModeId);
    }

    internal static void IsValidEnumValue<TEnum>(TEnum value) where TEnum : Enum
    {
        if (!Enum.IsDefined(typeof(TEnum), value))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    internal static TEnum ToEnum<TEnum>(this byte value) where TEnum : Enum
    {
        return !Enum.IsDefined(typeof(TEnum), value)
            ? throw new ArgumentOutOfRangeException(nameof(value))
            : (TEnum)Enum.ToObject(typeof(TEnum), value);
    }

    internal static void ValidateDataRetrievalOptions(int skip, int take)
    {
        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }
    }
}
