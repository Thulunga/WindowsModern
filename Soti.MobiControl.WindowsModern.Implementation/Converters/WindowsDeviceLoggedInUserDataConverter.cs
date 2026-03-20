using System;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Converters;

internal static class WindowsDeviceLoggedInUserDataConverter
{
    public static WindowsDeviceUserData ToWindowsDeviceUserData(this WindowsDeviceLoggedInUserModel loggedInUserData, int dataKeyId)
    {
        if (loggedInUserData == null)
        {
            throw new ArgumentNullException(nameof(loggedInUserData));
        }

        return new WindowsDeviceUserData
        {
            WindowsDeviceUserId = loggedInUserData.WindowsDeviceUserId,
            DeviceId = loggedInUserData.DeviceId,
            UserName = loggedInUserData.UserName,
            UserFullName = loggedInUserData.UserFullName,
            UserDomain = string.IsNullOrEmpty(loggedInUserData.UserDomain) ? null : loggedInUserData.UserDomain,
            UserSID = loggedInUserData.UserSID,
            IsUserLoggedIn = loggedInUserData.IsUserLoggedIn,
            WindowsDeviceUserType = loggedInUserData.WindowsDeviceUserType,
            CreatedDate = DateTime.UtcNow,
            DataKeyId = dataKeyId
        };
    }

    public static WindowsDeviceLoggedInUserModel ToWindowsDeviceUserModel(this WindowsDeviceUserData windowsDeviceUserData)
    {
        if (windowsDeviceUserData == null)
        {
            throw new ArgumentNullException(nameof(windowsDeviceUserData));
        }

        return new WindowsDeviceLoggedInUserModel
        {
            WindowsDeviceUserId = windowsDeviceUserData.WindowsDeviceUserId,
            DeviceId = windowsDeviceUserData.DeviceId,
            UserName = windowsDeviceUserData.UserName,
            UserFullName = windowsDeviceUserData.UserFullName,
            UserDomain = windowsDeviceUserData.UserDomain,
            UserSID = windowsDeviceUserData.UserSID,
            WindowsDeviceUserType = windowsDeviceUserData.WindowsDeviceUserType,
            LastCheckInDeviceUserTime = windowsDeviceUserData.LastCheckInDeviceUserTime
        };
    }
}
