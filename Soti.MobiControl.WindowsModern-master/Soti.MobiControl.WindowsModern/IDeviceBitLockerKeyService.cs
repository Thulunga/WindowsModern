using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Device BitLocker key(s) Service.
/// </summary>
public interface IDeviceBitLockerKeyService
{
    /// <summary>
    /// Check if the BitLocker key(s) information is available for the device.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <returns>
    ///   <c>true</c> if the BitLocker key(s) information available for the device; otherwise, <c>false</c>.
    /// </returns>
    bool AreBitLockerKeysAvailable(int deviceId);

    /// <summary>
    /// Determines whether [are BitLocker keys available] [the specified device ids].
    /// </summary>
    /// <param name="deviceIds">The device ids.</param>
    /// <returns>Collection of the BitLocker keys information available for the devices.</returns>
    IReadOnlyDictionary<int, bool> AreBitLockerKeysAvailable(HashSet<int> deviceIds);

    /// <summary>
    /// Get BitLockers keys by Device Id.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <returns>Collection of the BitLockers keys.</returns>
    IEnumerable<DeviceBitLockerKey> GetBitLockerKeys(int deviceId);

    /// <summary>
    /// Processes the BitLocker keys data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="bitLockerKeysData">The BitLocker keys data.</param>
    void ProcessBitLockerKeysData(int deviceId, string bitLockerKeysData);

    /// <summary>
    /// Deletes the BitLocker keys data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    void DeleteBitLockerKeysData(int deviceId);

    /// <summary>
    /// Clears the cache and notify other services.
    /// </summary>
    /// <param name="deviceId">The device id of the device.</param>
    /// <param name="notify">The value indicating whether to indicate other services.</param>
    void InvalidateBitLockerCache(int deviceId, bool notify);
}
