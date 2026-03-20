using System;
using Soti.Diagnostics;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Invalidate Logged-in User Message Consumer.
/// </summary>
internal sealed class InvalidateLoggedInUserCacheMessageConsumer : MessageConsumer<InvalidateLoggedInUserCacheMessage>
{
    private readonly IProgramTrace _programTrace;
    private readonly IWindowsDeviceLoggedInUserService _windowsDeviceLoggedInUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidateLoggedInUserCacheMessageConsumer"/> class.
    /// </summary>
    /// <param name="programTrace">ProgramTrace Instance.</param>
    /// <param name="windowsDeviceLoggedInUserService">WindowsDeviceLoggedInUserService Instance.</param>
    /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
    public InvalidateLoggedInUserCacheMessageConsumer(
        IProgramTrace programTrace, IWindowsDeviceLoggedInUserService windowsDeviceLoggedInUserService)
    {
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _windowsDeviceLoggedInUserService = windowsDeviceLoggedInUserService ?? throw new ArgumentNullException(nameof(windowsDeviceLoggedInUserService));
    }

    /// <summary>
    /// Consume call to remove cache.
    /// </summary>
    /// <param name="message">Instance of InvalidateLoggedInUserCacheMessage.</param>
    public override void Consume(InvalidateLoggedInUserCacheMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _windowsDeviceLoggedInUserService.InvalidateLoggedInUserCache(message.DeviceId, false);
        _programTrace.Write(TraceLevel.Verbose, "Caching", $"Logged-In User cache removed for device {message.DeviceId}.");
    }
}
