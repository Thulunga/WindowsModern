namespace Soti.MobiControl.WindowsModern.Models;

/// <summary>
/// Windows device tmp local user model.
/// </summary>
public sealed class WindowsDeviceTmpLocalUserModel
{
    /// <summary>
    /// Gets or sets tmp LocalUserId.
    /// </summary>
    public int WindowsDeviceUserId { get; set; }

    /// <summary>
    /// Gets or sets encrypted User Name.
    /// </summary>
    public byte[] UserName { get; set; }  // Encrypted data

    /// <summary>
    /// Gets or sets encrypted User Full Name.
    /// </summary>
    public byte[] UserFullName { get; set; }
}
