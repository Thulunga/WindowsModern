namespace Soti.MobiControl.WindowsModern
{
    /// <summary>
    /// Service interface for querying device configurations or profiles. Implementation is in MS.
    /// </summary>
    public interface IWindowsModernDeviceConfigurationProxyService
    {
        /// <summary>
        /// Gets whether antivirus payloads are assigned to the device.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>Returns whether antivirus payloads assigned to the device.</returns>
        bool IsAntivirusPayloadAssigned(int deviceId);
    }
}
