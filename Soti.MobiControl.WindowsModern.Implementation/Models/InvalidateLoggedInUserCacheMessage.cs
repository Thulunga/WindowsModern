using Soti.MobiControl.Messaging;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Logged-in User Cache Remove Message.
/// </summary>
internal class InvalidateLoggedInUserCacheMessage : IMessage
{
    /// <summary>
    /// Gets or sets Device ID.
    /// </summary>
    public int DeviceId { get; set; }
}
