using System;
using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern;

public interface IWindowsPhoneDeviceService
{
    /// <summary>
    /// Returns Windows Phone device info.
    /// </summary>
    /// <param name="deviceId">Windows Phone device id.</param>
    /// <returns>WindowsPhoneDeviceInfo instance.</returns>
    WindowsPhoneDeviceInfo GetByDeviceId(int deviceId);

    /// <summary>
    /// Updates existing record.
    /// </summary>
    /// <param name="deviceInfo">Windows Phone device info.</param>
    /// <param name="extendedProperties">extended Properties.</param>
    void Update(WindowsPhoneDeviceInfo deviceInfo, IEnumerable<WindowsPhoneInfoExtendedProperties> extendedProperties);

    /// <summary>
    /// Invalidate the device snapshot cache
    /// </summary>
    /// <param name="deviceId">device Id</param>
    void InvalidateDeviceSnapshotCache(int deviceId);

    /// <summary>
    /// Updates enrollment Id for existing record or inserts a new record.
    /// </summary>
    /// <param name="deviceId">unique device id.</param>
    /// <param name="enrollmentId">device enrollmentId.</param>
    void UpdateEnrollmentId(int deviceId, int enrollmentId);

    /// <summary>
    /// Gets device enrollment ID.
    /// </summary>
    /// <param name="deviceId">unique device id.</param>
    /// <returns>Device enrollment ID.</returns>
    int GetEnrollmentId(int deviceId);

    /// <summary>
    /// Gets WNS Channel URI and Channel Status.
    /// </summary>
    /// <param name="deviceId">unique device id.</param>
    /// <returns>WNS Channel URI and Channel Status.</returns>
    Tuple<string, int> GetWnsChannelInfo(int deviceId);

    /// <summary>
    /// Gets session device context data.
    /// </summary>
    /// <param name="deviceId">unique device id.</param>
    /// <returns>session device context data.</returns>
    string GetSessionContextById(int deviceId);

    /// <summary>
    /// Check Channel Status and Channel Uri.
    /// </summary>
    /// <param name="deviceId">unique device id.</param>
    /// <returns>session device context data.</returns>
    bool CheckChannel(int deviceId);

    /// <summary>
    /// Gets WNS Channel Status.
    /// </summary>
    /// <param name="deviceId">Device having WNS Channel Uri.</param>
    /// <returns>Channel status.</returns>
    int GetChannelStatus(int deviceId);

    /// <summary>
    /// Blocks WNS Channel.
    /// </summary>
    /// <param name="deviceId">Device having WNS Channel Uri.</param>
    void BlockChannel(int deviceId);

    /// <summary>
    /// Gets device TPM version.
    /// </summary>
    /// <param name="deviceId">unique device id.</param>
    /// <returns>Device TPM version.</returns>
    TpmVersion GetTpmVersion(int deviceId);

    /// <summary>
    /// Gets device TPM version.
    /// </summary>
    /// <param name="deviceIds">device ids.</param>
    /// <returns>Device TPM version.</returns>
    Dictionary<int, TpmVersion> GetTpmVersions(IEnumerable<int> deviceIds);

    /// <summary>
    /// Gets collection of WNS listeners (device Ids) with valid Channel status.
    /// </summary>
    /// <returns></returns>
    IEnumerable<int> GetWnsListeners();
}
