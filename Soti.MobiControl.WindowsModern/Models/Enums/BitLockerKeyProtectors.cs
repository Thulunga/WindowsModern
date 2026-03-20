using System;

namespace Soti.MobiControl.WindowsModern.Models.Enums
{
    /// <summary>
    /// BitLocker key protectors.
    /// </summary>
    [Flags]
    public enum BitLockerKeyProtectors : byte
    {
        None = 0,
        Tpm = 1,
        Pin = 2,
        StartupKey = 4,
        RecoveryPassword = 8,
        RecoveryKey = 16,
        Password = 32,
        AutoUnlock = 64
    }
}
