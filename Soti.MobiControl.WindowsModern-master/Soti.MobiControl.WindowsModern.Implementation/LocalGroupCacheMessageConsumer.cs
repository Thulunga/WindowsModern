using System;
using Soti.Diagnostics;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Local Group Cache Message Consumer.
/// </summary>
internal sealed class LocalGroupCacheMessageConsumer : MessageConsumer<LocalGroupCacheMessage>
{
    private readonly IProgramTrace _programTrace;
    private readonly IWindowsDeviceLocalGroupsService _windowsDeviceLocalGroupsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalGroupCacheMessageConsumer"/> class.
    /// </summary>
    /// <param name="programTrace">ProgramTrace Instance.</param>
    /// <param name="windowsDeviceLocalGroupsService">WindowsDeviceLocalGroupsService Instance.</param>
    /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
    public LocalGroupCacheMessageConsumer(
        IProgramTrace programTrace, IWindowsDeviceLocalGroupsService windowsDeviceLocalGroupsService)
    {
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _windowsDeviceLocalGroupsService = windowsDeviceLocalGroupsService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalGroupsService));
    }

    /// <summary>
    /// Consume call to update local group cache.<see cref="LocalGroupCacheMessageConsumer"/> class.
    /// </summary>
    /// <param name="message">Instance of WindowsDeviceLocalGroupNameData.</param>
    public override void Consume(LocalGroupCacheMessage message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        _windowsDeviceLocalGroupsService.SetCachedGroupData(message.GroupNameId, message.GroupName, false);
        _programTrace.Write(TraceLevel.Verbose, "Caching", "Local Group cache updated.");
    }
}
