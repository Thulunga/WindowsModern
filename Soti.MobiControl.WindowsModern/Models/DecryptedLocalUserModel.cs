namespace Soti.MobiControl.WindowsModern.Models;

public sealed class DecryptedLocalUserModel
{
    /// <summary>
    /// Gets or sets tmp LocalUserId.
    /// </summary>
    public int WindowsDeviceUserId { get; set; }

    /// <summary>
    /// Gets or sets Decrypted User Name.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets Decrypted User Full Name.
    /// </summary>
    public string UserFullName { get; set; }
}
