using System;
using Soti.Diagnostics;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Invalidate BitLocker Cache Message Consumer.
/// </summary>
internal sealed class InvalidateBitLockerCacheMessageConsumer : MessageConsumer<InvalidateBitLockerCacheMessage>
{
    private readonly IProgramTrace _programTrace;
    private readonly IDeviceBitLockerKeyService _deviceBitLockerKeyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidateBitLockerCacheMessageConsumer"/> class.
    /// </summary>
    /// <param name="programTrace">ProgramTrace Instance.</param>
    /// <param name="deviceBitLockerKeyService">DeviceBitLockerKeyService Instance.</param>
    /// <exception cref="ArgumentNullException">ArgumentNullException.</exception>
    public InvalidateBitLockerCacheMessageConsumer(
        IProgramTrace programTrace, IDeviceBitLockerKeyService deviceBitLockerKeyService)
    {
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _deviceBitLockerKeyService = deviceBitLockerKeyService ?? throw new ArgumentNullException(nameof(deviceBitLockerKeyService));
    }

    /// <summary>
    /// Consume call to remove cache.
    /// </summary>
    /// <param name="message">Instance of InvalidateBitLockerCacheMessage.</param>
    public override void Consume(InvalidateBitLockerCacheMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _deviceBitLockerKeyService.InvalidateBitLockerCache(message.DeviceId, false);
        _programTrace.Write(TraceLevel.Verbose, "Caching", $"AreBitLockerKeysAvailable cache removed for device {message.DeviceId}.");
    }
}
