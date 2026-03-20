using System;
using System.Collections.Generic;
using Soti.MobiControl.Devices;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for WindowsDeviceData entity.
/// </summary>
internal interface IWindowsDeviceDataProvider
{
    /// <summary>
    /// Checks whether the device is Locked or not.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <returns>Lock status of the device.</returns>
    bool IsDeviceLocked(int deviceId);

    /// <summary>
    /// Sets IsSandBoxEnabled value of the device.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="isSandBoxEnabled">Is SandBox Enabled.</param>
    void UpdateWindowsSandBoxStatus(int deviceId, bool isSandBoxEnabled);

    /// <summary>
    /// Determines lock status for the specified device ids.
    /// </summary>
    /// <param name="deviceIds">HashSet of DeviceIds.</param>
    /// <returns>Lock status of devices.</returns>
    IReadOnlyDictionary<int, bool> AreDevicesLocked(HashSet<int> deviceIds);

    /// <summary>
    /// Determines status of sandbox for the specified device ids.
    /// </summary>
    /// <param name="deviceIds">List of DeviceIds.</param>
    /// <returns>SandBox status of devices.</returns>
    IReadOnlyDictionary<int, bool> GetSandBoxStatusByIds(IEnumerable<int> deviceIds);

    /// <summary>
    /// Gets Device Details for the specified device ids.
    /// </summary>
    /// <param name="deviceIds">List of DeviceIds.</param>
    /// <returns>Details of devices.</returns>
    IReadOnlyDictionary<int, WindowsDeviceData> BulkGetDeviceDetails(IEnumerable<int> deviceIds);

    /// <summary>
    /// Selects WindowsDeviceData entities.
    /// </summary>
    /// <returns>Collection of WindowsDeviceData entities.</returns>
    IReadOnlyCollection<WindowsDeviceData> GetAll();

    /// <summary>
    /// Selects WindowsDeviceData entities using filter.
    /// </summary>
    /// <param name="deviceId">DeviceId filter value.</param>
    /// <returns>Filtered WindowsDeviceData entity.</returns>
    WindowsDeviceData Get(int deviceId);

    /// <summary>
    /// Inserts WindowsDeviceData entity.
    /// </summary>
    /// <param name="value">Entity to insert.</param>
    void Insert(WindowsDeviceData value);

    /// <summary>
    /// Updates WindowsDeviceData entity.
    /// </summary>
    /// <param name="value">Entity to update.</param>
    void Update(WindowsDeviceData value);

    /// <summary>
    /// Updates targeted WindowsDeviceData entities.
    /// </summary>
    /// <param name="deviceIds">Device Ids to Bulk Update.</param>
    /// <param name="isLocked">Is Locked.</param>
    /// <param name="passcode">Passcode.</param>
    /// <param name="dataKeyId">Data Key Id.</param>
    void BulkUpdate(IEnumerable<int> deviceIds, bool isLocked, byte[] passcode, int? dataKeyId);

    /// <summary>
    /// Updates the LastCheckInDeviceUserTime for the Windows Logged-In User.
    /// </summary>
    /// <param name="deviceId">The Device Id.</param>
    /// <param name="lastCheckInTime">The Last Check In Device User Time.</param>
    void UpdateLastCheckInDeviceUserTime(int deviceId, DateTime lastCheckInTime);

    /// <summary>
    /// Updates the HardwareId for the Windows device.
    /// </summary>
    /// <param name="deviceId">The Device Id.</param>
    /// <param name="hardwareId">Hardware Id of the device.</param>
    void UpdateHardwareId(int deviceId, string hardwareId);

    /// <summary>
    /// Updates the WifiSubnet for the Windows device.
    /// </summary>
    /// <param name="deviceId">The Device Id.</param>
    /// <param name="wifiSubnet">WifiSubnet of the device.</param>
    /// <param name="hardwareId">Hardware Id of the device.</param>
    public void UpdateHardwareIdWifiSubnet(int deviceId, string wifiSubnet, string hardwareId);

    /// <summary>
    /// Updates the Windows Device details for the Windows device.
    /// </summary>
    /// <param name="windowsModernSnapshot">Windows modern snapshot model.</param>
    public void UpdateWindowsDeviceDetails(WindowsModernSnapshot windowsModernSnapshot);

    /// <summary>
    /// Updates the Windows Defender Scan Info for the Windows device.
    /// </summary>
    /// <param name="deviceId">The device Id.</param>
    /// <param name="quickScanTime">The quick scan time.</param>
    /// <param name="fullScanTime">The full scan time.</param>
    /// <param name="syncTime">The sync time.</param>
    void UpdateDefenderScanInfo(int deviceId, DateTime? quickScanTime, DateTime? fullScanTime, DateTime? syncTime);

    /// <summary>
    /// Updates the os image info.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="osImageId">The os image id.</param>
    /// <param name="deployedTime">The deployed time.</param>
    void UpdateOsImageInfo(int deviceId, int? osImageId, DateTime? deployedTime);

    /// <summary>
    /// Update Bios Password Status ID
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="biosPasswordStatusId"></param>
    void UpdateBiosPasswordStatusId(int deviceId, byte biosPasswordStatusId);

    /// <summary>
    /// Bulk insert to Windows Device table.
    /// </summary>
    /// <param name="windowsDevices">Windows devices.</param>
    void BulkInsertWindowsDevices(IEnumerable<WindowsDeviceData> windowsDevices);

    /// <summary>
    /// Gets Antivirus scan summary for a device group.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <param name="lastSyncedTime">The last synced time.</param>
    /// <returns>Antivirus Group Scan Summary.</returns>
    AntivirusGroupScanSummary GetDeviceGroupsDefaultAntivirusScanSummary(IEnumerable<int> groupIds, DeviceFamily deviceFamily, DateTime lastSyncedTime);

    /// <summary>
    /// Get antivirus scan summary for the device groups within the specified date range.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <param name="lastScannedStartTime">Last scanned start time.</param>
    /// <param name="lastScannedEndTime">Last scanned end time.</param>
    /// <returns>Antivirus Group Scan Summary.</returns>
    AntivirusGroupScanSummary GetDeviceGroupsCustomAntivirusScanSummary(IEnumerable<int> groupIds, DeviceFamily deviceFamily, DateTime lastScannedStartTime, DateTime lastScannedEndTime);

    /// <summary>
    /// Gets antivirus scan information.
    /// </summary>
    /// <param name="deviceId">The device Id.</param>
    /// <returns>Antivirus quick and full scan time data.</returns>
    AntivirusScanTimeData GetAntivirusScanTimeData(int deviceId);

    /// <summary>
    /// Gets antivirus last full scan summary.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="startTime">Start time for getting last full scan summary.</param>
    /// <param name="endTime">End time for getting last full scan summary.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <param name="skip">Specifies antivirus last full scan records to skip.</param>
    /// <param name="take">Specifies antivirus last full scan records to take.</param>
    /// <param name="order">Specifies the order of sorting.</param>
    /// <param name="totalCount">Specifies total record count.</param>
    /// <returns>Returns antivirus last full scan summary.</returns>
    IEnumerable<AntivirusLastScanDeviceData> GetAntivirusLastFullScanSummary(
        IEnumerable<int> groupIds,
        DateTime startTime,
        DateTime endTime,
        DeviceFamily deviceFamily,
        int skip,
        int take,
        bool order,
        out int totalCount);

    /// <summary>
    /// Gets antivirus last full scan summary.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="startTime">Start time for getting last full scan summary.</param>
    /// <param name="endTime">End time for getting last full scan summary.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <returns>Returns antivirus last full scan summary.</returns>
    IEnumerable<AntivirusLastScanDeviceData> GetAntivirusLastFullScanSummary(
        IEnumerable<int> groupIds,
        DateTime startTime,
        DateTime endTime,
        DeviceFamily deviceFamily);

    /// <summary>
    /// Gets antivirus last quick scan summary.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="startTime">Start time for getting last quick scan summary.</param>
    /// <param name="endTime">End time for getting last quick scan summary.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <param name="skip">Specifies antivirus last quick scan records to skip.</param>
    /// <param name="take">Specifies antivirus last quick scan records to take.</param>
    /// <param name="order">Specifies the order of sorting.</param>
    /// <param name="totalCount">Specifies total record count.</param>
    /// <returns>Returns antivirus last quick scan summary.</returns>
    IEnumerable<AntivirusLastScanDeviceData> GetAntivirusLastQuickScanSummary(
        IEnumerable<int> groupIds,
        DateTime startTime,
        DateTime endTime,
        DeviceFamily deviceFamily,
        int skip,
        int take,
        bool order,
        out int totalCount);

    /// <summary>
    /// Gets antivirus last quick scan summary.
    /// </summary>
    /// <param name="groupIds">List of group Ids.</param>
    /// <param name="startTime">Start time for getting last quick scan summary.</param>
    /// <param name="endTime">End time for getting last quick scan summary.</param>
    /// <param name="deviceFamily">The device family.</param>
    /// <returns>Returns antivirus last quick scan summary.</returns>
    IEnumerable<AntivirusLastScanDeviceData> GetAntivirusLastQuickScanSummary(
        IEnumerable<int> groupIds,
        DateTime startTime,
        DateTime endTime,
        DeviceFamily deviceFamily);
}
