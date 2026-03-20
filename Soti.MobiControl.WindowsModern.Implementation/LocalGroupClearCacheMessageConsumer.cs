using System;
using Soti.Diagnostics;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Local Group Clear Cache Message Consumer.
/// </summary>
internal sealed class LocalGroupClearCacheMessageConsumer : MessageConsumer<LocalGroupCacheClearMessage>
{
    private readonly IProgramTrace _programTrace;
    private readonly IWindowsDeviceLocalGroupsService _windowsDeviceLocalGroupsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalGroupClearCacheMessageConsumer"/> class.
    /// </summary>
    /// <param name="programTrace">ProgramTrace Instance.</param>
    /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
    public LocalGroupClearCacheMessageConsumer(IProgramTrace programTrace,
        IWindowsDeviceLocalGroupsService windowsDeviceLocalGroupsService)
    {
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _windowsDeviceLocalGroupsService = windowsDeviceLocalGroupsService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalGroupsService));
    }

    /// <summary>
    /// Consume call to update local group cache.<see cref="LocalGroupClearCacheMessageConsumer"/> class.
    /// </summary>
    /// <param name="message">Instance of WindowsDeviceLocalGroupNameData.</param>
    public override void Consume(LocalGroupCacheClearMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        _windowsDeviceLocalGroupsService.InvalidateCache(false);
        _programTrace.Write(TraceLevel.Verbose, "Caching", "Local Group cache cleared.");
    }
}
