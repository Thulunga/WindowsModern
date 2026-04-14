using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers
{
    /// <summary>
    /// Device BitLocker Key data.
    /// </summary>
    internal sealed class DeviceBitLockerKeyData
    {
        /// <summary>
        /// Gets or sets the name of the drive.
        /// </summary>
        public string DriveName { get; set; }

        /// <summary>
        /// Gets or sets the recovery key identifier.
        /// </summary>
        public Guid RecoveryKeyId { get; set; }

        /// <summary>
        /// Gets or sets the recovery key.
        /// </summary>
        public byte[] RecoveryKey { get; set; }

        /// <summary>
        /// Gets or sets the data key identifier.
        /// </summary>
        public int DataKeyId { get; set; }

        /// <summary>
        /// Gets or sets the drive encryption status.
        /// </summary>
        public DriveEncryptionStatus DriveEncryptionStatus { get; set; }

        /// <summary>
        /// Gets or sets the key protectors.
        /// </summary>
        public BitLockerKeyProtectors KeyProtectors { get; set; }

        /// <summary>
        /// Gets or sets the drive type.
        /// </summary>
        public DriveType DriveType { get; set; }
    }
}
