using System.Collections.Generic;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers;

/// <summary>
/// Data provider interface for WindowsDeviceGroupData entity.
/// </summary>
internal interface IWindowsDeviceLocalGroupProvider
{
    /// <summary>
    /// Retrieves the group ID associated with a specific device ID and group name ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="groupNameId">The group name ID.</param>
    /// <returns>The group ID.</returns>
    public int GetGroupIdByNameId(int deviceId, int groupNameId);

    /// <summary>
    /// Retrieves a list of local groups data associated with a specific device ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <returns>A list of <see cref="WindowsDeviceLocalGroupData"/>.</returns>
    public IReadOnlyList<WindowsDeviceLocalGroupData> GetAllLocalGroupsDataByDeviceId(int deviceId);

    /// <summary>
    /// Retrieves a list of local group name IDs associated with specified devices.
    /// </summary>
    /// <param name="deviceIds">The collection of Device IDs.</param>
    /// <returns>A collection of local group name IDs.</returns>
    IReadOnlyList<int> GetDistinctLocalGroupNameIdsByDeviceIds(IEnumerable<int> deviceIds);

    /// <summary>
    /// Retrieves a list of local group name IDs associated with specified devices.
    /// </summary>
    /// <param name="deviceGroupIds">The collection of device group IDs.</param>
    /// <returns>A collection of local group name IDs.</returns>
    IReadOnlyList<int> GetDistinctLocalGroupNameIdsByDeviceGroupIds(ISet<int> deviceGroupIds);

    /// <summary>
    /// Inserts a new local group name into the database and returns the inserted data.
    /// </summary>
    /// <param name="localGroupName">The name of the local group to be inserted.</param>
    /// <param name="groupNameId">The group name ID output variable corresponding to inserted local group name.</param>
    public void InsertLocalGroupName(string localGroupName, out int groupNameId);

    /// <summary>
    /// Retrieves all local group name data from the database.
    /// </summary>
    /// <returns>A list of <see cref="WindowsDeviceLocalGroupNameData"/>.</returns>
    public IReadOnlyList<WindowsDeviceLocalGroupNameData> GetAllLocalGroupNames();

    /// <summary>
    /// Retrieves a local group name data entity based on the specified group name.
    /// </summary>
    /// <param name="groupName">The group name to filter by.</param>
    /// <returns>A <see cref="WindowsDeviceLocalGroupNameData"/> entity.</returns>
    public WindowsDeviceLocalGroupNameData GetLocalGroupNameDataByLocalGroupName(string groupName);

    /// <summary>
    /// Modifies the local group data for a specific device using bulk insert, update, or delete operations.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="entries">A collection of <see cref="WindowsDeviceLocalGroupData"/>.</param>
    /// <returns>A list of <see cref="WindowsDeviceLocalGroup"/> entity containing the details of local groups.</returns>
    public IReadOnlyList<WindowsDeviceLocalGroup> LocalGroupTableBulkModify(int deviceId, IEnumerable<WindowsDeviceLocalGroupData> entries);

    /// <summary>
    /// Modifies the local user group membership data for a specific device using bulk insert, update, or delete operations.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="entries">A collection of <see cref="WindowsDeviceLocalGroupUserData"/>.</param>
    public void LocalGroupUserTableBulkModify(int deviceId, IEnumerable<WindowsDeviceLocalGroupUserData> entries);

    /// <summary>
    /// Deletes the windows local group data.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    public void DeleteByDeviceId(int deviceId);

    /// <summary>
    /// Modifies the local group user data for a specific local group using bulk insert, update, or delete operations.
    /// </summary>
    /// <param name="entries">A collection of user IDs to be modified.</param>
    /// <param name="localGroupId">The local group ID to modify.</param>
    public void LocalGroupUserTableBulkModifyForGroup(IEnumerable<int> entries, int localGroupId);

    /// <summary>
    /// Retrieves the details of local group users based on the specified device ID and group name ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="groupNameId">The group name ID.</param>
    /// <returns>A list of <see cref="WindowsDeviceLocalGroupUserDetailsData"/> containing the details of the local group users.</returns>
    IReadOnlyList<WindowsDeviceLocalGroupUserDetailsData> GetLocalGroupUserDetailsByDeviceIdAndGroupId(int deviceId, int groupNameId);

    /// <summary>
    /// Retrieves the details of local group users based on the specified device IDs and group name ID.
    /// </summary>
    /// <param name="deviceIds">The collection of device IDs.</param>
    /// <param name="groupNameId">The group name ID.</param>
    /// <returns>A collection of <see cref="WindowsDeviceLocalGroupUserDetailsData"/> containing the details of the local group users.</returns>
    IReadOnlyList<WindowsDeviceLocalGroupUserDetailsData> GetLocalGroupUserDetailsByDeviceIdsAndGroupId(IEnumerable<int> deviceIds, int groupNameId);

    /// <summary>
    /// Retrieves the names and IDs of local group users based on the specified device ID and group name IDs.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="groupNameIds">The group name IDs.</param>
    /// <returns>A list of <see cref="WindowsDeviceLocalGroupUserDetailsData"/> containing the names and IDs of the local group users.</returns>
    IReadOnlyList<WindowsDeviceLocalGroupUserDetailsModal> GetUserNamesByDeviceIdAndGroupNameIds(int deviceId, IEnumerable<int> groupNameIds);

    /// <summary>
    /// Retrieves the local group data for a specified device ID and group name ID.
    /// </summary>
    /// <param name="deviceId">The device ID.</param>
    /// <param name="groupNameId">The group name ID.</param>
    /// <returns>
    /// A <see cref="WindowsDeviceLocalGroupData"/> entity.
    /// If no matching local group data is found, returns <c>null</c>.
    /// </returns>
    public WindowsDeviceLocalGroupData GetLocalGroupIdByGroupNameId(int deviceId, int groupNameId);

    /// <summary>
    /// Cleans up outdated or invalid local group names data from the database.
    /// </summary>
    /// <returns>The number of records that were cleaned up and their ids.</returns>
    public (int, IReadOnlyList<int>) CleanUpLocalGroupNamesData();

    /// <summary>
    /// Retrieves a collection of <see cref="WindowsDeviceLocalGroupNameData"/> objects corresponding to the specified group name IDs.
    /// </summary>
    /// <param name="groupNameIds">A collection group name IDs.</param>
    /// <returns>
    /// A list of <see cref="WindowsDeviceLocalGroupNameData"/>.
    /// </returns>
    public IReadOnlyList<WindowsDeviceLocalGroupNameData> GetLocalGroupNameByIds(IEnumerable<int> groupNameIds);

    /// <summary>
    /// Retrieves a list of group names data for the specified WindowsDeviceUserIds.
    /// </summary>
    /// <returns>A list of <see cref="WindowsDeviceLocalGroupUserGroupNameData"/>.</returns>
    IReadOnlyList<WindowsDeviceLocalGroupUserGroupNameData> GetGroupNamesByWindowsDeviceUserIds(IEnumerable<int> windowsDeviceUserIds);

    /// <summary>
    /// Inserts or updates record for local group.
    /// </summary>
    /// <param name="deviceId">The device Id.</param>
    /// <param name="groupNameId">The group name Id.</param>
    /// <param name="windowsDeviceLocalGroupId">The windows device local group Id output variable corresponding to inserted local group.</param>
    void UpsertWindowsDeviceLocalGroup(int deviceId, int groupNameId, out int windowsDeviceLocalGroupId);

    /// <summary>
    /// Inserts a new local group user.
    /// </summary>
    /// <param name="windowsDeviceUserId">The windows device user Id.</param>
    /// <param name="windowsDeviceLocalGroupId">The windows device local group Id.</param>
    void InsertLocalGroupUser(int windowsDeviceUserId, int windowsDeviceLocalGroupId);

    /// <summary>
    /// Retrieves local group search data for the specified collection of device and group name entries.
    /// </summary>
    /// <param name="entries">
    /// A collection of <see cref="WindowsDeviceLocalGroupData"/>.
    /// </param>
    /// <returns>
    /// A read-only list of <see cref="DeviceWindowsLocalGroupSearchDataSummary"/>.
    /// </returns>
    public IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> GetDeviceLocalGroupsSearchData(IEnumerable<WindowsDeviceLocalGroupData> entries);

    /// <summary>
    /// Retrieves local group watermark data for the specified device IDs.
    /// </summary>
    /// <param name="deviceIds">A collection of device IDs</param>
    /// <returns>
    /// A read-only list of <see cref="DeviceWindowsLocalGroupSearchDataSummary"/>.
    /// </returns>
    public IReadOnlyList<DeviceWindowsLocalGroupSearchDataSummary> GetDeviceLocalGroupWatermarks(IEnumerable<int> deviceIds);

    /// <summary>
    /// Retrieves a mapping of local user group names to their corresponding local user group name data.
    /// </summary>
    /// <param name="groupNames">A collection of local user group names.</param>
    /// <returns>
    /// A dictionary where each key is a device ID and the corresponding to an <see cref="IEnumerable{WindowsDeviceLocalUserModel}"/>
    /// </returns>
    public Dictionary<string, WindowsDeviceLocalGroupNameData> GetLocalGroupNameDataByLocalGroupNames(IEnumerable<string> groupNames);

    /// <summary>
    /// Retrieves a list of Local group data for the specified device and list of group name id.
    /// </summary>
    /// <returns>A collection of <see cref="WindowsDeviceLocalGroupData"/>.</returns>
    IReadOnlyList<WindowsDeviceLocalGroupData> GetLocalGroupsByDeviceAndNameIds(
        int deviceId,
        IEnumerable<int> windowsDeviceLocalGroupNameIds);
}
