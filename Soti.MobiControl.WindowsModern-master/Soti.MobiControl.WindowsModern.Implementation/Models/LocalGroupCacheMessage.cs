using Soti.MobiControl.Messaging;

namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
/// Local Group Cache Message.
/// </summary>
internal sealed class LocalGroupCacheMessage : IMessage
{
    /// <summary>
    /// Gets or sets Group Name ID
    /// </summary>
    public int GroupNameId { get; set; }

    /// <summary>
    /// Gets or sets Group Name
    /// </summary>
    public string GroupName { get; set; }
}
