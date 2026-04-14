using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models
{
    /// <summary>
    /// Device BitLocker Keys.
    /// </summary>
    public sealed class DeviceBitLockerKey
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
        public string RecoveryKey { get; set; }

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
