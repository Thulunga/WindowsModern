using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Defines operations for managing boot priority data for Windows devices.
/// </summary>
internal interface IWindowsDeviceBootPriorityDataProvider
{
    /// <summary>
    /// Retrieves boot priority data for a list of devices using the bulk get stored procedure.
    /// </summary>
    /// <param name="deviceIds">A collection of unique device identifiers.</param>
    /// <returns>A read-only list of <see cref="WindowsDeviceBootPriority"/> representing the boot priorities for the specified devices.</returns>
    IReadOnlyList<WindowsDeviceBootPriority> BulkGetByDeviceId(IEnumerable<int> deviceIds);

    /// <summary>
    /// Deletes all boot priority entries associated with the specified device.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the device whose boot priority entries should be deleted.</param>
    void DeleteByDeviceId(int deviceId);

    /// <summary>
    /// Modifies boot priority entries for multiple devices using a bulk update operation.
    /// </summary>
    /// <param name="data">A collection of <see cref="WindowsDeviceBootPriority"/> representing the updated boot priority information.</param>
    void BulkModify(IEnumerable<WindowsDeviceBootPriority> data);

    /// <summary>
    /// Retrieves boot priority data for a specific device.
    /// </summary>
    /// <param name="deviceId">The unique identifier of the device to retrieve data for.</param>
    /// <returns>A read-only list of <see cref="WindowsDeviceBootPriority"/> associated with the specified device.</returns>
    IReadOnlyList<WindowsDeviceBootPriority> GetByDeviceId(int deviceId);

    /// <summary>
    /// Modifies boot priority entries for a specific device using a bulk update operation.
    /// </summary>
    /// <param name="data">A collection of <see cref="WindowsDeviceBootPriority"/> representing the updated boot priority data for the device.</param>
    /// <param name="deviceId">The unique identifier of the device to apply the modifications to.</param>
    void BulkModifyForDevice(IEnumerable<WindowsDeviceBootPriority> data, int deviceId);

    /// <summary>
    /// Performs a bulk insert of the given boot priority names and returns the resulting records.
    /// </summary>
    /// <param name="priorityNames">A list of boot priority names to insert.</param>
    /// <returns>A list of <see cref="WindowsBootPriority"/> containing the inserted or existing records.</returns>
    IReadOnlyList<WindowsBootPriority> BulkInsertAndGet(IEnumerable<string> priorityNames);

    /// <summary>
    /// Retrieves a list of Windows boot priority entries based on the specified priority IDs.
    /// </summary>
    /// <param name="priorityIds">A collection of boot priority IDs to retrieve.</param>
    /// <returns>A read-only list of <see cref="WindowsBootPriority"/> corresponding to the provided IDs.</returns>
    IReadOnlyList<WindowsBootPriority> BulkGetByIds(IEnumerable<short> priorityIds);
}
