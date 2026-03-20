using System;

namespace Soti.MobiControl.WindowsModern.Models
{
    public class WindowsPhoneDeviceInfo
    {
        /// <summary>
        /// Gets or sets id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets device id.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets serialized device session context.
        /// </summary>
        public string SessionContext { get; set; }

        /// <summary>
        /// Gets or sets WNS channel URI.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Gets or sets device WNS channel status.
        /// </summary>
        public int ChannelStatus { get; set; }

        /// <summary>
        /// Gets or sets device session watermark id.
        /// </summary>
        public string SessionWatermark { get; set; }

        /// <summary>
        /// Gets or sets device enrollment info id.
        /// </summary>
        public int EnrollmentInfoId { get; set; }

        /// <summary>
        /// Gets or sets device enrollment id.
        /// </summary>
        public string EnrollmentId { get; set; }

        /// <summary>
        /// Gets or sets record time and date.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets device ROBO support value.
        /// </summary>
        public bool IsRoboSupport { get; set; }

        /// <summary>
        /// Gets or sets renew period in days value.
        /// </summary>
        public short RenewPeriodDays { get; set; }

        /// <summary>
        /// Gets or sets retry interval in days value.
        /// </summary>
        public short RetryIntervalDays { get; set; }

        /// <summary>
        /// Gets or sets Robo status id value.
        /// </summary>
        public byte RoboStatusId { get; set; }

        /// <summary>
        /// Gets or sets TPMSpecVersion.
        /// </summary>
        public string TpmSpecVersion { get; set; }

        /// <summary>
        /// Gets or sets TPMSpecLevel.
        /// </summary>
        public string TpmSpecLevel { get; set; }

        /// <summary>
        /// Gets or sets TPMSpecRevision.
        /// </summary>
        public string TpmSpecRevision { get; set; }

        /// <summary>
        /// Gets or sets IsSMode.
        /// </summary>
        public bool? IsSMode { get; set; }

        /// <summary>
        /// Gets or sets IsUpdateApprovalRequired.
        /// </summary>
        public bool? IsUpdateApprovalRequired { get; set; }

        /// <summary>
        /// Gets or sets IsManageUpdates.
        /// </summary>
        public bool? IsManageUpdates { get; set; }
    }
}