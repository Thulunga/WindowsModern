using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Soti.Diagnostics;
using Soti.MobiControl.Devices;
using Soti.MobiControl.Events;
using Soti.MobiControl.Exceptions;
using Soti.MobiControl.Search.Database;
using Soti.MobiControl.Search.Database.Enums;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Events;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.MobiControl.WindowsModern.Models.Enums;
using Soti.SensitiveDataProtection;
using Soti.SensitiveDataProtection.Model;
using Soti.Utilities.Collections;

namespace Soti.MobiControl.WindowsModern.Implementation;

internal sealed class WindowsDeviceLocalUsersService : IWindowsDeviceLocalUsersService
{
    private const string Delimiter = "#";
    private const string Separator = ",";
    private const string ExceptionErrorMessage = "cannot be empty";

    private const string RegexPattern = "^UserSid:([^,]+),UserName:([^,]+),PasswordLastModified:(.+?),Groups:(.*?)$";

    private const string LogName = "LocalUser";

    private static readonly char[] DelimiterArray = Delimiter.ToArray();

    private readonly IProgramTrace _programTrace;
    private readonly IWindowsDeviceUserProvider _windowsDeviceUserProvider;
    private readonly ITmpWindowsDeviceUserDataProvider _tmpWindowsDeviceUserDataProvider;
    private readonly IWindowsDeviceLocalGroupsService _windowsDeviceLocalGroupsService;
    private readonly IDataKeyService _dataKeyService;
    private readonly ISensitiveDataEncryptionService _sensitiveDataEncryptionService;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IDeviceKeyInformationRetrievalService _deviceKeyInformationRetrievalService;
    private readonly IWindowsDeviceLocalGroupProvider _windowsDeviceLocalGroupProvider;
    private readonly IDeviceSearchInfoService _deviceSearchInfoService;

    private readonly ConcurrentDictionary<int, DataKey> _dataKeyDictionary = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceLocalUsersService"/> class.
    /// </summary>
    /// <param name="programTrace">The program trace.</param>
    /// <param name="windowsDeviceUserProvider">The windows device user provider.</param>
    /// <param name="tmpWindowsDeviceUserDataProvider">The temporary windows device user provider.</param>
    /// <param name="windowsDeviceLocalGroupsService">The windows device local group service.</param>
    /// <param name="dataKeyService">The Data Key Service.</param>
    /// <param name="deviceKeyInformationRetrievalService">The Device Key Information Service.</param>
    /// <param name="sensitiveDataEncryptionService">The Sensitive Data Encryption Service.</param>
    /// <param name="eventDispatcher">The Event Dispatched Service.</param>
    /// <param name="windowsDeviceLocalGroupProvider">The windows device local group provider.</param>
    /// <param name="deviceSearchInfoService">The device search info service.</param>
    public WindowsDeviceLocalUsersService(
        IProgramTrace programTrace,
        IWindowsDeviceUserProvider windowsDeviceUserProvider,
        ITmpWindowsDeviceUserDataProvider tmpWindowsDeviceUserDataProvider,
        IWindowsDeviceLocalGroupsService windowsDeviceLocalGroupsService,
        IDataKeyService dataKeyService,
        IEventDispatcher eventDispatcher,
        IDeviceKeyInformationRetrievalService deviceKeyInformationRetrievalService,
        ISensitiveDataEncryptionService sensitiveDataEncryptionService,
        IWindowsDeviceLocalGroupProvider windowsDeviceLocalGroupProvider,
        IDeviceSearchInfoService deviceSearchInfoService)
    {
        _programTrace = programTrace ?? throw new ArgumentNullException(nameof(programTrace));
        _windowsDeviceUserProvider = windowsDeviceUserProvider ?? throw new ArgumentNullException(nameof(windowsDeviceUserProvider));
        _tmpWindowsDeviceUserDataProvider = tmpWindowsDeviceUserDataProvider ?? throw new ArgumentNullException(nameof(tmpWindowsDeviceUserDataProvider));
        _windowsDeviceLocalGroupsService = windowsDeviceLocalGroupsService ?? throw new ArgumentNullException(nameof(windowsDeviceLocalGroupsService));
        _dataKeyService = dataKeyService ?? throw new ArgumentNullException(nameof(dataKeyService));
        _sensitiveDataEncryptionService = sensitiveDataEncryptionService ?? throw new ArgumentNullException(nameof(sensitiveDataEncryptionService));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _deviceKeyInformationRetrievalService = deviceKeyInformationRetrievalService ?? throw new ArgumentNullException(nameof(deviceKeyInformationRetrievalService));
        _windowsDeviceLocalGroupProvider = windowsDeviceLocalGroupProvider ?? throw new ArgumentNullException(nameof(windowsDeviceLocalGroupProvider));
        _deviceSearchInfoService = deviceSearchInfoService ?? throw new ArgumentNullException(nameof(deviceSearchInfoService));
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceLocalUserSummary> GetDeviceLocalUsersSummaryInfo(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var deviceLocalUsersData =
            _windowsDeviceUserProvider.GetAllLocalUsersBasicDataByDeviceId(deviceId)?.ToArray();

        var deviceLocalUserModels = new List<DeviceLocalUserSummary>();

        if (deviceLocalUsersData == null || deviceLocalUsersData.Length == 0)
        {
            return deviceLocalUserModels;
        }

        var windowsDeviceUserIds = deviceLocalUsersData.Select(x => x.WindowsDeviceUserId);
        var userGroupsLookup = _windowsDeviceLocalGroupsService.GetUserGroupsByWindowsDeviceUserIds(windowsDeviceUserIds);
        foreach (var userData in deviceLocalUsersData)
        {
            deviceLocalUserModels.Add(userData.ToDeviceLocalUsersBasicModel(userGroupsLookup[userData.WindowsDeviceUserId]));
        }

        return deviceLocalUserModels;
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsDeviceLocalUserModel> GetDeviceLocalUsersSummary()
    {
        var deviceLocalUsersData =
            _windowsDeviceUserProvider.GetAllLocalUsersData()?.ToArray();
        var deviceLocalUserModels = new List<WindowsDeviceLocalUserModel>();
        if (deviceLocalUsersData == null)
        {
            return deviceLocalUserModels;
        }

        foreach (var userData in deviceLocalUsersData)
        {
            deviceLocalUserModels.Add(userData.ToDeviceLocalUsersBasicModel());
        }

        return deviceLocalUserModels;
    }

    /// <inheritdoc />
    public WindowsLocalUserNameAndPasswordModel GetLocalUserPasswordByDeviceIdAndSid(int deviceId, string userSid)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(userSid))
        {
            throw new ArgumentException(nameof(userSid));
        }

        var deviceLocalUsersData =
            _windowsDeviceUserProvider.GetLocalUsersPasswordByDeviceIdAndSid(deviceId, userSid);

        if (deviceLocalUsersData == null)
        {
            return null;
        }

        if (deviceLocalUsersData.AutoGeneratedPassword == null)
        {
            return new WindowsLocalUserNameAndPasswordModel
            {
                AutoGeneratedPassword = null,
                UserName = null
            };
        }

        var dataKey = GetCachedDataKey(deviceLocalUsersData.DataKeyId);
        var decryptedPassword =
            Encoding.UTF8.GetString(
                _sensitiveDataEncryptionService.Decrypt(deviceLocalUsersData.AutoGeneratedPassword, dataKey));
        return new WindowsLocalUserNameAndPasswordModel
        {
            AutoGeneratedPassword = decryptedPassword,
            UserName = deviceLocalUsersData.UserName
        };
    }

    /// <inheritdoc />
    public WindowsDeviceLocalUserModel GetLocalUserByDeviceIdAndSid(int deviceId, string userSid)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(userSid))
        {
            throw new ArgumentException(nameof(userSid));
        }

        var deviceLocalUsersData = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(deviceId, userSid);
        if (deviceLocalUsersData == null)
        {
            throw new InternalLogicException($"No local user with SID {userSid} in the device {deviceId}");
        }

        string decryptedPassword = null;
        var dataKey = GetCachedDataKey(deviceLocalUsersData.DataKeyId);
        if (deviceLocalUsersData.AutoGeneratedPassword != null)
        {
            decryptedPassword = Encoding.UTF8.GetString(
                _sensitiveDataEncryptionService.Decrypt(deviceLocalUsersData.AutoGeneratedPassword, dataKey));
        }

        var windowsDeviceUserId = deviceLocalUsersData.WindowsDeviceUserId;
        var userGroupsLookUp = _windowsDeviceLocalGroupsService.GetUserGroupsByWindowsDeviceUserIds(new List<int> { windowsDeviceUserId });
        var userGroups = userGroupsLookUp[windowsDeviceUserId];

        var deviceLocalUserModel =
            deviceLocalUsersData.ToDeviceLocalUsersModel(decryptedPassword, userGroups);
        return deviceLocalUserModel;
    }

    public IReadOnlyList<WindowsDeviceLocalUserModel> GetLocalUserNamesByDeviceIdAndSids(int deviceId, IEnumerable<string> userSids)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (userSids == null)
        {
            throw new ArgumentNullException(nameof(userSids));
        }

        return userSids.SwitchByCount(
            sid =>
            {
                var userData = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(deviceId, sid)?.ToDeviceLocalUsersModel(null, null);
                if (userData == null)
                {
                    return [];
                }

                userData.SID = sid;
                return [userData];
            },
            sids => _windowsDeviceUserProvider.GetLocalUserNamesByDeviceIdAndSids(deviceId, sids),
            () => new List<WindowsDeviceLocalUserModel>()
        );
    }

    /// <inheritdoc />
    public void SynchronizeLocalUserDataWithSnapshot(int deviceId, string localUserKeysData, IEnumerable<WindowsDeviceLocalGroup> groupData)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(localUserKeysData))
        {
            DeleteUserData(deviceId, true);
            return;
        }

        var snapshotData = ParseLocalUserInfoKeys(deviceId, localUserKeysData).ToArray();

        if (!snapshotData.Any())
        {
            return;
        }

        var localUserData = _windowsDeviceUserProvider.GetAllLocalUsersDataByDeviceId(deviceId)?.ToArray();

        if (localUserData is { Length: > 0 })
        {
            var windowsDeviceUserIds = localUserData.Select(x => x.WindowsDeviceUserId);
            var userGroupsLookup = _windowsDeviceLocalGroupsService.GetUserGroupsByWindowsDeviceUserIds(windowsDeviceUserIds);
            foreach (var user in localUserData)
            {
                user.UserGroups = userGroupsLookup[user.WindowsDeviceUserId];
            }
        }

        // If the local users count is same and other parameters is matching, then we don't need to update the records.
        if (AreLocalUserKeysEqual(localUserData, snapshotData, deviceId))
        {
            return;
        }

        var localUser = GetLocalUsersList(snapshotData, localUserData);
        var localUserOutput = _windowsDeviceUserProvider.BulkReplace(deviceId, localUser);
        var groupMemberships = localUser.ToDictionary(x => x.UserSID, y => y.UserGroups.ToList());

        _windowsDeviceLocalGroupsService.BulkModifyLocalGroupUser(deviceId, groupMemberships, localUserOutput, groupData);

        _programTrace.Write(TraceLevel.Verbose, LogName, $"Windows LocalUser key(s) information updated for {deviceId}");
    }

    /// <inheritdoc />
    public void DeleteUserData(int deviceId, bool onlyLocalUsers = false)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceUserProvider.DeleteByDeviceId(deviceId, onlyLocalUsers ? WindowsDeviceUserType.Local : null);
        _programTrace.Write(TraceLevel.Verbose, LogName, $"Windows LocalUser key(s) information deleted for {deviceId}");
    }

    /// <inheritdoc />
    public void BulkUpdateLocalUsers(ICollection<WindowsDeviceLocalUserModel> localUsers)
    {
        if (localUsers == null)
        {
            throw new ArgumentNullException(nameof(localUsers));
        }

        var windowsDeviceLocalUserModels = localUsers.ToArray();
        if (windowsDeviceLocalUserModels.Any(u => u == null) || !windowsDeviceLocalUserModels.Any())
        {
            throw new ArgumentNullException(nameof(localUsers));
        }

        var localUsersData = new List<WindowsDeviceUserData>();
        var dataKey = _dataKeyService.GetKey();
        foreach (var user in windowsDeviceLocalUserModels)
        {
            byte[] encryptedPassword = null;
            if (!string.IsNullOrWhiteSpace(user.AutoGeneratedPassword))
            {
                encryptedPassword =
                    _sensitiveDataEncryptionService.Encrypt(Encoding.UTF8.GetBytes(user.AutoGeneratedPassword), dataKey);
            }

            var userData = user.ToWindowsDeviceUserData(encryptedPassword, dataKey.Id);
            localUsersData.Add(userData);
        }

        var deviceId = localUsersData[0].DeviceId;
        IDictionary<string, int> localUserOutput = new Dictionary<string, int>();
        if (localUsersData.Count == 1)
        {
            var localUser = localUsersData[0];
            _windowsDeviceUserProvider.Insert(localUser);
            localUserOutput.Add(localUser.UserSID, localUser.WindowsDeviceUserId);
        }
        else
        {
            localUserOutput = _windowsDeviceUserProvider.BulkUpdate(localUsersData);
        }

        var userGroupIds = new Dictionary<string, int>();
        var userGroups = windowsDeviceLocalUserModels.SelectMany(x => x.UserGroups).Distinct();

        foreach (var userGroup in userGroups)
        {
            var groupNameId = _windowsDeviceLocalGroupsService.GetGroupNameId(userGroup.Trim());
            int groupId;
            if (groupNameId == 0)
            {
                _windowsDeviceLocalGroupProvider.InsertLocalGroupName(userGroup, out groupNameId);
                _windowsDeviceLocalGroupProvider.UpsertWindowsDeviceLocalGroup(deviceId, groupNameId, out groupId);
            }
            else
            {
                groupId = _windowsDeviceLocalGroupProvider.GetGroupIdByNameId(deviceId, groupNameId);

                if (groupId == 0)
                {
                    _windowsDeviceLocalGroupProvider.UpsertWindowsDeviceLocalGroup(deviceId, groupNameId, out groupId);
                }
            }

            userGroupIds.Add(userGroup, groupId);
        }

        foreach (var user in windowsDeviceLocalUserModels)
        {
            var windowsDeviceUserId = localUserOutput[user.SID];
            foreach (var userGroup in user.UserGroups)
            {
                var groupId = userGroupIds[userGroup];
                if (groupId > 0)
                {
                    _windowsDeviceLocalGroupProvider.InsertLocalGroupUser(windowsDeviceUserId, groupId);
                }
            }
        }
    }

    /// <inheritdoc />
    public void DeleteLocalUser(string userSid, int deviceId)
    {
        if (string.IsNullOrWhiteSpace(userSid))
        {
            throw new ArgumentException(nameof(userSid));
        }

        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceUserProvider.DeleteByUserSid(userSid, deviceId);
    }

    /// <inheritdoc />
    public void UpdateLocalUserPassword(string userSid, int deviceId)
    {
        if (string.IsNullOrWhiteSpace(userSid))
        {
            throw new ArgumentException(nameof(userSid));
        }

        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var localUserModel = GetLocalUserByDeviceIdAndSid(deviceId, userSid);

        if (localUserModel == null)
        {
            throw new InternalLogicException($"No local user with SID {userSid} in the device {deviceId}");
        }

        _windowsDeviceUserProvider.ResetPassword(localUserModel.WindowsDeviceLocalUserId);
        _programTrace.Write(TraceLevel.Information, LogName, $"Password of {userSid} has been updated");
    }

    /// <exception cref="ArgumentNullException"></exception>
    /// <inheritdoc />
    public IDictionary<int, string> GetUserSids(IEnumerable<int> deviceIds, string userName)
    {
        var deviceIdsList = deviceIds.AsArray();

        if (deviceIdsList.Count == 0)
        {
            throw new ArgumentNullException(nameof(deviceIds), ExceptionErrorMessage);
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("cannot be null or empty", nameof(userName));
        }

        if (deviceIdsList.Count == 1)
        {
            var deviceId = deviceIdsList[0];
            var sidData = _windowsDeviceUserProvider.GetUserSidByUserNameAndDeviceId(deviceId, userName);
            return sidData == null ? [] : new Dictionary<int, string> { [deviceId] = sidData.UserSID };
        }

        var sidsData = _windowsDeviceUserProvider.GetUserSidsByUserNameAndDeviceIds(deviceIdsList, userName);
        return sidsData?.ToDictionary(sidData => sidData.DeviceId, sidData => sidData.UserSID);
    }

    /// <inheritdoc />
    public void GenerateUsersLog(int deviceId, string devId, string action, IEnumerable<string> users)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(devId))
        {
            throw new ArgumentException($"{nameof(devId)} cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException($"{nameof(action)} cannot be null or empty");
        }

        var usersList = users.AsArray();

        if (usersList.Count == 0)
        {
            throw new ArgumentNullException(nameof(users), ExceptionErrorMessage);
        }

        _eventDispatcher.DispatchEvent(new WindowsMultiLocalUsersActionEvent(deviceId, devId, action, string.Join(", ", usersList)));
    }

    /// <inheritdoc />
    public void RenameLocalUser(string userSid, string newUsername, int deviceId)
    {
        if (string.IsNullOrWhiteSpace(userSid))
        {
            throw new ArgumentException($"{nameof(userSid)} cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(newUsername))
        {
            throw new ArgumentException($"{nameof(newUsername)} cannot be null or empty");
        }

        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceUserProvider.UpdateUsernameByUserSidAndDeviceId(userSid, deviceId, newUsername);
        _deviceSearchInfoService.AddOrUpdateDevices([deviceId], OperationPriority.Normal, false);
    }

    /// <inheritdoc />
    public IDictionary<int, IEnumerable<WindowsDeviceLocalUserModel>> GetUsernameDataByDeviceIds(IEnumerable<int> deviceIds)
    {
        var deviceIdsList = deviceIds.AsArray();

        if (deviceIdsList.Count == 0)
        {
            throw new ArgumentNullException(nameof(deviceIds));
        }

        var usernameData = _windowsDeviceUserProvider.GetUsernameByDeviceIds(deviceIdsList);

        var deviceUserMap = new Dictionary<int, List<WindowsDeviceLocalUserModel>>();

        foreach (var user in usernameData)
        {
            var localUser = new WindowsDeviceLocalUserModel
            {
                WindowsDeviceLocalUserId = user.WindowsDeviceUserId,
                UserName = user.UserName,
                DeviceId = user.DeviceId,
            };

            if (!deviceUserMap.TryGetValue(user.DeviceId, out var userList))
            {
                userList = new List<WindowsDeviceLocalUserModel>();
                deviceUserMap[user.DeviceId] = userList;
            }

            userList.Add(localUser);
        }

        return deviceUserMap.ToDictionary(
            kvp => kvp.Key, IEnumerable<WindowsDeviceLocalUserModel> (kvp) => kvp.Value
        );
    }

    /// <inheritdoc/>
    public IEnumerable<WindowsDeviceTmpLocalUserModel> GetAllTmpLocalUsers(int batchSize = 1000)
    {
        ValidateBatchSize(batchSize);
        return GetAllTmpLocalUsersIterator(batchSize);
    }

    /// <inheritdoc/>
    public void DeleteTmpLocalUsers(IEnumerable<WindowsDeviceTmpLocalUserModel> decryptedTmpUsers)
    {
        var usersList = decryptedTmpUsers.AsArray();

        if (usersList.Count == 0)
        {
            throw new ArgumentNullException(nameof(decryptedTmpUsers), ExceptionErrorMessage);
        }

        var userIds = usersList.Select(user => user.WindowsDeviceUserId).AsArray();
        _tmpWindowsDeviceUserDataProvider.Delete(userIds);
    }

    /// <inheritdoc/>
    public void UpdateDecryptedLocalUsers(IEnumerable<DecryptedLocalUserModel> decryptedTmpUsers)
    {
        var decryptedTmpUsersList = decryptedTmpUsers.AsArray();

        if (decryptedTmpUsersList.Count == 0)
        {
            throw new ArgumentNullException(nameof(decryptedTmpUsers), ExceptionErrorMessage);
        }

        _windowsDeviceUserProvider.WindowsDeviceUserNameBulkUpdate(decryptedTmpUsersList);
    }

    /// <inheritdoc/>
    public IDictionary<int, int> GetDataKeyIdByUserId(IEnumerable<int> userIds)
    {
        var userIdsList = userIds.AsArray();

        return userIdsList.Count == 0
            ? throw new ArgumentNullException(nameof(userIds), "cannot be null or empty")
            : _windowsDeviceUserProvider.GetDataKeyIdsByWindowsDeviceUserIds(userIdsList);
    }

    private List<WindowsDeviceTmpLocalUserModel> GetAllTmpLocalUsersIterator(int batchSize)
    {
        var allUsers = new List<WindowsDeviceTmpLocalUserModel>();

        for (var skip = 0; ; skip += batchSize)
        {
            var batch = _tmpWindowsDeviceUserDataProvider.GetTmpLocalUsersBatch(skip, batchSize).ToList();
            allUsers.AddRange(batch);

            if (batch.Count < batchSize)
            {
                break;
            }
        }

        return allUsers;
    }

    private static void ValidateBatchSize(int batchSize)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be positive");
        }
    }

    private static bool AreGroupsEqual(IEnumerable<string> localUserData, IEnumerable<string> snapshotData)
    {
        if (localUserData == null && snapshotData == null)
        {
            return true; // Both sequences are null, consider them equal
        }

        if (localUserData == null || snapshotData == null)
        {
            return false;
        }

        return localUserData.OrderBy(s => s.Trim(), StringComparer.Ordinal)
            .SequenceEqual(snapshotData.OrderBy(s => s.Trim(), StringComparer.Ordinal));
    }

    private static bool IsUserInfoEqual(WindowsDeviceUserData localUserData, WindowsDeviceLocalUserSnapShot snapshotData)
    {
        if (!string.Equals(localUserData.UserName, snapshotData.UserName, StringComparison.Ordinal))
        {
            return false;
        }

        return AreGroupsEqual(localUserData.UserGroups, snapshotData.UserGroups);
    }

    private bool IsPasswordNotModified(WindowsDeviceUserData localUserData, WindowsDeviceLocalUserSnapShot snapshotData)
    {
        if (localUserData.AutoGeneratedPassword == null)
        {
            return true;
        }

        _programTrace.Write(TraceLevel.Verbose, LogName, $"WindowsDeviceLocalUserService: Password last modified time {snapshotData.PasswordLastModified} Local user created at {localUserData.CreatedDate}");
        var timeDifference = snapshotData.PasswordLastModified - localUserData.CreatedDate;
        return timeDifference?.TotalMinutes < 2;
    }

    private WindowsDeviceUserData CreateWindowsDeviceLocalUserData(WindowsDeviceUserData localUserData, WindowsDeviceLocalUserSnapShot snapshotData, int dataKeyId, string userName = null)
    {
        var isMobiControlUser = localUserData?.IsMobiControlCreated == true;
        var isPasswordNotModified = isMobiControlUser && IsPasswordNotModified(localUserData, snapshotData);
        return new WindowsDeviceUserData
        {
            UserSID = snapshotData.SID,
            UserName = userName,
            UserGroups = snapshotData.UserGroups,
            DeviceId = snapshotData.DeviceId,
            CreatedDate = localUserData?.CreatedDate ?? snapshotData.PasswordLastModified,
            IsMobiControlCreated = isMobiControlUser,
            AutoGeneratedPassword = isPasswordNotModified ? localUserData.AutoGeneratedPassword : null,
            DataKeyId = dataKeyId
        };
    }

    private bool IsLocalUserModelEqual(WindowsDeviceUserData localUserData, WindowsDeviceLocalUserSnapShot snapshotData, string devId)
    {
        var isPasswordUpdated = !IsPasswordNotModified(localUserData, snapshotData);
        if (isPasswordUpdated)
        {
            _eventDispatcher.DispatchEvent(new LocalUserUpdatedFromDeviceEvent(localUserData.DeviceId, devId, localUserData.UserName));
        }

        return localUserData.IsMobiControlCreated
            ? IsUserInfoEqual(localUserData, snapshotData) &&
              IsPasswordNotModified(localUserData, snapshotData)
            : IsUserInfoEqual(localUserData, snapshotData);
    }

    private bool AreLocalUserKeysEqual(WindowsDeviceUserData[] localUserData, WindowsDeviceLocalUserSnapShot[] snapshotData, int deviceId)
    {
        if (localUserData == null || snapshotData == null)
        {
            return false;
        }

        var deletedLocalUserList = new List<string>();
        var renamedLocalUserList = new List<WindowsDeviceLocalUserRenameData>();

        foreach (var localUserKey in localUserData)
        {
            var matchingUser = snapshotData.FirstOrDefault(snapshotKey => snapshotKey.SID == localUserKey.UserSID);

            //if a matching sid is found then user exists
            if (matchingUser != null)
            {
                // check if user name is different then user has been renamed
                if (matchingUser.UserName != localUserKey.UserName)
                {
                    renamedLocalUserList.Add(new WindowsDeviceLocalUserRenameData() { NewUserName = matchingUser.UserName, OldUserName = localUserKey.UserName });
                }
            }
            else
            {
                // If the sid does not exist in snapshotData, add it to the list of deleted users
                deletedLocalUserList.Add(localUserKey.UserName);
            }
        }

        var devId = _deviceKeyInformationRetrievalService.GetDeviceKeyInformation(deviceId).DevId;

        foreach (var deletedLocalUser in deletedLocalUserList)
        {
            _eventDispatcher.DispatchEvent(
                new LocalUserDeletedFromDeviceEvent(deviceId, devId, deletedLocalUser));
        }

        if (renamedLocalUserList.Count > 0)
        {
            _eventDispatcher.DispatchEvent(
                new LocalUserRenamedFromDeviceEvent(deviceId, devId, renamedLocalUserList));

            return false;
        }

        if (deletedLocalUserList.Count > 0)
        {
            return false;
        }

        foreach (var snapshot in snapshotData)
        {
            var matchingUser = localUserData
                .FirstOrDefault(localUser => snapshot.SID.Equals(localUser.UserSID, StringComparison.Ordinal));
            if (matchingUser == null)
            {
                return false;
            }

            if (!IsLocalUserModelEqual(matchingUser, snapshot, devId))
            {
                return false;
            }
        }

        return true;
    }

    private List<WindowsDeviceLocalUserSnapShot> ParseLocalUserInfoKeys(int deviceId, string localUserData)
    {
        try
        {
            var localUserKeyArray = localUserData.Split(DelimiterArray, StringSplitOptions.RemoveEmptyEntries);

            var regexPattern = new Regex(RegexPattern, RegexOptions.None, TimeSpan.FromSeconds(1));

            var localUserInfoKeys = new List<WindowsDeviceLocalUserSnapShot>(capacity: localUserKeyArray.Length);

            foreach (var localUserKeysString in localUserKeyArray)
            {
                var matchResult = regexPattern.Match(localUserKeysString);

                if (!matchResult.Success)
                {
                    throw new FormatException();
                }

                var result = matchResult.Groups.OfType<Group>().Skip(1)
                    .Select(m => m.Value).ToArray();

                const int sidIndex = 0;
                const int userNameIndex = 1;
                const int passwordLastModifiedIndex = 2;
                const int userGroupsIndex = 3;

                localUserInfoKeys.Add(new WindowsDeviceLocalUserSnapShot
                {
                    DeviceId = deviceId,
                    SID = result[sidIndex],
                    UserName = result[userNameIndex],
                    UserGroups = result[userGroupsIndex]
                        .Split(Separator.ToArray(), StringSplitOptions.RemoveEmptyEntries),
                    PasswordLastModified = DateTime
                        .Parse(result[passwordLastModifiedIndex], CultureInfo.InvariantCulture).ToUniversalTime()
                });
            }

            return localUserInfoKeys;
        }
        catch (FormatException ex)
        {
            _programTrace.Write(TraceLevel.Error, LogName, $"Unable to process Windows LocalUser key information for {deviceId} as the input string was not in the proper format.");
            _programTrace.Write(TraceLevel.Error, LogName, ex.Message);
            return [];
        }
    }

    private DataKey GetCachedDataKey(int dataKeyId)
    {
        return _dataKeyDictionary.GetOrAdd(dataKeyId, _ => _dataKeyService.GetKey(dataKeyId));
    }

    private IReadOnlyList<WindowsDeviceUserData> GetLocalUsersList(WindowsDeviceLocalUserSnapShot[] snapshotData, WindowsDeviceUserData[] localUserData)
    {
        // Create record of localUsers from snapshotData & to be filled into DB
        return snapshotData.Select(snapshot =>
        {
            var matchingUser = localUserData?.FirstOrDefault(x => snapshot.SID == x.UserSID);
            var dataKey = matchingUser?.DataKeyId != null
                ? GetCachedDataKey(matchingUser.DataKeyId)
                : _dataKeyService.GetKey();

            return CreateWindowsDeviceLocalUserData(matchingUser, snapshot, dataKey.Id, snapshot.UserName);
        }).AsArray();
    }
}
