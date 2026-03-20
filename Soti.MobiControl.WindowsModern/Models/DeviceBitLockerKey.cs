using System;

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
    }
}
