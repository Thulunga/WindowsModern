namespace Soti.MobiControl.WindowsModern.Web.Enums
{
    /// <summary>
    /// BitLocker encryption status for a drive volume.
    /// </summary>
    public enum DriveEncryptionStatus : byte
    {
        None = 0,
        Encrypting = 1,
        EncryptionPaused = 2,
        Encrypted = 3,
        Decrypting = 4,
        DecryptionPaused = 5,
        Decrypted = 6
    }
}
