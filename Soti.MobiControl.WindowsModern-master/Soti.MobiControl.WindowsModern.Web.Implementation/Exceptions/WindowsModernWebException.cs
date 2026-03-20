using System;
using Soti.MobiControl.Exceptions;
using Soti.MobiControl.Exceptions.Attributes;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Exceptions;

/// <summary>
/// WindowsModern Web Exceptions.
/// </summary>
[Serializable]
[BusinessLogicExceptionErrorCodeRange(start: 11800, end: 11849)]
internal sealed class WindowsModernWebException : BusinessLogicException
{
    private WindowsModernWebException(int errorCode, string description, params object[] errorMessageParameters)
        : base(errorCode, description, errorMessageParameters)
    {
    }

    /// <summary>
    /// Windows Defender Payload Not Assigned exception.
    /// </summary>
    /// <returns>WindowsModernWebException</returns>
    public static WindowsModernWebException WindowsDefenderPayloadNotAssigned()
    {
        return new WindowsModernWebException(11800, "Windows Defender payload is not assigned");
    }

    /// <summary>
    /// Windows Defender Sync Is In Progress.
    /// </summary>
    /// <returns>WindowsModernWebException</returns>
    public static WindowsModernWebException WindowsDefenderSyncIsInProgress()
    {
        return new WindowsModernWebException(11801, "Sync is in progress. Try again later");
    }

    /// <summary>
    /// Windows Modern Device Required exception.
    /// </summary>
    /// <returns>WindowsModernWebException</returns>
    public static WindowsModernWebException RequireWindowsDesktopDevice()
    {
        return new WindowsModernWebException(11802, "At least one device must be a Windows Modern Device.");
    }
}
