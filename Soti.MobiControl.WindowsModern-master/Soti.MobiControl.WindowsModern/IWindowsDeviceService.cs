using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern
{
    /// <summary>
    /// Windows Device Service.
    /// </summary>
    public interface IWindowsDeviceService
    {
        /// <summary>
        /// Checks whether the device is Locked or not.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <returns>Lock status of the device.</returns>
        bool IsDeviceLocked(int deviceId);

        /// <summary>
        /// Get the status of Windows SandBox for a device.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <returns>Windows Sandbox Status.</returns>
        WindowsSandBoxDetails GetWinSandBoxStatusByDeviceId(int deviceId);

        /// <summary>
        /// Get the status of Windows SandBox for all devices.
        /// </summary>
        /// <returns>Windows Sandbox Status.</returns>
        IEnumerable<WindowsSandBoxDetails> GetAllWinSandBoxStatus();

        /// <summary>
        /// Update Windows SandBox status.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="isSandBoxEnabled">New status of SandBox.</param>
        void UpdateWindowsSandBoxStatus(int deviceId, bool isSandBoxEnabled);

        /// <summary>
        /// Determines lock status for the specified device ids.
        /// </summary>
        /// <param name="deviceIds">The device ids.</param>
        /// <returns>Lock status of devices.</returns>
        IReadOnlyDictionary<int, bool> AreDevicesLocked(HashSet<int> deviceIds);

        /// <summary>
        /// Gets windows sandbox status for the specified device ids.
        /// </summary>
        /// <param name="deviceIds">The device ids.</param>
        /// <returns>Sandbox status of devices.</returns>
        IReadOnlyDictionary<int, bool> GetWinSandBoxStatusByDeviceIds(IEnumerable<int> deviceIds);

        /// <summary>
        /// Gets windows device details for the specified device ids.
        /// </summary>
        /// <param name="deviceIds">The device ids.</param>
        /// <returns>Details of devices.</returns>
        IReadOnlyDictionary<int, WindowsDeviceModel> BulkGetDeviceDetails(IEnumerable<int> deviceIds);

        /// <summary>
        /// Returns Windows device Model.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <returns>Windows Device Model.</returns>
        WindowsDeviceModel GetByDeviceId(int deviceId);

        /// <summary>
        /// Inserts new record.
        /// </summary>
        /// <param name="deviceModel">Windows device model.</param>
        void Insert(WindowsDeviceModel deviceModel);

        /// <summary>
        /// Updates existing record.
        /// </summary>
        /// <param name="deviceModel">Windows device model.</param>
        void Update(WindowsDeviceModel deviceModel);

        /// <summary>
        /// Updates the LastCheckInDeviceUserTime for the Windows Logged-In User.
        /// </summary>
        /// <param name="deviceId">The Device Id.</param>
        /// <param name="lastCheckInTime">Last Check-In Device user time.</param>
        void UpdateLoggedInUserLastCheckInTime(int deviceId, DateTime lastCheckInTime);

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
        /// <param name="lastQuickScanTime">Last quick scan time of the device.</param>
        /// <param name="lastFullScanTime">Last full scan time of the device.</param>
        /// <param name="lastSyncTime">Last defender data sync time for the device.</param>
        void UpdateDefenderScanInfo(int deviceId, DateTime? lastQuickScanTime, DateTime? lastFullScanTime, DateTime? lastSyncTime);

        /// <summary>
        /// Updates the os image info.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="osImageId">The os image id.</param>
        /// <param name="deployedTime">The deployed time.</param>
        void UpdateOsImageInfo(int deviceId, int osImageId, DateTime deployedTime);

        /// <summary>
        /// Bulk insert to Windows Device table.
        /// </summary>
        /// <param name="windowsDevices">Windows devices.</param>
        void BulkInsertWindowsDevices(IEnumerable<WindowsDeviceModel> windowsDevices);
    }
}
