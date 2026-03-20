using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado
{
    internal static class WindowsPhoneInfoExtendedPropertiesConverter
    {
        private static readonly Dictionary<WindowsPhoneInfoExtendedProperties, WindowsPhoneInfoUpdateColumns> ExtendedPropertiesMapper =
            new Dictionary<WindowsPhoneInfoExtendedProperties, WindowsPhoneInfoUpdateColumns>
            {
                { WindowsPhoneInfoExtendedProperties.None, WindowsPhoneInfoUpdateColumns.None },
                { WindowsPhoneInfoExtendedProperties.ChannelUri, WindowsPhoneInfoUpdateColumns.ChannelUri },
                { WindowsPhoneInfoExtendedProperties.ChannelStatus, WindowsPhoneInfoUpdateColumns.ChannelStatus },
                { WindowsPhoneInfoExtendedProperties.SessionContext, WindowsPhoneInfoUpdateColumns.SessionContext },
                { WindowsPhoneInfoExtendedProperties.SessionWatermark, WindowsPhoneInfoUpdateColumns.SessionWatermark },
                { WindowsPhoneInfoExtendedProperties.Timestamp, WindowsPhoneInfoUpdateColumns.Timestamp },
                { WindowsPhoneInfoExtendedProperties.EnrollmentInfoId, WindowsPhoneInfoUpdateColumns.EnrollmentInfoId },
                { WindowsPhoneInfoExtendedProperties.IsRoboSupport, WindowsPhoneInfoUpdateColumns.IsRoboSupport },
                { WindowsPhoneInfoExtendedProperties.RenewPeriodDays, WindowsPhoneInfoUpdateColumns.RenewPeriodDays },
                { WindowsPhoneInfoExtendedProperties.RetryIntervalDays, WindowsPhoneInfoUpdateColumns.RetryIntervalDays },
                { WindowsPhoneInfoExtendedProperties.RoboStatusId, WindowsPhoneInfoUpdateColumns.RoboStatusId },
                { WindowsPhoneInfoExtendedProperties.DeviceId, WindowsPhoneInfoUpdateColumns.DeviceId },
                { WindowsPhoneInfoExtendedProperties.EnrollmentId, WindowsPhoneInfoUpdateColumns.EnrollmentId },
                { WindowsPhoneInfoExtendedProperties.TpmSpecDetails, WindowsPhoneInfoUpdateColumns.TpmSpecDetails },
                { WindowsPhoneInfoExtendedProperties.IsSMode, WindowsPhoneInfoUpdateColumns.IsSMode },
                { WindowsPhoneInfoExtendedProperties.IsUpdateApprovalRequired, WindowsPhoneInfoUpdateColumns.IsUpdateApprovalRequired },
                { WindowsPhoneInfoExtendedProperties.IsManageUpdates, WindowsPhoneInfoUpdateColumns.IsManageUpdates }
            };

        public static WindowsPhoneInfoUpdateColumns ToDataColumns(this IEnumerable<WindowsPhoneInfoExtendedProperties> extendedProperties)
        {
            if (extendedProperties == null)
            {
                return WindowsPhoneInfoUpdateColumns.None;
            }

            var includeFlags = WindowsPhoneInfoUpdateColumns.None;
            foreach (var prop in extendedProperties)
            {
                includeFlags |= ExtendedPropertiesMapper[prop];
            }

            return includeFlags;
        }
    }
}