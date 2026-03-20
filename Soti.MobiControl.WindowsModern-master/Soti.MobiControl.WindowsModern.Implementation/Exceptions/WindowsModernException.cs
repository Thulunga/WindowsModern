using System;
using Soti.MobiControl.Exceptions;
using Soti.MobiControl.Exceptions.Attributes;

namespace Soti.MobiControl.WindowsModern.Implementation.Exceptions;

/// <summary>
/// Windows Modern Exceptions.
/// </summary>
[Serializable]
[BusinessLogicExceptionErrorCodeRange(start: 11750, end: 11799)]
internal sealed class WindowsModernException : BusinessLogicException
{
    private WindowsModernException(int errorCode, string description, params object[] errorMessageParameters)
        : base(errorCode, description, errorMessageParameters)
    {
    }

    /// <summary>
    /// Antivirus Sync Data not found exception.
    /// </summary>
    /// <returns>WindowsModernException</returns>
    public static WindowsModernException AntivirusSyncDataNotFound()
    {
        return new WindowsModernException(11750, "Group sync required. Please try again after syncing");
    }
}
