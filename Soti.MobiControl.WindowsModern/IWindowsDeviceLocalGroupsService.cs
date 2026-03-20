using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.WindowsModern.Models;
using GroupMemberships = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>;
using LocalUserIds = System.Collections.Generic.IDictionary<string, int>;

namespace Soti.MobiControl.WindowsModern;

/// <summary>
/// Interface for Windows Device Local Groups Service.
/// </summary>
public interface IWindowsDeviceLocalGroupsService
{
    /// <summary>
    /// Returns the device local groups summary info.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>Local Groups info for the deviceId.</returns>
    public IReadOnlyList<DeviceLocalGroupSummary> GetDeviceLocalGroupsSummaryInfo(int deviceId);

    /// <summary>
    /// Returns the names of local groups across given devices.
    /// </summary>
    /// <param name="deviceIds">The collection of Device IDs.</param>
    /// <returns>A collection of local group names.</returns>
    IReadOnlyList<string> GetDevicesLocalGroups(IEnumerable<int> deviceIds);

    /// <summary>
    /// Returns the names of local groups across all Windows devices in the given device group.
    /// </summary>
    /// <param name="deviceGroupIds">The collection of device group IDs.</param>
    /// <returns>A collection of local group names.</returns>
    IReadOnlyList<string> GetDeviceGroupsLocalGroups(ISet<int> deviceGroupIds);

    /// <summary>
    /// Performs a bulk update of the local group users for a specified group.
    /// </summary>
    /// <param name="windowsUserIds">A collection of Windows user IDs to be updated for a particular local group.</param>
    /// <param name="localGroupId">The localGroupId for which users will be updated.</param>
    public void BulkUpdateLocalGroupUserForGroup(IEnumerable<int> windowsUserIds, int localGroupId);

    /// <summary>
    /// Retrieves user details for a specific local group based on the device ID and group name ID.
    /// </summary>
    /// <param name="deviceId">The device ID for which to retrieve local group user details.</param>
    /// <param name="groupNameId">The group name ID associated with the local group.</param>
    /// <returns>An <see cref="IReadOnlyList{WindowsDeviceLocalGroupUserDetailsModal}"/> containing the details of users in the local group.</returns>
    IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> GetUserDetailsForLocalGroup(int deviceId, int groupNameId);

    /// <summary>
    /// Retrieves user details for a specific local group based on the device IDs and group name ID.
    /// </summary>
    /// <param name="deviceIds">Collection of Device IDs for which to retrieve local group user details.</param>
    /// <param name="groupNameId">The group name ID associated with the local group.</param>
    /// <returns>An <see cref="IEnumerable{WindowsDeviceLocalGroupUserDetailsModal}"/> containing the details of users in the local group.</returns>
    IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> BulkGetUserDetailsForLocalGroup(IEnumerable<int> deviceIds, int groupNameId);

    /// <summary>
    /// Retrieves user IDs and names for a specific local group based on the device ID and group name IDs.
    /// </summary>
    /// <param name="deviceId">The device ID for which to retrieve local group user details.</param>
    /// <param name="groupNameIds">The group name ID associated with the local group.</param>
    /// <returns>An <see cref="IReadOnlyList{WindowsDeviceLocalGroupUserDetailsModal}"/> containing the IDs and names of users in the local group.</returns>
    IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> GetUserNamesForLocalGroups(int deviceId, IEnumerable<int> groupNameIds);

    /// <summary>
    /// Retrieves the local group ID for a given device ID and local group name ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="groupNameId">The local group name ID associated with the device.</param>
    /// <returns>
    /// The local group ID that corresponds to the provided device ID and local group name ID.
    /// </returns>
    public int GetLocalGroupIdByLocalGroupNameId(int deviceId, int groupNameId);

    /// <summary>
    /// Retrieves the local group name ID from local group name.
    /// </summary>
    /// <param name="groupName">The group name.</param>
    /// <returns> The local group name ID associated with the specified group name.</returns>
    public int GetGroupNameId(string groupName);

    /// <summary>
    /// Retrieves all the local group names.
    /// </summary>
    /// <returns> All local group names along with their associated local group name ID .</returns>
    public IReadOnlyList<DeviceLocalGroupNameSummary> GetAllGroupNames();

    /// <summary>
    /// Retrieves the local group name corresponding to a given group name ID.
    /// </summary>
    /// <param name="groupNameId">The group name ID.</param>
    /// <returns>The local group name associated with the specified group name ID.</returns>
    public string GetGroupName(int groupNameId);

    /// <summary>
    /// Inserts the local group name.
    /// </summary>
    /// <param name="localGroupName">The local group name.</param>
    /// <returns>The local group name id</returns>
    public int InsertLocalGroupName(string localGroupName);

    /// <summary>
    /// Inserts the local group name.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="groupNameId">The group name ID.</param>
    /// <returns>The local group id</returns>
    public int UpsertWindowsDeviceLocalGroup(int deviceId, int groupNameId);

    /// <summary>
    /// Inserts the local group name.
    /// </summary>
    /// <param name="windowsDeviceUserId">The windows device user ID.</param>
    /// <param name="windowsDeviceLocalGroupId">The windows device local group ID.</param>
    public void InsertLocalGroupUser(int windowsDeviceUserId, int windowsDeviceLocalGroupId);

    /// <summary>
    /// Retrieves a collection of local group names based on the specified group name IDs.
    /// </summary>
    /// <param name="groupNameIds">The collection of group name IDs.</param>
    /// <returns>
    /// A list of <see cref="DeviceLocalGroupNameSummary"/> containing the group name IDs and their corresponding names.
    /// </returns>
    public IReadOnlyList<DeviceLocalGroupNameSummary> GetGroupNamesByGroupNameIds(IEnumerable<int> groupNameIds);

    /// <summary>
    /// Stores the group name ID and group name in the cache.
    /// </summary>
    /// <param name="groupNameId">The group name ID.</param>
    /// <param name="groupName">The group name.</param>
    /// <param name="notifyMsAndDse">Value indicating whether to broadcast invalidation message.Defaults to <c>true</c>.</param>
    public void SetCachedGroupData(int groupNameId, string groupName, bool notifyMsAndDse = true);

    /// <summary>
    /// Clears the cache.
    /// </summary>
    /// <param name="notifyMsAndDse">Value indicating whether to broadcast invalidation message.</param>
    public void InvalidateCache(bool notifyMsAndDse = true);

    /// <summary>
    /// Processes the Windows LocalGroup keys data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="localGroupKeysData">The Windows LocalGroup keys data.</param>
    /// <returns>The group names data of the device.</returns>
    IReadOnlyList<WindowsDeviceLocalGroup> SynchronizeLocalGroupDataWithSnapshot(int deviceId, string localGroupKeysData);

    /// <summary>
    /// Processes the Windows LocalGroupUser keys data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="groupMemberships">The dictionary of local groups for each user.</param>
    /// <param name="localUserIds">The dictionary of local user IDs.</param>
    /// <param name="groupData">The group names data of the device.</param>
    void BulkModifyLocalGroupUser(int deviceId, GroupMemberships groupMemberships, LocalUserIds localUserIds, IEnumerable<WindowsDeviceLocalGroup> groupData);

    /// <summary>
    /// Deletes the windows device local group data based on deviceId.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    public void DeleteLocalGroupData(int deviceId);

    /// <summary>
    /// Retrieves the group names with list of WindowsDeviceUser IDs.
    /// </summary>
    /// <param name="windowsDeviceUserIds">List of WindowsDeviceUser IDs.</param>
    /// <returns>Lookup with group names associated with windows device user Ids.</returns>
    public ILookup<int, string> GetUserGroupsByWindowsDeviceUserIds(IEnumerable<int> windowsDeviceUserIds);

    /// <summary>
    /// Cleans up orphaned local group names data from the database.
    /// </summary>
    /// <returns>The number of records that were cleaned up.</returns>
    public int CleanUpOrphanedLocalGroupNamesData();

    /// <summary>
    /// Retrieves local group search data for the specified combinations of device IDs and group name IDs.
    /// </summary>
    /// <param name="entries">
    /// A collection of tuples where each tuple contains a <c>GroupNameId</c> (Item1) and a <c>DeviceId</c> (Item2).
    /// </param>
    /// <returns>
    /// A list of <see cref="DeviceWindowsLocalGroupSearchDataSummary"/>.
    /// </returns>
    public IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> GetDeviceLocalGroupsSearchDataByDeviceIdAndGroupNameId(IEnumerable<(int, int)> entries);

    /// <summary>
    /// Retrieves local group watermarks for the specified device IDs.
    /// </summary>
    /// <param name="deviceIds">A collection of device IDs.</param>
    /// <returns>
    /// A collection of <see cref="DeviceWindowsLocalGroupSearchDataSummary"/> representing the watermarks.
    /// Returns an empty collection if input is null, empty, or if no data is found.
    /// </returns>
    public IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> GetDeviceLocalGroupWatermarksByDeviceIds(IEnumerable<int> deviceIds);

    /// <summary>
    /// Retrieves the local group name IDs from local group names.
    /// </summary>
    /// <param name="groupNames">The group names.</param>
    /// <returns> The collection of local group name IDs associated with the specified group names.</returns>
    LocalUserIds GetGroupNameIdsByNames(IEnumerable<string> groupNames);
}
