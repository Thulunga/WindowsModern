namespace Soti.MobiControl.WindowsModern.Models
{
    /// <summary>
    /// Re-Enrollment Rule.
    /// </summary>
    public sealed class WindowsReEnrollmentCriteria
    {
        /// <summary>
        /// Gets or sets the HardwareID.
        /// </summary>
        public bool HardwareId { get; set; }

        /// <summary>
        /// Gets or sets the MacAddress.
        /// </summary>
        public bool MacAddress { get; set; }
    }
}
