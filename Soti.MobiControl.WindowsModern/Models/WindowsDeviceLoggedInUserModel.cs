using System;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Models;

public sealed class WindowsDeviceLoggedInUserModel
{
    /// <summary>
    /// Gets or sets WindowsDeviceUserId.
    /// </summary>
    public int WindowsDeviceUserId { get; set; }

    /// <summary>
    /// Gets or sets DeviceId.
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// Gets or sets devId of the device.
    /// </summary>
    public string DevId { get; set; }

    /// <summary>
    /// Gets or sets User Name.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the UserFullName.
    /// </summary>
    public string UserFullName { get; set; }

    /// <summary>
    /// Gets or sets the UserDomain if the the user type is 1 - Domain.
    /// </summary>
    public string UserDomain { get; set; }

    /// <summary>
    /// Gets or sets SID.
    /// </summary>
    public string UserSID { get; set; }

    /// <summary>
    /// Gets or sets IsUserLoggedIn defining if the user is logged in.
    /// Table default value: false.
    /// </summary>
    public bool IsUserLoggedIn { get; set; }

    /// <summary>
    /// Gets or sets the WindowsDeviceUserType. It can be 0 - Local or 1 - Domain.
    /// </summary>
    public WindowsDeviceUserType WindowsDeviceUserType { get; set; }

    /// <summary>
    /// Gets or sets the LastCheckInDeviceUserTime.
    /// </summary>
    public DateTime? LastCheckInDeviceUserTime { get; set; }
}
