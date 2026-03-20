using Soti.MobiControl.Messaging;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// AreBitLockerKeysAvailable Cache Remove Message.
/// </summary>
internal class InvalidateBitLockerCacheMessage : IMessage
{
    /// <summary>
    /// Gets or sets Device ID.
    /// </summary>
    public int DeviceId { get; set; }
}
