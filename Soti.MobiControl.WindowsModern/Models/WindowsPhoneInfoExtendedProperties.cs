using System;

namespace Soti.MobiControl.WindowsModern.Models
{
    [Flags]
    public enum WindowsPhoneInfoExtendedProperties : long
    {
        /// <summary>
        /// No columns for update
        /// </summary>
        None = 0L,

        /// <summary>
        /// ChannelUri column
        /// </summary>
        ChannelUri = 1L,

        /// <summary>
        /// ChannelStatus column
        /// </summary>
        ChannelStatus = 2L,

        /// <summary>
        /// SessionContext column
        /// </summary>
        SessionContext = 4L,

        /// <summary>
        /// SessionWatermark column
        /// </summary>
        SessionWatermark = 8L,

        /// <summary>
        /// TimeStamp column
        /// </summary>
        Timestamp = 16L,

        /// <summary>
        /// EnrollmentInfoId column
        /// </summary>
        EnrollmentInfoId = 32L,

        /// <summary>
        /// IsRoboSupport column
        /// </summary>
        IsRoboSupport = 64L,

        /// <summary>
        /// RenewPeriodDays column
        /// </summary>
        RenewPeriodDays = 128L,

        /// <summary>
        /// RetryIntervalDays column
        /// </summary>
        RetryIntervalDays = 256L,

        /// <summary>
        /// RoboStatusId column
        /// </summary>
        RoboStatusId = 512L,

        /// <summary>
        /// RoboStatusId column
        /// </summary>
        DeviceId = 1024L,

        /// <summary>
        /// EnrollmentId column
        /// </summary>
        EnrollmentId = 2048L,

        /// <summary>
        /// TPMSpecVersion column
        /// </summary>
        TpmSpecDetails = 4096L,

        /// <summary>
        /// IsSMode column
        /// </summary>
        IsSMode = 8192L,

        /// <summary>
        /// IsUpdateApprovalRequired column
        /// </summary>
        IsUpdateApprovalRequired = 16384L,

        /// <summary>
        /// IsManageUpdates column
        /// </summary>
        IsManageUpdates = 32768L
    }
}