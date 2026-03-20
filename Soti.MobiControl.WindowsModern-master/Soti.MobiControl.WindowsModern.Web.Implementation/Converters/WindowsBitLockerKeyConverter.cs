using System;
using Soti.MobiControl.WindowsModern.Web.Contracts;
using DeviceBitLockerKeyModel = Soti.MobiControl.WindowsModern.Models.DeviceBitLockerKey;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters
{
    /// <summary>
    /// Converter class to convert to WindowsBitLockerKey contract.
    /// </summary>
    internal static class WindowsBitLockerKeyConverter
    {
        /// <summary>
        /// Converts to BitLocker Recovery keys contract.
        /// </summary>
        /// <param name="contract">The contract.</param>
        /// <returns>WindowsBitLockerKey contract.</returns>
        public static WindowsBitLockerKey ToRecoveryKeysContract(this DeviceBitLockerKeyModel contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return new WindowsBitLockerKey
            {
                DriveName = contract.DriveName,
                RecoveryKeyId = contract.RecoveryKeyId,
                RecoveryKey = contract.RecoveryKey,
            };
        }
    }
}