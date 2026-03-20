using System;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Converters
{
    /// <summary>
    /// Converts to device local user summary.
    /// </summary>
    internal static class DeviceLocalUserConverter
    {
        /// <summary>
        /// DeviceLocalUserSummary Converter.
        /// </summary>
        /// <param name="deviceLocalUserSummary">The Local User Model.</param>
        /// <returns>DeviceLocalUserSummary.</returns>
        public static Contracts.DeviceLocalUserSummary ToDeviceLocalUserSummary(this Models.DeviceLocalUserSummary deviceLocalUserSummary)
        {
            if (deviceLocalUserSummary == null)
            {
                throw new ArgumentNullException(nameof(deviceLocalUserSummary));
            }

            return new Contracts.DeviceLocalUserSummary
            {
                UserName = deviceLocalUserSummary.UserName,
                UserGroups = deviceLocalUserSummary.UserGroups,
                Sid = deviceLocalUserSummary.Sid,
                IsMobiControlCreated = deviceLocalUserSummary.IsMobiControlCreated
            };
        }
    }
}
