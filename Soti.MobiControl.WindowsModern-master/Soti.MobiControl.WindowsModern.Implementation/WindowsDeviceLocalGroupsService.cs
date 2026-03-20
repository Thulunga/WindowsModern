using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Soti.Diagnostics;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.Search.Database;
using Soti.MobiControl.Search.Database.Enums;
using Soti.MobiControl.Search.Database.Identities;
using Soti.MobiControl.Settings;
using Soti.MobiControl.WindowsModern.Implementation.Converters;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using Soti.Transactions;
using Soti.Utilities.Collections;
using GroupMemberships = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>;
using LocalUserIds = System.Collections.Generic.IDictionary<string, int>;

namespace Soti.MobiControl.WindowsModern.Implementation;

/// <summary>
/// Defines service for Windows device local groups.
/// </summary>
internal sealed class WindowsDeviceLocalGroupsService : IWindowsDeviceLocalGroupsService
{
    private const double NoCacheDuration = -1;
    private const string FeatureName = "WindowsDeviceLocalGroups";
    private const string SettingName = "CacheDuration";
    private const string LocalGroupNameIdCacheKey = "LocalGroupNameId";
    private const string LocalGroupNameCacheKey = "LocalGroupName";
    private double _cacheDurationFromGlobalSettings = -1;

    private static readonly char[] Delimiter = ['@'];
    private const string LogName = "LocalGroup";

    private readonly IProgramTrace _traceLogger;
    private readonly Lazy<IMessagePublisher> _messagePublisher;
    private readonly IWindowsDeviceLocalGroupProvider _windowsDeviceLocalGroupProvider;
    private readonly IDeviceSearchInfoService _deviceSearchInfoService;
    private readonly ISettingsService _settingsService;

    private static readonly MemoryCache MemoryCache = new MemoryCache(new MemoryCacheOptions
    {
        ExpirationScanFrequency = TimeSpan.MaxValue
    });

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsDeviceLocalGroupsService"/> class.
    /// </summary>
    /// <param name="traceLogger">IProgramTrace.</param>
    /// <param name="windowsDeviceLocalGroupProvider">IWindowsDeviceProvider</param>
    /// <param name="deviceSearchInfoService">IDeviceSearchInfoService</param>
    /// <param name="messagePublisher">IMessagePublisher</param>
    /// <param name="settingsService">ISettingsService</param>
    public WindowsDeviceLocalGroupsService(IProgramTrace traceLogger,
        IWindowsDeviceLocalGroupProvider windowsDeviceLocalGroupProvider,
        IDeviceSearchInfoService deviceSearchInfoService,
        Lazy<IMessagePublisher> messagePublisher,
        ISettingsService settingsService
    )
    {
        _traceLogger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        _windowsDeviceLocalGroupProvider = windowsDeviceLocalGroupProvider ??
                                           throw new ArgumentNullException(nameof(windowsDeviceLocalGroupProvider));
        _deviceSearchInfoService = deviceSearchInfoService ?? throw new ArgumentNullException(nameof(deviceSearchInfoService));
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    /// <inheritdoc/>
    public IReadOnlyList<DeviceLocalGroupSummary> GetDeviceLocalGroupsSummaryInfo(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var deviceLocalGroupsData =
            _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(deviceId)?.ToArray();

        return deviceLocalGroupsData == null
            ? new List<DeviceLocalGroupSummary>()
            :
            [
                .. deviceLocalGroupsData.Select
                    (groupData => new DeviceLocalGroupSummary
                    {
                        GroupName = GetGroupName(groupData.GroupNameId),
                        IsAdminGroup = groupData.IsAdminGroup
                    })
                    .OrderBy(data => data.IsAdminGroup),
            ];
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetDevicesLocalGroups(IEnumerable<int> deviceIds)
    {
        var deviceIdList = deviceIds.AsArray();

        return deviceIdList.Count == 0
            ? throw new ArgumentNullException(nameof(deviceIds), "Device IDs collection cannot be empty.")
            : (_windowsDeviceLocalGroupProvider
                .GetDistinctLocalGroupNameIdsByDeviceIds(deviceIdList)?
                .Select(GetGroupName)
                .AsArray());
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetDeviceGroupsLocalGroups(ISet<int> deviceGroupIds)
    {
        return deviceGroupIds == null
            ? throw new ArgumentNullException(nameof(deviceGroupIds))
            : _windowsDeviceLocalGroupProvider.GetDistinctLocalGroupNameIdsByDeviceGroupIds(deviceGroupIds)?.Select(GetGroupName).AsArray();
    }

    /// <inheritdoc/>
    public void BulkUpdateLocalGroupUserForGroup(IEnumerable<int> windowsUserIds, int localGroupId)
    {
        if (localGroupId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(localGroupId));
        }

        if (windowsUserIds == null)
        {
            throw new ArgumentNullException(nameof(windowsUserIds));
        }

        _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModifyForGroup(windowsUserIds, localGroupId);
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> GetUserDetailsForLocalGroup(int deviceId, int groupNameId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        var deviceUserDetailsOfGroup = _windowsDeviceLocalGroupProvider.GetLocalGroupUserDetailsByDeviceIdAndGroupId(deviceId, groupNameId);

        var deviceUserDetails = new List<WindowsDeviceLocalGroupUserDetailsModal>();

        if (deviceUserDetailsOfGroup != null)
        {
            deviceUserDetails.AddRange(
                from userDetails in deviceUserDetailsOfGroup
                select userDetails.ToWindowsDeviceLocalGroupUserDetailsModal(groupNameId));
        }

        return deviceUserDetails;
    }

    /// <inheritdoc/>
    public IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> BulkGetUserDetailsForLocalGroup(IEnumerable<int> deviceIds, int groupNameId)
    {
        var deviceIdList = deviceIds.AsArray();

        if (deviceIdList.Count == 0)
        {
            throw new ArgumentNullException(nameof(deviceIds), "Device IDs collection cannot be empty.");
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        return _windowsDeviceLocalGroupProvider
            .GetLocalGroupUserDetailsByDeviceIdsAndGroupId(deviceIdList, groupNameId)?
            .Select(x => x.ToWindowsDeviceLocalGroupUserDetailsModal(groupNameId))
            .AsArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> GetUserNamesForLocalGroups(int deviceId, IEnumerable<int> groupNameIds)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupNameIds == null)
        {
            throw new ArgumentNullException(nameof(groupNameIds));
        }

        return groupNameIds.SwitchByCount(
            id => GetUserDetailsForLocalGroup(deviceId, id),
            ids => _windowsDeviceLocalGroupProvider.GetUserNamesByDeviceIdAndGroupNameIds(deviceId, ids),
            () => new List<WindowsDeviceLocalGroupUserDetailsModal>()
        );
    }

    /// <inheritdoc/>
    public int GetLocalGroupIdByLocalGroupNameId(int deviceId, int groupNameId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        var localGroupData = _windowsDeviceLocalGroupProvider.GetLocalGroupIdByGroupNameId(deviceId, groupNameId);

        return localGroupData switch
        {
            null => throw new InvalidOperationException($"No local group found for deviceId: {deviceId} and groupNameId: {groupNameId}"),
            _ => localGroupData.GroupId
        };
    }

    /// <inheritdoc/>
    public int GetGroupNameId(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new ArgumentNullException(nameof(groupName));
        }

        if (MemoryCache.TryGetValue(GetLocalGroupNameKey(groupName), out int groupNameId))
        {
            return groupNameId;
        }

        var groupNameData = _windowsDeviceLocalGroupProvider.GetLocalGroupNameDataByLocalGroupName(groupName);
        groupNameId = groupNameData?.GroupNameId ?? 0;
        if (groupNameId != 0)
        {
            SetCachedGroupData(groupNameId, groupName);
        }

        return groupNameId;
    }

    /// <inheritdoc />
    public LocalUserIds GetGroupNameIdsByNames(IEnumerable<string> groupNames)
    {
        if (groupNames == null)
        {
            throw new ArgumentNullException(nameof(groupNames));
        }

        var groupData = new Dictionary<string, int>();
        var groupNamesToFetch = new List<string>();

        foreach (var groupName in groupNames)
        {
            if (MemoryCache.TryGetValue(GetLocalGroupNameKey(groupName), out int groupNameId))
            {
                groupData[groupName] = groupNameId;
            }
            else
            {
                groupNamesToFetch.Add(groupName);
            }
        }

        groupNamesToFetch.SwitchByCount(
            name =>
            {
                var groupNameData = _windowsDeviceLocalGroupProvider.GetLocalGroupNameDataByLocalGroupName(name);
                if (groupNameData == null)
                {
                    return;
                }

                groupData[name] = groupNameData.GroupNameId;
                SetCachedGroupData(groupNameData.GroupNameId, name);
            },
            names =>
            {
                var groupDataFromDb = _windowsDeviceLocalGroupProvider.GetLocalGroupNameDataByLocalGroupNames(names);
                if (groupDataFromDb == null)
                {
                    return;
                }

                foreach (var group in groupDataFromDb)
                {
                    groupData[group.Key] = group.Value.GroupNameId;
                    SetCachedGroupData(group.Value.GroupNameId, group.Key);
                }
            });

        return groupData;
    }

    /// <inheritdoc/>
    public int InsertLocalGroupName(string localGroupName)
    {
        if (string.IsNullOrWhiteSpace(localGroupName))
        {
            throw new ArgumentNullException(nameof(localGroupName));
        }

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(localGroupName, out var windowsDeviceLocalGroupNameId);
        var identifiers = new List<Identifier>() { new IntegerIdentifier(windowsDeviceLocalGroupNameId) };
        AddOrUpdateRecordsForSotiSearch(SearchTable.WindowsLocalGroup, identifiers);
        return windowsDeviceLocalGroupNameId;
    }

    /// <inheritdoc/>
    public void InsertLocalGroupUser(int windowsDeviceUserId, int windowsDeviceLocalGroupId)
    {
        if (windowsDeviceUserId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowsDeviceUserId));
        }

        if (windowsDeviceLocalGroupId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowsDeviceLocalGroupId));
        }

        _windowsDeviceLocalGroupProvider.InsertLocalGroupUser(windowsDeviceUserId, windowsDeviceLocalGroupId);
    }

    /// <inheritdoc/>
    public int UpsertWindowsDeviceLocalGroup(int deviceId, int groupNameId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        _windowsDeviceLocalGroupProvider.UpsertWindowsDeviceLocalGroup(deviceId, groupNameId, out var windowsDeviceLocalGroupId);
        var identifiers = new List<Identifier>() { new LinkedIdentifier(new IntegerIdentifier(windowsDeviceLocalGroupId), new IntegerIdentifier(deviceId)) };
        AddOrUpdateRecordsForSotiSearch(SearchTable.DeviceWindowsLocalGroup, identifiers);
        return windowsDeviceLocalGroupId;
    }

    /// <inheritdoc/>
    public IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> GetDeviceLocalGroupsSearchDataByDeviceIdAndGroupNameId(IEnumerable<(int, int)> entries)
    {
        var entriesList = entries.AsArray();
        if (entriesList.Count == 0)
        {
            throw new ArgumentNullException(nameof(entries), "Entries collection cannot be empty.");
        }

        var localGroupData = entriesList
            .Select(e => new WindowsDeviceLocalGroupData
            {
                GroupNameId = e.Item1,
                DeviceId = e.Item2
            })
            .AsArray();

        return _windowsDeviceLocalGroupProvider.GetDeviceLocalGroupsSearchData(localGroupData);
    }

    /// <inheritdoc />
    public string GetGroupName(int groupNameId)
    {
        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        if (MemoryCache.TryGetValue(GetLocalGroupNameIdKey(groupNameId), out string groupName))
        {
            return groupName;
        }

        var groupNamesData = _windowsDeviceLocalGroupProvider.GetAllLocalGroupNames().AsArray();
        var matchedGroup = groupNamesData
            .FirstOrDefault(x => x.GroupNameId == groupNameId);

        foreach (var group in groupNamesData)
        {
            SetCachedGroupData(group.GroupNameId, group.GroupName);
        }

        return matchedGroup?.GroupName;
    }

    /// <inheritdoc/>
    public IReadOnlyList<DeviceLocalGroupNameSummary> GetAllGroupNames()
    {
        var deviceLocalGroupNameData = _windowsDeviceLocalGroupProvider.GetAllLocalGroupNames();
        return (deviceLocalGroupNameData == null
            ? new List<DeviceLocalGroupNameSummary>()
            : deviceLocalGroupNameData.Select
            (groupData => new DeviceLocalGroupNameSummary
            {
                GroupNameId = groupData.GroupNameId,
                GroupName = groupData.GroupName,
            })).AsArray();
    }

    /// <inheritdoc/>
    public void SetCachedGroupData(int groupNameId, string groupName, bool notifyMsAndDse = true)
    {
        CheckLocalGroupData(groupNameId, groupName);
        if (_cacheDurationFromGlobalSettings < 0)
        {
            MemoryCache.Set(GetLocalGroupNameKey(groupName), groupNameId);
            MemoryCache.Set(GetLocalGroupNameIdKey(groupNameId), groupName);
        }
        else
        {
            var cacheDurationFromGlobalSettings = DateTimeOffset.UtcNow.AddSeconds(_cacheDurationFromGlobalSettings);
            MemoryCache.Set(GetLocalGroupNameKey(groupName), groupNameId, cacheDurationFromGlobalSettings);
            MemoryCache.Set(GetLocalGroupNameIdKey(groupNameId), groupName, cacheDurationFromGlobalSettings);
        }

        if (notifyMsAndDse)
        {
            PublishCacheUpdateMessage(groupNameId, groupName);
        }
    }

    /// <inheritdoc />
    public void InvalidateCache(bool notifyMsAndDse = true)
    {
        MemoryCache.Clear();
        if (notifyMsAndDse)
        {
            _messagePublisher.Value.Publish(
                new LocalGroupCacheClearMessage(),
                ApplicableServer.Ms,
                ApplicableServer.Dse);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<WindowsDeviceLocalGroup> SynchronizeLocalGroupDataWithSnapshot(int deviceId, string localGroupKeysData)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (string.IsNullOrWhiteSpace(localGroupKeysData))
        {
            DeleteRecordsForSotiSearchByDeviceId(deviceId);
            DeleteLocalGroupData(deviceId);
            return [];
        }

        var snapshotData = ParseLocalGroupInfoKeys(deviceId, localGroupKeysData);

        if (snapshotData.Count == 0)
        {
            return [];
        }

        var localGroupData = _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(deviceId)?.ToArray();

        // If the local groups count is same and other parameters is matching, then we don't need to update the records.
        if (AreLocalGroupKeysEqual(localGroupData, snapshotData))
        {
            return [];
        }

        var entries = _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(deviceId, snapshotData).ToList();
        var deleteIdentifiers = new List<Identifier>();
        var updateIdentifiers = new List<Identifier>();
        foreach (var entry in entries)
        {
            if (entry.Action == WindowsDeviceLocalGroupBulkModifyActionType.Deleted)
            {
                deleteIdentifiers.Add(new LinkedIdentifier(new IntegerIdentifier(entry.GroupNameId), new IntegerIdentifier(deviceId)));
            }

            if (entry.Action == WindowsDeviceLocalGroupBulkModifyActionType.Insert)
            {
                updateIdentifiers.Add(new LinkedIdentifier(new IntegerIdentifier(entry.GroupNameId), new IntegerIdentifier(deviceId)));
            }
        }

        AddOrUpdateRecordsForSotiSearch(SearchTable.DeviceWindowsLocalGroup, updateIdentifiers);
        DeleteRecordsForSotiSearch(SearchTable.DeviceWindowsLocalGroup, deleteIdentifiers);
        _traceLogger.Write(TraceLevel.Verbose, LogName, $"Windows LocalGroup key(s) information updated for {deviceId}");

        return entries;
    }

    /// <inheritdoc />
    public void BulkModifyLocalGroupUser(int deviceId, GroupMemberships groupMemberships, LocalUserIds localUserIds, IEnumerable<WindowsDeviceLocalGroup> groupData)
    {
        ValidateInputs(deviceId, groupMemberships, localUserIds);

        var allGroupNames = GetAllGroupNamesByGroupMemberships(groupMemberships);
        var allGroupData = GetGroupNameIdsByNames(allGroupNames);
        var localGroupsDictionary = new Dictionary<int, int>();
        List<int> localGroupsToFetch = null;
        if (groupData != null)
        {
            localGroupsDictionary = groupData.ToDictionary(x => x.GroupNameId, x => x.GroupId);
            var availableLocalGroups = localGroupsDictionary.Values;
            localGroupsToFetch = (from @group in allGroupData where !availableLocalGroups.Contains(@group.Value) select @group.Value).ToList();
        }

        if (localGroupsToFetch != null && localGroupsToFetch.Count != 0)
        {
            var localGroupsData = _windowsDeviceLocalGroupProvider.GetLocalGroupsByDeviceAndNameIds(deviceId, localGroupsToFetch);

            if (localGroupsData != null)
            {
                foreach (var localGroupData in localGroupsData)
                {
                    localGroupsDictionary[localGroupData.GroupNameId] = localGroupData.GroupId;
                }
            }
        }

        TransactionHelper.RunInTransaction(() =>
        {
            var localGroupUserInputData = PrepareGroupUserInputData(groupMemberships, localUserIds, allGroupData, localGroupsDictionary, deviceId);
            _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModify(deviceId, localGroupUserInputData);
        });
    }

    /// <inheritdoc />
    public void DeleteLocalGroupData(int deviceId)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        _windowsDeviceLocalGroupProvider.DeleteByDeviceId(deviceId);
        _traceLogger.Write(TraceLevel.Verbose, LogName, $"Windows LocalGroup key(s) information deleted for {deviceId}");
    }

    /// <inheritdoc />
    public ILookup<int, string> GetUserGroupsByWindowsDeviceUserIds(IEnumerable<int> windowsDeviceUserIds)
    {
        var windowsDeviceUserIdsList = windowsDeviceUserIds.AsArray();

        if (windowsDeviceUserIdsList.Count == 0)
        {
            throw new ArgumentNullException(nameof(windowsDeviceUserIds));
        }

        var userGroupNamesWithIds = _windowsDeviceLocalGroupProvider.GetGroupNamesByWindowsDeviceUserIds(windowsDeviceUserIdsList);
        var userGroupsLookup = userGroupNamesWithIds.ToLookup(x => x.WindowsDeviceUserId, x => x.LocalGroupName);
        return userGroupsLookup;
    }

    /// <inheritdoc />
    public int CleanUpOrphanedLocalGroupNamesData()
    {
        var cleanedUpLocalGroupNames = _windowsDeviceLocalGroupProvider.CleanUpLocalGroupNamesData();

        var deleteIdentifiers = cleanedUpLocalGroupNames.Item2
            .Select(groupName => new IntegerIdentifier(groupName))
            .AsArray();
        _deviceSearchInfoService.DeleteRecords(SearchTable.WindowsLocalUser, deleteIdentifiers);

        return cleanedUpLocalGroupNames.Item1;
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceLocalGroupNameSummary> GetGroupNamesByGroupNameIds(IEnumerable<int> groupNameIds)
    {
        var idsList = groupNameIds.AsArray();

        return idsList.Count == 0
            ? throw new ArgumentNullException(nameof(groupNameIds), "GroupName IDs collection cannot be empty.")
            : _windowsDeviceLocalGroupProvider
                .GetLocalGroupNameByIds(idsList)
                .Select(x => new DeviceLocalGroupNameSummary
                {
                    GroupNameId = x.GroupNameId,
                    GroupName = x.GroupName
                }).AsArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> GetDeviceLocalGroupWatermarksByDeviceIds(IEnumerable<int> deviceIds)
    {
        var deviceIdList = deviceIds.AsArray();

        if (deviceIdList.Count == 0)
        {
            return Array.Empty<DeviceWindowsLocalGroupSearchDataSummary>();
        }

        var result = _windowsDeviceLocalGroupProvider.GetDeviceLocalGroupWatermarks(deviceIdList);

        return result ?? Array.Empty<DeviceWindowsLocalGroupSearchDataSummary>();
    }

    private static string GetLocalGroupNameKey(string groupName)
    {
        return $"{LocalGroupNameCacheKey}_{groupName}";
    }

    private static string GetLocalGroupNameIdKey(int groupNameId)
    {
        return $"{LocalGroupNameIdCacheKey}_{groupNameId}";
    }

    private static void CheckLocalGroupData(int groupNameId, string groupName)
    {
        if (groupNameId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(groupNameId));
        }

        if (string.IsNullOrWhiteSpace(groupName))
        {
            throw new ArgumentNullException(nameof(groupName));
        }
    }

    private void PublishCacheUpdateMessage(int groupNameId, string groupName)
    {
        _messagePublisher.Value.Publish(
            new LocalGroupCacheMessage
            {
                GroupNameId = groupNameId,
                GroupName = groupName
            },
            ApplicableServer.Ms,
            ApplicableServer.Dse);
    }

    private static bool AreLocalGroupKeysEqual(WindowsDeviceLocalGroupData[] localGroupData, ICollection<WindowsDeviceLocalGroupData> snapshotData)
    {
        if (localGroupData == null || snapshotData == null || localGroupData.Length != snapshotData.Count)
        {
            return false;
        }

        foreach (var snapshot in snapshotData)
        {
            var matchingGroup = localGroupData
                .FirstOrDefault(localGroup => localGroup.GroupNameId == snapshot.GroupNameId);
            if (matchingGroup == null || !snapshot.IsAdminGroup.Equals(matchingGroup.IsAdminGroup))
            {
                return false;
            }
        }

        return true;
    }

    private List<WindowsDeviceLocalGroupData> ParseLocalGroupInfoKeys(int deviceId, string localGroupData)
    {
        var localGroups = localGroupData.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries);
        if (localGroups.Length == 0)
        {
            return [];
        }

        GetCacheDurationInSecondsFromGlobalSettings();
        var localGroupsCount = localGroups.Length;

        // localGroupsCount decremented by 1 as the last group (admin) is handled outside the for loop.
        var localGroupInfoKeys = new List<WindowsDeviceLocalGroupData>(capacity: localGroupsCount--);

        for (var i = 0; i < localGroupsCount; i++)
        {
            localGroupInfoKeys.Add(GetLocalGroupData(deviceId, localGroups[i], false));
        }

        localGroupInfoKeys.Add(GetLocalGroupData(deviceId, localGroups[localGroupsCount], true));

        return localGroupInfoKeys;
    }

    private static void ValidateInputs(int deviceId, GroupMemberships groupMemberships, LocalUserIds localUserIds)
    {
        if (deviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        if (groupMemberships == null)
        {
            throw new ArgumentNullException(nameof(groupMemberships));
        }

        if (localUserIds == null)
        {
            throw new ArgumentNullException(nameof(localUserIds));
        }
    }

    private static List<string> GetAllGroupNamesByGroupMemberships(GroupMemberships groupMemberships)
    {
        return groupMemberships.Values
            .Where(g => g != null)
            .SelectMany(g => g)
            .Where(groupName => !string.IsNullOrWhiteSpace(groupName))
            .Distinct()
            .ToList();
    }

    private List<WindowsDeviceLocalGroupUserData> PrepareGroupUserInputData(
        GroupMemberships groupMemberships,
        LocalUserIds localUserIds,
        LocalUserIds allGroupData,
        Dictionary<int, int> localGroupsDictionary,
        int deviceId)
    {
        var localGroupUserInputData = new List<WindowsDeviceLocalGroupUserData>();

        foreach (var userGroups in groupMemberships)
        {
            var userGroupsList = userGroups.Value;
            if (userGroupsList == null || userGroupsList.Count == 0)
            {
                continue;
            }

            var userId = localUserIds[userGroups.Key];
            localGroupUserInputData.AddRange
            (userGroupsList.Select
                    (groupName => GetOrCreateGroupNameId(groupName, allGroupData))
                .Select(groupNameId => GetOrCreateLocalGroup(deviceId, groupNameId, localGroupsDictionary))
                .Select(groupId => new WindowsDeviceLocalGroupUserData { UserId = userId, GroupId = groupId }));
        }

        return localGroupUserInputData;
    }

    private int GetOrCreateGroupNameId(string groupName, LocalUserIds allGroupData)
    {
        if (allGroupData.TryGetValue(groupName, out var groupNameId))
        {
            return groupNameId;
        }

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(groupName, out groupNameId);
        var identifiers = new List<Identifier> { new IntegerIdentifier(groupNameId) };
        AddOrUpdateRecordsForSotiSearch(SearchTable.WindowsLocalGroup, identifiers);
        allGroupData[groupName] = groupNameId;

        return groupNameId;
    }

    private int GetOrCreateLocalGroup(int deviceId, int groupNameId, Dictionary<int, int> localGroupsLookup)
    {
        if (localGroupsLookup.TryGetValue(groupNameId, out var groupId))
        {
            return groupId;
        }

        _windowsDeviceLocalGroupProvider.UpsertWindowsDeviceLocalGroup(deviceId, groupNameId, out groupId);
        var identifiers = new List<Identifier> { new LinkedIdentifier(new IntegerIdentifier(groupNameId), new IntegerIdentifier(deviceId)) };
        AddOrUpdateRecordsForSotiSearch(SearchTable.DeviceWindowsLocalGroup, identifiers);
        localGroupsLookup[groupNameId] = groupId;

        return groupId;
    }

    private WindowsDeviceLocalGroupData GetLocalGroupData(int deviceId, string localGroupName, bool isAdminGroup)
    {
        var localGroupNameId = GetGroupNameId(localGroupName);
        if (localGroupNameId != 0)
        {
            return new WindowsDeviceLocalGroupData
            {
                GroupNameId = localGroupNameId,
                IsAdminGroup = isAdminGroup,
                DeviceId = deviceId
            };
        }

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(localGroupName, out localGroupNameId);
        var identifiers = new List<Identifier>() { new IntegerIdentifier(localGroupNameId) };
        AddOrUpdateRecordsForSotiSearch(SearchTable.WindowsLocalGroup, identifiers);
        SetCachedGroupData(localGroupNameId, localGroupName);

        return new WindowsDeviceLocalGroupData
        {
            GroupNameId = localGroupNameId,
            IsAdminGroup = isAdminGroup,
            DeviceId = deviceId
        };
    }

    private void GetCacheDurationInSecondsFromGlobalSettings()
    {
        _cacheDurationFromGlobalSettings = _settingsService.GetSetting(
            FeatureName,
            SettingName,
            CachingOptions.AllowCaching,
            defaultValue: NoCacheDuration);
    }

    private void AddOrUpdateRecordsForSotiSearch(SearchTable table, IEnumerable<Identifier> identifiers)
    {
        if (identifiers == null)
        {
            throw new ArgumentNullException(nameof(identifiers));
        }

        _deviceSearchInfoService.AddOrUpdateRecords(table, identifiers);
    }

    private void DeleteRecordsForSotiSearch(SearchTable table, IEnumerable<Identifier> identifiers)
    {
        if (identifiers == null)
        {
            throw new ArgumentNullException(nameof(identifiers));
        }

        _deviceSearchInfoService.DeleteRecords(table, identifiers);
    }

    private void DeleteRecordsForSotiSearchByDeviceId(int deviceId)
    {
        if (deviceId < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceId));
        }

        var identifiers = _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(deviceId)
            .Select(x =>
                new LinkedIdentifier(new IntegerIdentifier(x.GroupNameId), new IntegerIdentifier(x.DeviceId))
            );
        _deviceSearchInfoService.DeleteRecords(SearchTable.DeviceWindowsLocalGroup, identifiers);
    }
}
