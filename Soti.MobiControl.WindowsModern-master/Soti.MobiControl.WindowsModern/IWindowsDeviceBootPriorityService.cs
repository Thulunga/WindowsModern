using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Provides methods for managing BIOS boot priority data associated with Windows devices.
/// </summary>
public interface IWindowsDeviceBootPriorityService
{
    /// <summary>
    /// Deletes all boot priority records associated with the specified device.
    /// </summary>
    /// <param name="deviceId">The ID of the device.</param>
    void DeleteByDeviceId(int deviceId);

    /// <summary>
    /// Retrieves all boot priority records for the specified device.
    /// </summary>
    /// <param name="deviceId">The ID of the device.</param>
    /// <returns>A list of boot priority records.</returns>
    IReadOnlyList<WindowsDeviceBootPriority> GetByDeviceId(int deviceId);

    /// <summary>
    /// Modifies boot priority data in bulk for multiple devices.
    /// </summary>
    /// <param name="data">The boot priority data to modify.</param>
    void BulkModify(IEnumerable<WindowsDeviceBootPriority> data);

    /// <summary>
    /// Modifies boot priority data in bulk for a specific device.
    /// </summary>
    /// <param name="data">The boot priority data to modify.</param>
    /// <param name="deviceId">The ID of the device.</param>
    void BulkModifyForDevice(IEnumerable<WindowsDeviceBootPriority> data, int deviceId);

    /// <summary>
    /// Retrieves boot priority records for the specified list of device IDs.
    /// </summary>
    /// <param name="deviceIds">The device IDs to retrieve boot priority data for.</param>
    /// <returns>A list of boot priority records.</returns>
    IReadOnlyList<WindowsDeviceBootPriority> BulkGetByDeviceId(IEnumerable<int> deviceIds);

    /// <summary>
    /// Inserts or retrieves boot priority definitions by name.
    /// </summary>
    /// <param name="priorityNames">The names of the boot priorities.</param>
    /// <returns>A list of boot priority definitions.</returns>
    IReadOnlyList<WindowsBootPriority> BulkInsertAndGet(IEnumerable<string> priorityNames);

    /// <summary>
    /// Retrieves boot priority definitions by their IDs.
    /// </summary>
    /// <param name="priorityIds">The IDs of the boot priorities.</param>
    /// <returns>A list of boot priority definitions.</returns>
    IReadOnlyList<WindowsBootPriority> BulkGetByIds(IEnumerable<short> priorityIds);
}
