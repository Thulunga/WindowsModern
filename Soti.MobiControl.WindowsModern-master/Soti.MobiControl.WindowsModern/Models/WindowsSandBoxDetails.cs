namespace Soti.MobiControl.WindowsModern.Models
{
    /// <summary>
    /// Windows Device Model.
    /// </summary>
    public sealed class WindowsSandBoxDetails
    {
        /// <summary>
        /// Device Id of the device.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Represents status of Windows SandBox in a device.
        /// </summary>
        public bool IsSandBoxEnabled { get; set; }
    }
}