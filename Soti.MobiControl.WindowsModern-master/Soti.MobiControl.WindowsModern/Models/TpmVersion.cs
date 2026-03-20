namespace Soti.MobiControl.WindowsModern.Models
{
    public class TpmVersion
    {
        public const int TpmVersionItems = 3;

        /// <summary>
        /// Gets or sets the TPM spec version.
        /// </summary>
        public string TpmSpecVersion { get; set; }

        /// <summary>
        /// Gets or sets the TPM spec level.
        /// </summary>
        public string TpmSpecLevel { get; set; }

        /// <summary>
        /// Gets or sets the TPM spec revision.
        /// </summary>
        public string TpmSpecRevision { get; set; }
    }
}