using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern;

public interface IWindowsDeviceLocalUsersService
{
    /// <summary>
    /// Returns the device local users summary info.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <returns>Local Users info for the deviceId.</returns>
    IReadOnlyList<DeviceLocalUserSummary> GetDeviceLocalUsersSummaryInfo(int deviceId);

    /// <summary>
    /// Returns the device local users summary info for upgrade.
    /// </summary>
    /// <returns>Local Users info for the deviceId.</returns>
    IReadOnlyList<WindowsDeviceLocalUserModel> GetDeviceLocalUsersSummary();

    /// <summary>
    /// Returns the device local user's info.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="userSid">The device Local User id.</param>
    /// <returns>Local User's info for the deviceId and user id.</returns>
    WindowsDeviceLocalUserModel GetLocalUserByDeviceIdAndSid(int deviceId, string userSid);

    /// <summary>
    /// Returns the IDs and names of local users.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="userSids">The device Local User SIDs.</param>
    /// <returns>IDs and names of local users for the deviceId and user SIDs.</returns>
    IReadOnlyList<WindowsDeviceLocalUserModel> GetLocalUserNamesByDeviceIdAndSids(int deviceId, IEnumerable<string> userSids);

    /// <summary>
    /// Returns the device local user's info.
    /// </summary>
    /// <param name="deviceId">The device id.</param>
    /// <param name="userSid">The device Local User id.</param>
    /// <returns>Local User's password for the deviceId and user id.</returns>
    WindowsLocalUserNameAndPasswordModel GetLocalUserPasswordByDeviceIdAndSid(int deviceId, string userSid);

    /// <summary>
    /// Processes the Windows LocalUser keys data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="localUserKeysData">The Windows LocalUser keys data.</param>
    /// <param name="groupData">The group names data of the device.</param>
    void SynchronizeLocalUserDataWithSnapshot(int deviceId, string localUserKeysData, IEnumerable<WindowsDeviceLocalGroup> groupData);

    /// <summary>
    /// Updates the windows device local users data.
    /// If record already exists its updated else a new record is added.
    /// </summary>
    /// <param name="localUsers">The windows local users.</param>
    void BulkUpdateLocalUsers(ICollection<WindowsDeviceLocalUserModel> localUsers);

    /// <summary>
    /// Deletes the Windows LocalUser.
    /// </summary>
    /// <param name="userSid">UserSid.</param>
    /// <param name="deviceId">The device identifier.</param>
    void DeleteLocalUser(string userSid, int deviceId);

    /// <summary>
    /// Update LocalUser Password.
    /// </summary>
    /// <param name="userSid">UserSid.</param>
    /// <param name="deviceId">The device identifier.</param>
    void UpdateLocalUserPassword(string userSid, int deviceId);

    /// <summary>
    /// Deletes the windows device user data based on deviceId.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="onlyLocalUsers">Delete all users or all the user data.</param>
    void DeleteUserData(int deviceId, bool onlyLocalUsers = false);

    /// <summary>
    /// Returns the user SIDs in each device corresponding to given username.
    /// </summary>
    /// <param name="deviceIds">The collection of Device IDs.</param>
    /// <param name="userName">User Name.</param>
    /// <returns>Dictionary of user SIDs corresponding to each Device ID.</returns>
    IDictionary<int, string> GetUserSids(IEnumerable<int> deviceIds, string userName);

    /// <summary>
    /// Creates device log for Windows multi users device actions.
    /// </summary>
    /// <param name="deviceId">DeviceId.</param>
    /// <param name="devId">DevId.</param>
    /// <param name="action">Action.</param>
    /// <param name="users">Users.</param>
    void GenerateUsersLog(int deviceId, string devId, string action, IEnumerable<string> users);

    /// <summary>
    /// Renames the Windows local user.
    /// </summary>
    /// <param name="userSid">User SID.</param>
    /// <param name="newUsername">New Username.</param>
    /// <param name="deviceId">Device ID.</param>
    void RenameLocalUser(string userSid, string newUsername, int deviceId);

    /// <summary>
    /// Retrieves a mapping of device IDs to their corresponding user data collections.
    /// </summary>
    /// <param name="deviceIds">A collection of device IDs for which to retrieve user data.</param>
    /// <returns>
    /// A dictionary where each key is a device ID and the corresponding to an <see cref="IEnumerable{WindowsDeviceLocalUserModel}"/>
    /// </returns>
    public IDictionary<int, IEnumerable<WindowsDeviceLocalUserModel>> GetUsernameDataByDeviceIds(IEnumerable<int> deviceIds);

    /// <summary>
    /// Retreives all the encrypted users from temporary Local user table.
    /// </summary>
    /// <param name="batchSize">Batch size.</param>
    /// <returns>Encrypted local User's info</returns>
    IEnumerable<WindowsDeviceTmpLocalUserModel> GetAllTmpLocalUsers(int batchSize = 1000);

    /// <summary>
    /// Gets data key ids by user ids.
    /// </summary>
    /// <param name="userIds">User Id</param>
    /// <returns>Returns a dictionary with UserId and DataKeyId as key value pair.</returns>
    public IDictionary<int, int> GetDataKeyIdByUserId(IEnumerable<int> userIds);

    /// <summary>
    /// Updates the windows local user username.
    /// </summary>
    /// <param name="decryptedTmpUsers">Decrypted username</param>
    void UpdateDecryptedLocalUsers(IEnumerable<DecryptedLocalUserModel> decryptedTmpUsers);

    /// <summary>
    /// Deletes local user details from temporary local user table.
    /// </summary>
    /// <param name="decryptedTmpUsers">Windows Device User Id</param>
    void DeleteTmpLocalUsers(IEnumerable<WindowsDeviceTmpLocalUserModel> decryptedTmpUsers);
}
