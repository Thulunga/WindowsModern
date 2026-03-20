using Soti.MobiControl.Exceptions;
using Soti.MobiControl.Exceptions.Attributes;
using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Exceptions;

/// <summary>
/// Declares DeviceHardwareException exception.
/// </summary>
[Serializable]
[BusinessLogicExceptionErrorCodeRange(start: 12050, end: 12099)]
internal sealed class DeviceHardwareException : BusinessLogicException
{
    private const string NoDeviceHardwareFoundExceptionMsg = "No device hardware exists for the specified hardware serial number '{0}'.";
    private const string UnableToUpdateDeviceHardwareStatusExceptionMsg = "Unable to update device hardware status from '{0}' to '{1}' for hardware serial number '{2}'.";

    /// <summary>
    /// Initializes an instance of the DeviceHardwareException.
    /// </summary>
    /// <param name="errorCode">Error Code.</param>
    /// <param name="description">Description.</param>
    /// <param name="errorMessageParameters">Error Message Parameters.</param>
    private DeviceHardwareException(int errorCode, string description, params object[] errorMessageParameters)
        : base(errorCode, description, errorMessageParameters)
    {
    }

    /// <summary>
    /// Device Hardware Not Found Exception.
    /// </summary>
    /// <param name="deviceHardwareSerialNumber">Device hardware serial number.</param>
    public static DeviceHardwareException DeviceHardwareNotFoundException(string deviceHardwareSerialNumber)
    {
        return new DeviceHardwareException(12051, NoDeviceHardwareFoundExceptionMsg, deviceHardwareSerialNumber);
    }

    /// <summary>
    /// Device Hardware Status Exception.
    /// </summary>
    /// <param name="existingDeviceHardwareStatus">Existing device hardware status.</param>
    /// <param name="newDeviceHardwareStatus">New device hardware status.</param>
    /// <param name="deviceHardwareSerialNumber">Device hardware serial number.</param>
    public static DeviceHardwareException DeviceHardwareStatusException(
        string existingDeviceHardwareStatus,
        string newDeviceHardwareStatus,
        string deviceHardwareSerialNumber)
    {
        return new DeviceHardwareException(
            12052,
            UnableToUpdateDeviceHardwareStatusExceptionMsg,
            existingDeviceHardwareStatus,
            newDeviceHardwareStatus,
            deviceHardwareSerialNumber);
    }
}
