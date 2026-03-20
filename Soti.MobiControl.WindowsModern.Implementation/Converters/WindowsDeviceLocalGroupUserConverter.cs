using System;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Converters;

/// <summary>
/// WindowsDeviceLocalGroupUser dat converter class.
/// </summary>
internal static class WindowsDeviceLocalGroupUserConverter
{
    /// <summary>
    /// Converts an instance of <see cref="WindowsDeviceLocalGroupUserDetailsData"/> to
    /// <see cref="WindowsDeviceLocalGroupUserDetailsModal"/>.
    /// </summary>
    /// <param name="localGroupUserDetailData">The source data model containing user details of the local group.</param>
    /// <param name="groupNameId">The name ID of the local group.</param>
    /// <returns>
    /// A <see cref="WindowsDeviceLocalGroupUserDetailsModal"/> containing the user details.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="localGroupUserDetailData"/> is null.
    /// </exception>
    public static WindowsDeviceLocalGroupUserDetailsModal ToWindowsDeviceLocalGroupUserDetailsModal(this WindowsDeviceLocalGroupUserDetailsData localGroupUserDetailData, int groupNameId)
    {
        if (localGroupUserDetailData == null)
        {
            throw new ArgumentNullException(nameof(localGroupUserDetailData));
        }

        return new WindowsDeviceLocalGroupUserDetailsModal
        {
            DeviceId = localGroupUserDetailData.DeviceId,
            WindowsDeviceUserId = localGroupUserDetailData.WindowsDeviceUserId,
            UserSid = localGroupUserDetailData.UserSid,
            UserName = localGroupUserDetailData.UserName,
            IsAdminGroup = localGroupUserDetailData.IsAdminGroup,
            GroupNameId = groupNameId
        };
    }
}
