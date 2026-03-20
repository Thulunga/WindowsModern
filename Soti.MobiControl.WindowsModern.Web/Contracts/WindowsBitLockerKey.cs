using System;

namespace Soti.MobiControl.WindowsModern.Web.Contracts
{
    /// <summary>
    /// Device BitLocker Keys.
    /// </summary>
    public sealed class WindowsBitLockerKey
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
