namespace Soti.MobiControl.WindowsModern.Implementation.Models;

/// <summary>
///     ProductVersion properties
/// </summary>
public sealed class WindowsProductVersion
{
    /// <summary>
    ///   Name of the Product Version to be displayed in the UI.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Product Version of Windows. Eg : Windows 11, Windows 10
    /// </summary>
    public string ProductVersion { get; set; }
}
