using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers
{
    /// <summary>
    /// Device BitLocker Key(s) Provider.
    /// </summary>
    internal interface IDeviceBitLockerKeyProvider
    {
        /// <summary>
        /// Determines whether [are BitLocker key(s) available] [the specified device identifier].
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>
        ///   <c>true</c> if [are BitLocker key(s) available] [the specified device identifier]; otherwise, <c>false</c>.
        /// </returns>
        bool AreBitLockerKeysAvailable(int deviceId);

        /// <summary>
        /// Determines whether [are BitLocker key(s) available] [the specified device ids].
        /// </summary>
        /// <param name="deviceIds">The device ids.</param>
        /// <returns>Collection of the BitLocker keys information available for the devices.</returns>
        IReadOnlyDictionary<int, bool> AreBitLockerKeysAvailable(HashSet<int> deviceIds);

        /// <summary>
        /// Gets the bit locker keys.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>Collection of device BitLocker key information.</returns>
        IEnumerable<DeviceBitLockerKeyData> GetBitLockerKeys(int deviceId);

        /// <summary>
        /// Updates the bit locker keys.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="bitLockerKeys">The bit locker keys.</param>
        void UpdateBitLockerKeys(int deviceId, IEnumerable<DeviceBitLockerKeyData> bitLockerKeys);

        /// <summary>
        /// Deletes the bit locker keys.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        void DeleteBitLockerKeys(int deviceId);
    }
}
