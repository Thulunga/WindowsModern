using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for Windows Device User Data entity.
/// </summary>
internal interface IWindowsDeviceUserProvider
{
    /// <summary>
    /// Gets Local Users Data.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <returns> WindowsDeviceUserData.</returns>
    IReadOnlyList<WindowsDeviceUserData> GetAllLocalUsersDataByDeviceId(int deviceId);

    /// <summary>
    /// Gets Local Users Data.
    /// </summary>
    /// <returns> WindowsDeviceUserData.</returns>
    IReadOnlyList<WindowsDeviceUserData> GetAllLocalUsersData();

    /// <summary>
    /// Gets Local Users Basic Data.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <returns> WindowsDeviceUserData.</returns>
    IReadOnlyList<WindowsDeviceUserData> GetAllLocalUsersBasicDataByDeviceId(int deviceId);

    /// <summary>
    /// Gets Local Users Data.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="userSid">Device local user Id.</param>
    /// <returns> WindowsDeviceUserData.</returns>
    WindowsDeviceUserData GetLocalUsersDataByDeviceIdAndSid(int deviceId, string userSid);

    /// <summary>
    /// Gets Local Usernames.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="userSids">Device local user SIDs.</param>
    /// <returns> Names and IDs of local users.</returns>
    IReadOnlyList<WindowsDeviceLocalUserModel> GetLocalUserNamesByDeviceIdAndSids(int deviceId, IEnumerable<string> userSids);

    /// <summary>
    /// Gets Local Users Password.
    /// </summary>
    /// <param name="deviceId">Device Id.</param>
    /// <param name="userSid">Device local user Id.</param>
    /// <returns> WindowsDeviceUserData.</returns>
    WindowsDeviceUserData GetLocalUsersPasswordByDeviceIdAndSid(int deviceId, string userSid);

    /// <summary>
    /// Inserts WindowsDeviceUserData entity.
    /// </summary>
    /// <param name="windowsDeviceUserData">Entity to insert.</param>
    /// <remarks>
    /// The method modifies the <param name="windowsDeviceUserData"/> instance by setting its WindowsDeviceUserId property to the ID generated upon insertion.
    /// </remarks>
    void Insert(WindowsDeviceUserData windowsDeviceUserData);

    /// <summary>
    /// Updates WindowsDeviceUserData entity.
    /// </summary>
    /// <param name="windowsDeviceUserData">Entity to update.</param>>
    void Update(WindowsDeviceUserData windowsDeviceUserData);

    /// <summary>
    /// Deletes WindowsDeviceUserData entity.
    /// </summary>
    /// <param name="windowsDeviceUserId">WindowsDeviceUserId filter value.</param>
    void Delete(int windowsDeviceUserId);

    /// <summary>
    /// Updates the windows device user data.
    /// Removes all existing user data for device and replaces with new data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="windowsDeviceUserData">The windows device user data.</param>
    /// <returns>Dictionary of inserted users (UserSID, WindowsDeviceUserId).</returns>
    IDictionary<string, int> BulkReplace(int deviceId, IEnumerable<WindowsDeviceUserData> windowsDeviceUserData);

    /// <summary>
    /// Updates the windows device user data.
    /// If record already exists its updated else a new record is added.
    /// </summary>
    /// <param name="windowsDeviceUserData">The windows device user data.</param>
    IDictionary<string, int> BulkUpdate(IEnumerable<WindowsDeviceUserData> windowsDeviceUserData);

    /// <summary>
    /// Deletes the windows local user keys.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="windowsDeviceUserType">Windows device user type, set it to null to delete all users.</param>
    void DeleteByDeviceId(int deviceId, WindowsDeviceUserType? windowsDeviceUserType = null);

    /// <summary>
    /// Deletes the windows local user.
    /// </summary>
    /// <param name="userSid">UserSid.</param>
    /// <param name="deviceId">The device identifier.</param>
    void DeleteByUserSid(string userSid, int deviceId);

    /// <summary>
    /// Resets the windows local user password.
    /// </summary>
    /// <param name="windowsDeviceUserId">User Identifier.</param>
    void ResetPassword(int windowsDeviceUserId);

    /// <summary>
    /// Modifies the Windows Device Logged-In User Details.
    /// </summary>
    /// <param name="windowsDeviceUserData">The Windows Device User Data.</param>
    void ModifyLoggedInUser(WindowsDeviceUserData windowsDeviceUserData);

    /// <summary>
    /// Modifies the IsUserLoggedIn flag to false.
    /// </summary>
    /// <param name="deviceId">The device Id.</param>
    void LogOffUserByDeviceId(int deviceId);

    /// <summary>
    /// Gets the Windows Device Logged-In User Data.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <returns>WindowsDeviceUserData.</returns>
    WindowsDeviceUserData GetLoggedInUserDataByDeviceId(int deviceId);

    /// <summary>
    /// Gets the Windows Device Logged-In User Data by device ids.
    /// </summary>
    /// <param name="deviceIds">The device ids.</param>
    /// <returns>Collection of the windows device logged-in user data.</returns>
    IReadOnlyList<WindowsDeviceUserData> GetLoggedInUserDataByDeviceIds(IEnumerable<int> deviceIds);

    /// <summary>
    /// Gets the Windows Device UserSID by UserName and DeviceId.
    /// </summary>
    /// <param name="deviceId">Device ID.</param>
    /// <param name="userName">User Name</param>
    /// <returns>WindowsDeviceUserSidData.</returns>
    WindowsDeviceUserSidData GetUserSidByUserNameAndDeviceId(int deviceId, string userName);

    /// <summary>
    /// Gets the Windows Device UserSIDs by UserName and DeviceIds.
    /// </summary>
    /// <param name="deviceIds">Collection of Device IDs.</param>
    /// <param name="userName">User Name</param>
    /// <returns>Collection of WindowsDeviceUserSidData.</returns>
    IReadOnlyList<WindowsDeviceUserSidData> GetUserSidsByUserNameAndDeviceIds(IEnumerable<int> deviceIds, string userName);

    /// <summary>
    /// Updates the windows local user after rename.
    /// </summary>
    /// <param name="userSid">User SID.</param>
    /// <param name="deviceId">Device ID.</param>
    /// <param name="newUsername">New Username.</param>
    void UpdateUsernameByUserSidAndDeviceId(string userSid, int deviceId, string newUsername);

    /// <summary>
    /// Retrieves a collection of <see cref="WindowsDeviceUserData"/> records associated with the specified device IDs.
    /// </summary>
    /// <param name="deviceIds">A list of device Ids.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{WindowsDeviceUserData}"/> containing username data for the provided device IDs.
    /// </returns>
    public IReadOnlyList<WindowsDeviceUserData> GetUsernameByDeviceIds(IEnumerable<int> deviceIds);

    /// <summary>
    /// Updates the windows local user username.
    /// </summary>
    /// <param name="decryptedUserName">Decrypted username</param>
    void WindowsDeviceUserNameBulkUpdate(IEnumerable<DecryptedLocalUserModel> decryptedUserName);

    /// <summary>
    /// Gets data key id by device user id.
    /// </summary>
    /// <param name="windowsDeviceUserIds">Windows Device User Id</param>
    /// <returns> Returns a dictionary with WindowsDeviceUserId and DataKeyId as key value pair.</returns>
    IDictionary<int, int> GetDataKeyIdsByWindowsDeviceUserIds(IEnumerable<int> windowsDeviceUserIds);
}
