using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Converters;

internal static class WindowsDeviceDataConverter
{
    public static WindowsDeviceData ToWindowsDeviceData(WindowsDeviceModel deviceModel)
    {
        return new WindowsDeviceData
        {
            DataKeyId = null,
            DeviceId = deviceModel.DeviceId,
            IsLocked = deviceModel.IsLocked,
            Passcode = null,
            LastCheckInDeviceUserTime = null,
            HardwareId = null,
        };
    }
}
