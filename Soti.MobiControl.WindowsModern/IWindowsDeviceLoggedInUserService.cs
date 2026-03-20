using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Windows Device Logged-In User Service.
/// </summary>
public interface IWindowsDeviceLoggedInUserService
{
    /// <summary>
    /// Fetch the Windows Device Logged-In User Details by device id.
    /// </summary>
    /// <param name="deviceId">The device id of the device.</param>
    WindowsDeviceLoggedInUserModel GetWindowsDeviceLoggedInUserData(int deviceId);

    /// <summary>
    /// Process the Windows Device Logged-In User Details.
    /// </summary>
    /// <param name="windowsDeviceLoggedInUserModel">The windows logged-in user model.</param>
    void ProcessWindowsDeviceLoggedInUser(WindowsDeviceLoggedInUserModel windowsDeviceLoggedInUserModel);

    /// <summary>
    /// Modifies the IsUserLoggedIn flag to false.
    /// </summary>
    /// <param name="deviceId">The device id of the device.</param>
    void LogOffWindowsDeviceUser(int deviceId);

    /// <summary>
    /// Fetch the Windows Device Logged-In User Details by device ids.
    /// </summary>
    /// <param name="deviceIds">The collection of the device ids.</param>
    /// <returns>Collection of the windows device logged-in user data.</returns>
    IEnumerable<WindowsDeviceLoggedInUserModel> GetWindowsDeviceLoggedInUserDataByDeviceIds(IEnumerable<int> deviceIds);

    /// <summary>
    /// Clears the cache and notify other services.
    /// </summary>
    /// <param name="deviceId">The device id of the device.</param>
    /// <param name="notify">The value indicating whether to indicate other services.</param>
    void InvalidateLoggedInUserCache(int deviceId, bool notify);
}
