namespace Soti.MobiControl.WindowsModern.Models
{
    public enum WnsChannelStatus
    {
        /// <summary>
        /// Valid channel URI
        /// </summary>
        Success = 0,

        /// <summary>
        /// invalid PFN
        /// </summary>
        InvalidPackageName = 1,

        /// <summary>
        /// invalid or expired device authentication with MSA
        /// </summary>
        ExpiredAuthentication = 2,

        /// <summary>
        /// WNS client registration failed due to an invalid or revoked PFN
        /// </summary>
        RegistrationFailed = 3,

        /// <summary>
        /// no Channel URI assigned
        /// </summary>
        NoChannelAssigned = 4,

        /// <summary>
        /// Channel URI has expired
        /// </summary>
        ChannelExpired = 5,

        /// <summary>
        /// Channel URI failed to be revoked
        /// </summary>
        ChannelRevokeFailed = 6,

        /// <summary>
        /// push notification received, but unable to establish an OMA-DM session due to power or connectivity limitations.
        /// </summary>
        UnableConnect = 7,

        /// <summary>
        /// Unknown Error
        /// </summary>
        UnknownError = 8
    }
}