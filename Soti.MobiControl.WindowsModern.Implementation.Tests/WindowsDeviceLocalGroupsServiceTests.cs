using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.Diagnostics;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.Search.Database;
using Soti.MobiControl.Settings;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
public sealed class WindowsDeviceLocalGroupsServiceTests
{
    private const int DeviceId1 = 1001;
    private const int DeviceId2 = 1002;
    private const int GroupNameId = 1;
    private const int GroupNameId2 = 2;
    private const int GroupId = 11;
    private const int InvalidDeviceId = -1000;
    private const int InvalidGroupId = -11;
    private const int InvalidGroupNameId = -1;
    private const string GroupName1 = "GroupName1";
    private const string GroupName2 = "GroupName2";
    private const string UserName = "UserName";
    private const string UserName2 = "UserName2";
    private const string DeviceIdParameter = "deviceId";
    private const string GroupNameIdParameter = "groupNameId";
    private const string GroupNameParameter = "groupName";
    private const string LocalGroupId = "localGroupId";
    private const string WindowsUserIds = "windowsUserIds";
    private static readonly List<int> UserIds = [1, 2, 3];

    private IWindowsDeviceLocalGroupsService _windowsDeviceLocalGroupsService;
    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IMessagePublisher> _messagePublisherMock;
    private Mock<IWindowsDeviceLocalGroupProvider> _windowsDeviceGroupProviderMock;
    private Mock<IDeviceSearchInfoService> _deviceSearchInfoServiceMock;
    private Mock<ISettingsService> _settingsServiceMock;

    [SetUp]
    public void Setup()
    {
        _programTraceMock = new Mock<IProgramTrace>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _windowsDeviceGroupProviderMock = new Mock<IWindowsDeviceLocalGroupProvider>(MockBehavior.Strict);
        _deviceSearchInfoServiceMock = new Mock<IDeviceSearchInfoService>();
        _settingsServiceMock = new Mock<ISettingsService>();
        _windowsDeviceLocalGroupsService = new WindowsDeviceLocalGroupsService(_programTraceMock.Object, _windowsDeviceGroupProviderMock.Object,
            _deviceSearchInfoServiceMock.Object, new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object), _settingsServiceMock.Object);
    }

    [Test]
    public void GetDeviceLocalGroupsSummaryInfo_Success()
    {
        _windowsDeviceLocalGroupsService.InvalidateCache();

        var localGroupData1 = CreateLocalGroupData(1, DeviceId1, 11, true);
        var localGroupData2 = CreateLocalGroupData(2, DeviceId1, 12, false);

        var localGroupNameData1 = CreateLocalGroupNameData(11, GroupName1);
        var localGroupNameData2 = CreateLocalGroupNameData(12, GroupName2);

        _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupsDataByDeviceId(DeviceId1))
            .Returns(new List<WindowsDeviceLocalGroupData> { localGroupData1, localGroupData2 });
        _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupNames())
            .Returns(new List<WindowsDeviceLocalGroupNameData> { localGroupNameData1, localGroupNameData2 });

        Assert.DoesNotThrow(() => _windowsDeviceLocalGroupsService.GetDeviceLocalGroupsSummaryInfo(DeviceId1));

        var res = _windowsDeviceLocalGroupsService.GetDeviceLocalGroupsSummaryInfo(DeviceId1).ToList();
        var result1 = res.Find(x => x.GroupName == GroupName1);
        var result2 = res.Find(x => x.GroupName == GroupName2);

        Assert.NotNull(res);
        Assert.AreEqual(2, res.Count);
        Assert.AreEqual(localGroupData1.IsAdminGroup, result1.IsAdminGroup);
        Assert.AreEqual(localGroupData2.IsAdminGroup, result2.IsAdminGroup);
    }

    [Test]
    public void GetDeviceLocalGroupsSummaryInfo_Success_NoGroups()
    {
        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetAllLocalGroupsDataByDeviceId(DeviceId1))
            .Returns((IReadOnlyList<WindowsDeviceLocalGroupData>)null);
        var result = _windowsDeviceLocalGroupsService.GetDeviceLocalGroupsSummaryInfo(DeviceId1);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void GetDeviceLocalGroupsSummaryInfo_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.GetDeviceLocalGroupsSummaryInfo(InvalidGroupNameId));
    }

    [Test]
    public void GetDevicesLocalGroups_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupsService.GetDevicesLocalGroups(null));
    }

    [Test]
    public void GetDevicesLocalGroups_ValidInput_ReturnsGroupsList()
    {
        _windowsDeviceLocalGroupsService.InvalidateCache();

        var localGroupsList = new List<string> { "Users", "Administrators" };
        var localGroupIdsList = new List<int> { 1, 2 };

        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetDistinctLocalGroupNameIdsByDeviceIds(It.IsAny<IEnumerable<int>>()))
            .Returns(localGroupIdsList).Verifiable();
        _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupNames())
            .Returns(new List<WindowsDeviceLocalGroupNameData>
            {
                CreateLocalGroupNameData(localGroupIdsList[0], localGroupsList[0]),
                CreateLocalGroupNameData(localGroupIdsList[1], localGroupsList[1])
            }).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetDevicesLocalGroups([DeviceId1]);

        _windowsDeviceGroupProviderMock.Verify(x => x.GetDistinctLocalGroupNameIdsByDeviceIds(It.IsAny<IEnumerable<int>>()), Times.Once);
        _windowsDeviceGroupProviderMock.Verify(x => x.GetAllLocalGroupNames(), Times.Once);

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(localGroupsList, result);
    }

    [Test]
    public void GetDeviceGroupsLocalGroups_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupsService.GetDeviceGroupsLocalGroups(null));
    }

    [Test]
    public void GetDeviceGroupsLocalGroups_EmptyDeviceGroupIds_ReturnsEmptyList()
    {
        ISet<int> emptyDeviceGroupIds = new HashSet<int>();
        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetDistinctLocalGroupNameIdsByDeviceGroupIds(It.IsAny<ISet<int>>()))
            .Returns(new List<int>()).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetDeviceGroupsLocalGroups(emptyDeviceGroupIds);

        _windowsDeviceGroupProviderMock
            .Verify(x => x.GetDistinctLocalGroupNameIdsByDeviceGroupIds(emptyDeviceGroupIds), Times.Once);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetDeviceGroupsLocalGroups_ValidInput_ReturnsGroupsList()
    {
        _windowsDeviceLocalGroupsService.InvalidateCache();

        var localGroupsList = new List<string> { "Users", "Administrators" };
        var localGroupIdsList = new List<int> { 1, 2 };

        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetDistinctLocalGroupNameIdsByDeviceGroupIds(It.IsAny<ISet<int>>()))
            .Returns(localGroupIdsList).Verifiable();
        _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupNames())
            .Returns(new List<WindowsDeviceLocalGroupNameData>
            {
                CreateLocalGroupNameData(localGroupIdsList[0], localGroupsList[0]),
                CreateLocalGroupNameData(localGroupIdsList[1], localGroupsList[1])
            }).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetDeviceGroupsLocalGroups(new HashSet<int> { 1 });

        _windowsDeviceGroupProviderMock.Verify(x => x.GetDistinctLocalGroupNameIdsByDeviceGroupIds(It.IsAny<ISet<int>>()), Times.Once);
        _windowsDeviceGroupProviderMock.Verify(x => x.GetAllLocalGroupNames(), Times.Once);

        Assert.IsNotNull(result);
        CollectionAssert.AreEqual(localGroupsList, result);
    }

    [Test]
    public void BulkUpdateLocalGroupUserForGroup_ValidInput_CallsProvider()
    {
        _windowsDeviceGroupProviderMock
            .Setup(x => x.LocalGroupUserTableBulkModifyForGroup(UserIds, GroupId));

        _windowsDeviceLocalGroupsService.BulkUpdateLocalGroupUserForGroup(UserIds, GroupId);

        _windowsDeviceGroupProviderMock.Verify(x => x.LocalGroupUserTableBulkModifyForGroup(UserIds, GroupId), Times.Once);
    }

    [Test]
    public void BulkUpdateLocalGroupUserForGroup_InvalidLocalGroupId_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.BulkUpdateLocalGroupUserForGroup(UserIds, InvalidGroupId));
        Assert.AreEqual(LocalGroupId, ex?.ParamName);
    }

    [Test]
    public void BulkUpdateLocalGroupUserForGroup_NullUserIds_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupsService.BulkUpdateLocalGroupUserForGroup(null, GroupId));
        Assert.AreEqual(WindowsUserIds, ex?.ParamName);
    }

    [Test]
    public void GetUserDetailsForLocalGroup_ValidInput_ReturnsUserDetails()
    {
        var userDetailsList = new List<WindowsDeviceLocalGroupUserDetailsData>
        {
            new()
            {
                UserName = UserName,
                DataKeyId = 1,
            }
        };

        _windowsDeviceGroupProviderMock
            .Setup(p => p.GetLocalGroupUserDetailsByDeviceIdAndGroupId(DeviceId1, GroupNameId))
            .Returns(userDetailsList);

        var result = _windowsDeviceLocalGroupsService.GetUserDetailsForLocalGroup(DeviceId1, GroupNameId).ToList();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(UserName, result[0].UserName);
        _windowsDeviceGroupProviderMock.Verify(p => p.GetLocalGroupUserDetailsByDeviceIdAndGroupId(DeviceId1, GroupNameId), Times.Once);
    }

    [Test]
    public void GetUserDetailsForLocalGroup_InvalidDeviceId_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.GetUserDetailsForLocalGroup(InvalidDeviceId, GroupNameId));
        Assert.AreEqual(DeviceIdParameter, ex?.ParamName);
    }

    [Test]
    public void GetUserDetailsForLocalGroup_InvalidGroupNameId_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.GetUserDetailsForLocalGroup(DeviceId1, InvalidGroupNameId));
        Assert.AreEqual(GroupNameIdParameter, ex?.ParamName);
    }

    [Test]
    public void BulkGetUserDetailsForLocalGroup_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupsService.BulkGetUserDetailsForLocalGroup(null, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceLocalGroupsService.BulkGetUserDetailsForLocalGroup([1, 2], 0));
    }

    [Test]
    public void BulkGetUserDetailsForLocalGroup_ValidInput_ReturnsUserDetails()
    {
        var userDetailsList = new List<WindowsDeviceLocalGroupUserDetailsData>
        {
            new()
            {
                DeviceId = DeviceId1,
                UserName = UserName,
                DataKeyId = 1,
            }
        };

        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetLocalGroupUserDetailsByDeviceIdsAndGroupId(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
            .Returns(userDetailsList).Verifiable();

        var result = _windowsDeviceLocalGroupsService.BulkGetUserDetailsForLocalGroup([DeviceId1], GroupNameId).ToList();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(DeviceId1, result[0].DeviceId);
        Assert.AreEqual(UserName, result[0].UserName);
        _windowsDeviceGroupProviderMock.Verify(x => x.GetLocalGroupUserDetailsByDeviceIdsAndGroupId(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
    }

    [Test]
    public void GetUserNamesForLocalGroups_InvalidDeviceId_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.GetUserNamesForLocalGroups(InvalidDeviceId, [GroupNameId]));
    }

    [Test]
    public void GetUserNamesForLocalGroups_NullGroupNameIds_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupsService.GetUserNamesForLocalGroups(DeviceId1, null));
    }

    [Test]
    public void GetUserNamesForLocalGroups_EmptyGroupNameIds_ReturnsEmptyList()
    {
        Assert.That(_windowsDeviceLocalGroupsService.GetUserNamesForLocalGroups(DeviceId1, []), Is.Empty);
    }

    [Test]
    public void GetUserNamesForLocalGroups_Success_SingleGroup()
    {
        var userDetailsList = new List<WindowsDeviceLocalGroupUserDetailsData>
        {
            new()
            {
                WindowsDeviceUserId = UserIds[0],
                UserName = UserName
            }
        };

        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetLocalGroupUserDetailsByDeviceIdAndGroupId(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(userDetailsList).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetUserNamesForLocalGroups(DeviceId1, [GroupNameId]);

        _windowsDeviceGroupProviderMock.Verify(x => x.GetLocalGroupUserDetailsByDeviceIdAndGroupId(It.IsAny<int>(), It.IsAny<int>()), Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(GroupNameId, result[0].GroupNameId);
        Assert.AreEqual(UserIds[0], result[0].WindowsDeviceUserId);
        Assert.AreEqual(UserName, result[0].UserName);
    }

    [Test]
    public void GetUserNamesForLocalGroups_Success_MultipleGroups()
    {
        var userDetailsList = new List<WindowsDeviceLocalGroupUserDetailsModal>
        {
            new()
            {
                GroupNameId = GroupNameId,
                WindowsDeviceUserId = UserIds[0],
                UserName = UserName
            },
            new()
            {
                GroupNameId = GroupNameId2,
                WindowsDeviceUserId = UserIds[1],
                UserName = UserName2
            }
        };

        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetUserNamesByDeviceIdAndGroupNameIds(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
            .Returns(userDetailsList).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetUserNamesForLocalGroups(DeviceId1, [GroupNameId, GroupNameId2]);

        _windowsDeviceGroupProviderMock.Verify(x => x.GetUserNamesByDeviceIdAndGroupNameIds(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()), Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(GroupNameId, result[0].GroupNameId);
        Assert.AreEqual(UserIds[0], result[0].WindowsDeviceUserId);
        Assert.AreEqual(UserName, result[0].UserName);
        Assert.AreEqual(GroupNameId2, result[1].GroupNameId);
        Assert.AreEqual(UserIds[1], result[1].WindowsDeviceUserId);
        Assert.AreEqual(UserName2, result[1].UserName);
    }

    [Test]
    public void GetLocalGroupIdByLocalGroupNameId_ValidInputs_ReturnsGroupId()
    {
        var localGroupData = CreateLocalGroupData(1, DeviceId1, 11, true);

        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupIdByGroupNameId(DeviceId1, GroupNameId))
            .Returns(localGroupData);

        var result = _windowsDeviceLocalGroupsService.GetLocalGroupIdByLocalGroupNameId(DeviceId1, GroupNameId);
        Assert.AreEqual(1, result);
    }

    [Test]
    public void GetLocalGroupIdByLocalGroupNameId_DeviceIdLessThanOrEqualToZero_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.GetLocalGroupIdByLocalGroupNameId(InvalidDeviceId, GroupNameId));
        Assert.AreEqual(DeviceIdParameter, ex?.ParamName);
    }

    [Test]
    public void GetLocalGroupIdByLocalGroupNameId_GroupNameIdLessThanOrEqualToZero_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.GetLocalGroupIdByLocalGroupNameId(DeviceId1, InvalidGroupNameId));
        Assert.AreEqual(GroupNameIdParameter, ex?.ParamName);
    }

    [Test]
    public void GetGroupNameId_ReturnsGroupId_WhenGroupExists()
    {
        var localGroupNameData = CreateLocalGroupNameData(GroupNameId, GroupName1);

        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupNameDataByLocalGroupName(GroupName1))
            .Returns(localGroupNameData);

        _windowsDeviceLocalGroupsService.SetCachedGroupData(GroupNameId, GroupName1, true);

        var result = _windowsDeviceLocalGroupsService.GetGroupNameId(GroupName1);

        Assert.AreEqual(GroupNameId, result);
    }

    [Test]
    public void GetGroupNameId_ShouldThrowArgumentNullException_WhenGroupNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupsService.GetGroupNameId(null));
    }

    [Test]
    public void GetGroupNameId_ShouldReturnZero_WhenGroupNameIsNotInProvider()
    {
        const string groupName = "NewGroupName";

        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupNameDataByLocalGroupName(groupName))
            .Returns((WindowsDeviceLocalGroupNameData)null);

        var result = _windowsDeviceLocalGroupsService.GetGroupNameId(groupName);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void GetGroupNameId_ShouldReturnGroupNameIdFromProvider_WhenGroupNameIsInProvider()
    {
        const int expectedGroupNameId = 1;

        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupNameDataByLocalGroupName(GroupName1))
            .Returns(new WindowsDeviceLocalGroupNameData { GroupNameId = expectedGroupNameId });

        var result = _windowsDeviceLocalGroupsService.GetGroupNameId(GroupName1);

        Assert.AreEqual(expectedGroupNameId, result);
    }

    [Test]
    public void GetGroupName_ReturnsGroupName_WhenGroupIdExists()
    {
        _windowsDeviceLocalGroupsService.SetCachedGroupData(GroupNameId, GroupName1, true);

        var result = _windowsDeviceLocalGroupsService.GetGroupName(GroupNameId);

        Assert.AreEqual(GroupName1, result);
    }

    [Test]
    public void SetCachedGroupData_WithInvalidGroupNameId_ShouldThrowArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.SetCachedGroupData(InvalidGroupNameId, GroupName1, true));

        Assert.That(ex.ParamName, Is.EqualTo(GroupNameIdParameter));
    }

    [Test]
    public void SetCachedGroupData_WithNullOrWhiteSpaceGroupName_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupsService.SetCachedGroupData(GroupNameId, null, true));

        Assert.That(ex.ParamName, Is.EqualTo(GroupNameParameter));
    }

    [Test]
    public void GetGroupName_ShouldThrowArgumentOutOfRangeException_WhenGroupNameIdIsLessThanOrEqualToZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceLocalGroupsService.GetGroupName(InvalidGroupNameId));
    }

    [Test]
    public void SetCachedGroupData_StoresGroupIdAndGroupName()
    {
        var localGroupNameData = CreateLocalGroupNameData(GroupNameId, GroupName1);

        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupNameDataByLocalGroupName(GroupName1))
            .Returns(localGroupNameData);

        _windowsDeviceLocalGroupsService.SetCachedGroupData(GroupNameId, GroupName1, true);

        Assert.AreEqual(GroupNameId, _windowsDeviceLocalGroupsService.GetGroupNameId(GroupName1));
        Assert.AreEqual(GroupName1, _windowsDeviceLocalGroupsService.GetGroupName(GroupNameId));
    }

    [Test]
    public void DeleteLocalGroupData_Throws_ArgumentOutOfRangeException_Exceptions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceLocalGroupsService.DeleteLocalGroupData(0));
    }

    [Test]
    public void DeleteLocalGroupData_Succeeds()
    {
        _windowsDeviceGroupProviderMock.Setup(m => m.DeleteByDeviceId(1));
        _programTraceMock
            .Setup(x => x.Write(It.IsAny<TraceLevel>(), It.IsAny<string>(), It.IsAny<string>()));

        _windowsDeviceLocalGroupsService.DeleteLocalGroupData(1);

        _windowsDeviceGroupProviderMock
            .Verify(m => m.DeleteByDeviceId(1), Times.Once);
    }

    [Test]
    public void SynchronizeLocalGroupDataWithSnapshot_ShouldThrowArgumentOutOfRangeException_WhenDeviceIdIsLessThanOrEqualToZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceLocalGroupsService.SynchronizeLocalGroupDataWithSnapshot(0, "SomeData"));
    }

    [Test]
    public void SynchronizeLocalGroupDataWithSnapshot_ShouldReturnEarly_WhenLocalGroupKeysDataIsEmpty()
    {
        var localGroupData = new[]
        {
            new WindowsDeviceLocalGroupData { GroupNameId = 1, IsAdminGroup = true, DeviceId = DeviceId1 }
        };
        var localGroupKeysData = string.Empty;

        _windowsDeviceGroupProviderMock.Setup(x => x.DeleteByDeviceId(It.IsAny<int>()));
        _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupsDataByDeviceId(DeviceId1))
            .Returns(localGroupData);

        _windowsDeviceLocalGroupsService.SynchronizeLocalGroupDataWithSnapshot(DeviceId1, localGroupKeysData);

        _windowsDeviceGroupProviderMock.Verify(x => x.GetAllLocalGroupsDataByDeviceId(It.IsAny<int>()), Times.Once);
        _windowsDeviceGroupProviderMock.Verify(x => x.DeleteByDeviceId(DeviceId1), Times.Once);
        _windowsDeviceGroupProviderMock.VerifyNoOtherCalls();
        _programTraceMock.Verify(x => x.Write(
            TraceLevel.Verbose,
            It.IsAny<string>(),
            It.Is<string>(msg => msg.Contains($"Windows LocalGroup key(s) information deleted for {DeviceId1}"))
        ), Times.Once);
        _programTraceMock.VerifyNoOtherCalls();
    }

    [Test]
    public void SynchronizeLocalGroupDataWithSnapshot_ShouldReturnEarly_WhenLocalGroupKeysDataIsInvalid()
    {
        const string localGroupKeysData = "@";

        _windowsDeviceLocalGroupsService.SynchronizeLocalGroupDataWithSnapshot(DeviceId1, localGroupKeysData);

        _windowsDeviceGroupProviderMock.VerifyNoOtherCalls();
        _programTraceMock.VerifyNoOtherCalls();
    }

    [Test]
    public void SynchronizeLocalGroupDataWithSnapshot_ShouldNotModifyLocalGroupTable_WhenLocalGroupKeysAreEqual()
    {
        var localGroupData = new[]
        {
            new WindowsDeviceLocalGroupData { GroupNameId = 1, IsAdminGroup = true, DeviceId = DeviceId1 }
        };

        _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupsDataByDeviceId(DeviceId1))
            .Returns(localGroupData);

        _windowsDeviceLocalGroupsService.SynchronizeLocalGroupDataWithSnapshot(DeviceId1, "GroupName1");

        _windowsDeviceGroupProviderMock.Verify(x => x.LocalGroupTableBulkModify(DeviceId1, It.IsAny<IEnumerable<WindowsDeviceLocalGroupData>>()), Times.Never);
    }

    [Test]
    public void SynchronizeLocalGroupDataWithSnapshot_ShouldLogMessage_WhenLocalGroupDataIsUpdated()
    {
        // This test also verifies if group names are not added to memory cache when cache duration is set as 0.

        const string newGroup1 = "NewGroup1";
        const string newGroup2 = "NewGroup2";
        var groupNameId1 = 1;
        var groupNameId2 = 2;
        WindowsDeviceLocalGroupData[] localGroupData = [];
        WindowsDeviceLocalGroup[] localGroupBulkModifyData = [
            new()
            {
                GroupId = GroupId,
                GroupNameId = groupNameId1,
                Action = WindowsDeviceLocalGroupBulkModifyActionType.Insert
            },
            new()
            {
                GroupId = GroupId + 1,
                GroupNameId = groupNameId2,
                Action = WindowsDeviceLocalGroupBulkModifyActionType.Insert
            }
        ];

        _settingsServiceMock.Setup(x => x.GetSetting(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CachingOptions>(),
            It.IsAny<ObfuscationOption>(), It.IsAny<double>())).Returns(0); // Cache duration is set as 0.
        _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupsDataByDeviceId(DeviceId1))
            .Returns(localGroupData);
        _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupsDataByDeviceId(DeviceId1))
            .Returns(localGroupData);
        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupNameDataByLocalGroupName(It.IsAny<string>())).Returns((WindowsDeviceLocalGroupNameData)null);
        _windowsDeviceGroupProviderMock.Setup(x => x.LocalGroupTableBulkModify(DeviceId1, It.IsAny<IEnumerable<WindowsDeviceLocalGroupData>>()))
            .Returns(localGroupBulkModifyData)
            .Verifiable();
        _windowsDeviceGroupProviderMock
            .Setup(x => x.InsertLocalGroupName(newGroup1, out groupNameId1)).Verifiable();
        _windowsDeviceGroupProviderMock
            .Setup(x => x.InsertLocalGroupName(newGroup2, out groupNameId2)).Verifiable();

        _windowsDeviceLocalGroupsService.SynchronizeLocalGroupDataWithSnapshot(DeviceId1, "NewGroup1@NewGroup2");

        _programTraceMock.Verify(x => x.Write(
            TraceLevel.Verbose,
            It.IsAny<string>(),
            It.Is<string>(msg => msg.Contains($"Windows LocalGroup key(s) information updated for {DeviceId1}"))
        ), Times.Once);

        _settingsServiceMock.Verify(x => x.GetSetting(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CachingOptions>(),
            It.IsAny<ObfuscationOption>(), It.IsAny<double>()), Times.Once);
        _windowsDeviceGroupProviderMock.Verify(x => x.LocalGroupTableBulkModify(DeviceId1, It.IsAny<IEnumerable<WindowsDeviceLocalGroupData>>()), Times.Once);

        // Verify if group name is not present in cache, and fetched from DB.
        _windowsDeviceGroupProviderMock.Reset();
        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupNameDataByLocalGroupName(newGroup1)).Returns(new WindowsDeviceLocalGroupNameData
        {
            GroupNameId = groupNameId1,
            GroupName = newGroup1
        });
        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupNameDataByLocalGroupName(newGroup2)).Returns(new WindowsDeviceLocalGroupNameData
        {
            GroupNameId = groupNameId2,
            GroupName = newGroup2
        });

        Assert.AreEqual(groupNameId1, _windowsDeviceLocalGroupsService.GetGroupNameId(newGroup1));
        Assert.AreEqual(groupNameId2, _windowsDeviceLocalGroupsService.GetGroupNameId(newGroup2));

        _windowsDeviceGroupProviderMock.Verify(x => x.GetLocalGroupNameDataByLocalGroupName(It.IsAny<string>()), Times.Exactly(2));
    }

    [Test]
    public void SynchronizeLocalGroupDataWithSnapshot_ShouldReturn_WhenLocalGroupKeysDataIsNullOrWhiteSpace()
    {
        var invalidInputs = new[] { null, string.Empty, "   " };
        var localGroupData1 = CreateLocalGroupData(1, DeviceId1, 11, true);
        var localGroupData2 = CreateLocalGroupData(2, DeviceId1, 12, false);

        foreach (var input in invalidInputs)
        {
            _windowsDeviceGroupProviderMock.Invocations.Clear();
            _windowsDeviceGroupProviderMock.Setup(x => x.GetAllLocalGroupsDataByDeviceId(DeviceId1))
                .Returns(new List<WindowsDeviceLocalGroupData> { localGroupData1, localGroupData2 });
            _windowsDeviceGroupProviderMock.Setup(x => x.DeleteByDeviceId(DeviceId1));

            _windowsDeviceLocalGroupsService.SynchronizeLocalGroupDataWithSnapshot(DeviceId1, input);

            _windowsDeviceGroupProviderMock.Verify(x => x.GetAllLocalGroupsDataByDeviceId(It.IsAny<int>()), Times.Once);
            _windowsDeviceGroupProviderMock.Verify(x => x.LocalGroupTableBulkModify(It.IsAny<int>(), It.IsAny<IEnumerable<WindowsDeviceLocalGroupData>>()), Times.Never);
            _programTraceMock.Verify(x => x.Write(
                TraceLevel.Verbose,
                It.IsAny<string>(),
                It.Is<string>(msg => msg.Contains($"Windows LocalGroup key(s) information deleted for {DeviceId1}"))
            ), Times.AtLeastOnce);
        }
    }

    [Test]
    public void BulkModifyLocalGroupUser_ThrowsArgumentOutOfRangeException_ForInvalidDeviceId()
    {
        const int invalidDeviceId = -1;
        var groupMemberships = new Dictionary<string, List<string>> { { "user1", ["Group1"] } };
        var localUserIds = new Dictionary<string, int> { { "user1", 1 } };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.BulkModifyLocalGroupUser(invalidDeviceId, groupMemberships, localUserIds, null));
    }

    [Test]
    public void BulkModifyLocalGroupUser_ThrowsArgumentNullException_ForNullGroupMemberships()
    {
        const int validDeviceId = 1;
        var localUserIds = new Dictionary<string, int> { { "user1", 1 } };

        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupsService.BulkModifyLocalGroupUser(validDeviceId, null, localUserIds, null));
    }

    [Test]
    public void BulkModifyLocalGroupUser_ThrowsArgumentNullException_ForNullLocalUserIds()
    {
        const int validDeviceId = 1;
        var groupMemberships = new Dictionary<string, List<string>> { { "user1", ["Group1"] } };

        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupsService.BulkModifyLocalGroupUser(validDeviceId, groupMemberships, null, null));
    }

    [Test]
    public void BulkModifyLocalGroupUser_Success()
    {
        const int userId1 = 1001;
        const int userId2 = 1002;
        const int existingGroupId = 101;
        const int existingGroupNameId = 101;
        const string existingGroupName = "ExistingGroup";
        const string newGroupName = "NewGroupName";
        var newGroupId = 102;
        var newGroupNameId = 2;

        var groupMemberships = new Dictionary<string, List<string>>
        {
            { "User1", new List<string> { existingGroupName } },
            { "User2", new List<string> { newGroupName } },
            { "User3", null }
        };

        var localUserIds = new Dictionary<string, int>
        {
            { "User1", userId1 },
            { "User2", userId2 }
        };
        var newLocalGroupNameData = new WindowsDeviceLocalGroupNameData { GroupName = newGroupName, GroupNameId = newGroupNameId };

        var groupNameData = new Dictionary<string, WindowsDeviceLocalGroupNameData>
        {
            { existingGroupName, new WindowsDeviceLocalGroupNameData {
                GroupName = existingGroupName,
                GroupNameId = existingGroupNameId
            }}
        };

        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetLocalGroupNameDataByLocalGroupNames(It.IsAny<IEnumerable<string>>()))
            .Returns((IEnumerable<string> names) =>
            {
                return groupNameData
                    .Where(kvp => names.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            });

        var localGroupsData = new List<WindowsDeviceLocalGroupData>
        {
            new() { GroupNameId = 1, GroupId = existingGroupId }
        };
        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetLocalGroupsByDeviceAndNameIds(DeviceId1, It.IsAny<IEnumerable<int>>()))
            .Returns(localGroupsData).Verifiable();

        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetGroupIdByNameId(DeviceId1, GroupNameId))
            .Returns(existingGroupId).Verifiable();
        _windowsDeviceGroupProviderMock
            .Setup(x => x.InsertLocalGroupName(newGroupName, out newGroupNameId));
        _windowsDeviceGroupProviderMock
            .Setup(x => x.LocalGroupUserTableBulkModify(DeviceId1, It.IsAny<IEnumerable<WindowsDeviceLocalGroupUserData>>()))
            .Verifiable();

        _windowsDeviceGroupProviderMock
            .Setup(x => x.UpsertWindowsDeviceLocalGroup(DeviceId1, It.IsAny<int>(), out newGroupId))
            .Verifiable();

        _windowsDeviceGroupProviderMock
            .Setup(x => x.LocalGroupUserTableBulkModify(DeviceId1, It.IsAny<IEnumerable<WindowsDeviceLocalGroupUserData>>()))
            .Verifiable();
        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetLocalGroupNameDataByLocalGroupName(newGroupName))
            .Returns(newLocalGroupNameData).Verifiable();
        _windowsDeviceGroupProviderMock.Setup(x => x.InsertLocalGroupName(newGroupName, out newGroupNameId)).Verifiable();
        _windowsDeviceGroupProviderMock.Setup(x => x.UpsertWindowsDeviceLocalGroup(DeviceId1, It.IsAny<int>(), out newGroupId)).Verifiable();

        _windowsDeviceLocalGroupsService.BulkModifyLocalGroupUser(DeviceId1, groupMemberships, localUserIds, []);

        _windowsDeviceGroupProviderMock.Verify(x => x.GetLocalGroupNameDataByLocalGroupNames(It.IsAny<IEnumerable<string>>()), Times.Once);
        _windowsDeviceGroupProviderMock.Verify(x => x.GetLocalGroupsByDeviceAndNameIds(
            DeviceId1, It.IsAny<IEnumerable<int>>()), Times.Once);
        _windowsDeviceGroupProviderMock.Verify(x => x.InsertLocalGroupName(newGroupName, out newGroupNameId), Times.Once);
        _windowsDeviceGroupProviderMock.Verify(x => x.UpsertWindowsDeviceLocalGroup(
            DeviceId1, newGroupNameId, out newGroupId), Times.Once);
        _windowsDeviceGroupProviderMock.Verify(x => x.LocalGroupUserTableBulkModify(
            DeviceId1, It.IsAny<IEnumerable<WindowsDeviceLocalGroupUserData>>()),
            Times.Once);
    }

    [Test]
    public void GetUserGroupsByWindowsDeviceUserIds_Throws_Exception()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupsService.GetUserGroupsByWindowsDeviceUserIds(null));
    }

    [Test]
    public void GetUserGroupsByWindowsDeviceUserIds_Success()
    {
        var windowsDeviceUserIds = new[] { 1, 2 };
        var userGroupMock = new List<WindowsDeviceLocalGroupUserGroupNameData>
        {
            CreateUserGroupsData(1, 1, GroupName1),
            CreateUserGroupsData(2, 1, GroupName1),
            CreateUserGroupsData(2, 2, GroupName2)
        };
        _windowsDeviceGroupProviderMock.Setup(x => x.GetGroupNamesByWindowsDeviceUserIds(windowsDeviceUserIds)).Returns(userGroupMock);
        var result = _windowsDeviceLocalGroupsService.GetUserGroupsByWindowsDeviceUserIds(windowsDeviceUserIds);
        _windowsDeviceGroupProviderMock.Verify(x => x.GetGroupNamesByWindowsDeviceUserIds(windowsDeviceUserIds), Times.Once);
        var expected = new Dictionary<int, string[]>
        {
            { 1, [GroupName1] },
            { 2, [GroupName1, GroupName2] }
        };
        Assert.IsNotNull(result);
        foreach (var id in windowsDeviceUserIds)
        {
            var userGroups = result[id];
            Assert.AreEqual(expected[id].Length, userGroups.Count());
            Assert.AreEqual(expected[id], userGroups);
        }
    }

    [Test]
    public void InsertLocalGroupName_ThrowsArgumentNullException_ForNullGroupName()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupsService.InsertLocalGroupName(null));
    }

    [Test]
    public void InsertLocalGroupName_Success()
    {
        int groupId;
        _windowsDeviceGroupProviderMock.Setup(x => x.InsertLocalGroupName("TestGroup1", out groupId));
        _windowsDeviceLocalGroupsService.InsertLocalGroupName("TestGroup1");
        _windowsDeviceGroupProviderMock.Verify(x => x.InsertLocalGroupName("TestGroup1", out groupId), Times.Once);
    }

    [Test]
    public void InsertLocalGroupUser_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.InsertLocalGroupUser(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.InsertLocalGroupUser(2, -1));
    }

    [Test]
    public void InsertLocalGroupUser_Success()
    {
        _windowsDeviceGroupProviderMock.Setup(x => x.InsertLocalGroupUser(100151, 1));
        _windowsDeviceLocalGroupsService.InsertLocalGroupUser(100151, 1);
        _windowsDeviceGroupProviderMock.Verify(x => x.InsertLocalGroupUser(100151, 1), Times.Once);
    }

    [Test]
    public void UpsertWindowsDeviceLocalGroup_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.UpsertWindowsDeviceLocalGroup(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupsService.UpsertWindowsDeviceLocalGroup(2, -1));
    }

    [Test]
    public void UpsertWindowsDeviceLocalGroup_Success()
    {
        int windowsDeviceLocalGroupId;
        _windowsDeviceGroupProviderMock.Setup(x => x.UpsertWindowsDeviceLocalGroup(1001, 1, out windowsDeviceLocalGroupId));
        _windowsDeviceLocalGroupsService.UpsertWindowsDeviceLocalGroup(1001, 1);
        _windowsDeviceGroupProviderMock.Verify(x => x.UpsertWindowsDeviceLocalGroup(1001, 1, out windowsDeviceLocalGroupId), Times.Once);
    }

    [Test]
    public void InvalidateCache_NotifyMsAndDse()
    {
        _messagePublisherMock.Setup(x => x.Publish(new LocalGroupCacheClearMessage(), ApplicableServer.Ms, ApplicableServer.Dse)).Verifiable();

        _windowsDeviceLocalGroupsService.InvalidateCache();

        _messagePublisherMock.Verify(x => x.Publish(It.IsAny<LocalGroupCacheClearMessage>(), ApplicableServer.Ms, ApplicableServer.Dse), Times.Once);
    }

    [Test]
    public void InvalidateCache_NotNotifyMsAndDse()
    {
        _messagePublisherMock.Setup(x => x.Publish(new LocalGroupCacheClearMessage(), ApplicableServer.Ms, ApplicableServer.Dse)).Verifiable();

        _windowsDeviceLocalGroupsService.InvalidateCache(false);

        _messagePublisherMock.Verify(x => x.Publish(It.IsAny<LocalGroupCacheClearMessage>(), ApplicableServer.Ms, ApplicableServer.Dse), Times.Never);
    }

    [Test]
    public void CleanUpOrphanedLocalGroupNamesData_Success()
    {
        _windowsDeviceGroupProviderMock.Setup(x => x.CleanUpLocalGroupNamesData()).Returns((2, new List<int>() { 1, 2, 3 })).Verifiable();

        var result = _windowsDeviceLocalGroupsService.CleanUpOrphanedLocalGroupNamesData();
        Assert.AreEqual(2, result);

        _windowsDeviceGroupProviderMock.Verify(x => x.CleanUpLocalGroupNamesData(), Times.Once);
    }

    [Test]
    public void GetAllGroupNames_ReturnsEmptyList_WhenProviderReturnsNull()
    {
        _windowsDeviceGroupProviderMock.Setup(p => p.GetAllLocalGroupNames()).Returns((IReadOnlyList<WindowsDeviceLocalGroupNameData>)null);

        var result = _windowsDeviceLocalGroupsService.GetAllGroupNames();

        _windowsDeviceGroupProviderMock.Verify(x => x.GetAllLocalGroupNames(), Times.Once);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void GetAllGroupNames_ReturnsCorrectGroupNames_WhenProviderReturnsData()
    {
        var mockData = new List<WindowsDeviceLocalGroupNameData>
        {
            new() { GroupNameId = 1, GroupName = GroupName1 },
            new() { GroupNameId = 2, GroupName = GroupName2 }
        };
        _windowsDeviceGroupProviderMock.Setup(p => p.GetAllLocalGroupNames()).Returns(mockData);

        var result = _windowsDeviceLocalGroupsService.GetAllGroupNames();

        _windowsDeviceGroupProviderMock.Verify(x => x.GetAllLocalGroupNames(), Times.Once);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(1, result[0].GroupNameId);
        Assert.AreEqual(GroupName1, result[0].GroupName);
        Assert.AreEqual(2, result[1].GroupNameId);
        Assert.AreEqual(GroupName2, result[1].GroupName);
    }

    [Test]
    public void GetGroupNamesByGroupNameIds_WithValidIds_ReturnsMappedSummaries()
    {
        var groupNameIds = new List<int> { 1, 2 };
        var providerData = new List<WindowsDeviceLocalGroupNameData>
        {
            new() { GroupNameId = 1, GroupName = GroupName1 },
            new() { GroupNameId = 2, GroupName = GroupName2 }
        };

        _windowsDeviceGroupProviderMock
            .Setup(p => p.GetLocalGroupNameByIds(groupNameIds))
            .Returns(providerData);

        var result = _windowsDeviceLocalGroupsService.GetGroupNamesByGroupNameIds(groupNameIds).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(1, result[0].GroupNameId);
        Assert.AreEqual(GroupName1, result[0].GroupName);
        Assert.AreEqual(2, result[1].GroupNameId);
        Assert.AreEqual(GroupName2, result[1].GroupName);

        _windowsDeviceGroupProviderMock.Verify(p => p.GetLocalGroupNameByIds(groupNameIds), Times.Once);
    }

    [Test]
    public void GetGroupNamesByGroupNameIds_WhenProviderReturnsEmpty_ReturnsEmptyList()
    {
        var groupNameIds = new List<int> { InvalidGroupNameId };
        _windowsDeviceGroupProviderMock
            .Setup(p => p.GetLocalGroupNameByIds(groupNameIds))
            .Returns([]);

        var result = _windowsDeviceLocalGroupsService.GetGroupNamesByGroupNameIds(groupNameIds).ToList();

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void GetDeviceLocalGroupWatermarksByDeviceIds_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupsService.GetDeviceLocalGroupWatermarksByDeviceIds(null));
    }

    [Test]
    public void GetDeviceLocalGroupWatermarksByDeviceIds_EmptyList_ReturnsEmptyCollection()
    {
        var result = _windowsDeviceLocalGroupsService.GetDeviceLocalGroupWatermarksByDeviceIds(Enumerable.Empty<int>());

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void GetDeviceLocalGroupWatermarksByDeviceIds_ValidList_ReturnsExpectedData()
    {
        var deviceIds = new List<int> { DeviceId1, DeviceId2 };

        var expected = new List<DeviceWindowsLocalGroupSearchDataSummary>
        {
            new()
            {
                DeviceId = DeviceId1, GroupNameId = 1 , ModificationWatermark = -32567
            },
            new()
            {
                DeviceId = DeviceId1, GroupNameId = 2 , ModificationWatermark = -32567
            },
            new()
            {
                DeviceId = DeviceId2, GroupNameId = 2 , ModificationWatermark = -32557
            }
        };

        _windowsDeviceGroupProviderMock
            .Setup(p => p.GetDeviceLocalGroupWatermarks(It.Is<IEnumerable<int>>(ids => ids.SequenceEqual(deviceIds))))
            .Returns(expected);

        var result = _windowsDeviceLocalGroupsService.GetDeviceLocalGroupWatermarksByDeviceIds(deviceIds);

        Assert.IsNotNull(result);
        Assert.AreEqual(expected.Count, result.Count);
    }

    [Test]
    public void GetDeviceLocalGroupsSearchDataByDeviceIdAndGroupNameId_NullEntries_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => _windowsDeviceLocalGroupsService.GetDeviceLocalGroupsSearchDataByDeviceIdAndGroupNameId(null)
        );
    }

    [Test]
    public void GetDeviceLocalGroupsSearchDataByDeviceIdAndGroupNameId_ValidEntries_ReturnsExpectedData()
    {
        var entries = new List<(int, int)>
        {
            (1, 100),
            (2, 200)
        };

        var expectedData = new List<DeviceWindowsLocalGroupSearchDataSummary>
        {
            new() { GroupNameId = 1, DeviceId = 100 },
            new() { GroupNameId = 2, DeviceId = 200 }
        };

        _windowsDeviceGroupProviderMock
            .Setup(provider => provider.GetDeviceLocalGroupsSearchData(It.IsAny<IReadOnlyList<WindowsDeviceLocalGroupData>>()))
            .Returns(expectedData);

        var result = _windowsDeviceLocalGroupsService.GetDeviceLocalGroupsSearchDataByDeviceIdAndGroupNameId(entries);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(1, result[0].GroupNameId);
        Assert.AreEqual(100, result[0].DeviceId);
        Assert.AreEqual(2, result[result.Count - 1].GroupNameId);
        Assert.AreEqual(200, result[result.Count - 1].DeviceId);
    }

    [Test]
    public void GetGroupNameIdsByNames_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupsService.GetGroupNameIdsByNames(null));
    }

    [Test]
    public void GetGroupNameIdsByNames_ReturnEmptyIfGroupDataFromDbIsNull_SingleGroup()
    {
        _windowsDeviceLocalGroupsService.InvalidateCache();
        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetLocalGroupNameDataByLocalGroupName(It.IsAny<string>()))
            .Returns((WindowsDeviceLocalGroupNameData)null).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetGroupNameIdsByNames([GroupName1]);

        _windowsDeviceGroupProviderMock.Verify(x => x.GetLocalGroupNameDataByLocalGroupName(It.IsAny<string>()), Times.Once);

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void GetGroupNameIdsByNames_ReturnEmptyIfGroupDataFromDbIsNull_MultipleGroups()
    {
        _windowsDeviceLocalGroupsService.InvalidateCache();
        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetLocalGroupNameDataByLocalGroupNames(It.IsAny<IEnumerable<string>>()))
            .Returns((Dictionary<string, WindowsDeviceLocalGroupNameData>)null).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetGroupNameIdsByNames([GroupName1, GroupName2]);

        _windowsDeviceGroupProviderMock
            .Verify(x => x.GetLocalGroupNameDataByLocalGroupNames(It.IsAny<IEnumerable<string>>()), Times.Once);

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public void GetGroupNameIdsByNames_Success_ReturnFromCache()
    {
        _windowsDeviceLocalGroupsService.SetCachedGroupData(GroupNameId, GroupName1);

        var result = _windowsDeviceLocalGroupsService.GetGroupNameIdsByNames([GroupName1]);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(GroupNameId, result[GroupName1]);
    }

    [Test]
    public void GetGroupNameIdsByNames_Success_ReturnFromDb_SingleGroup()
    {
        _windowsDeviceLocalGroupsService.InvalidateCache();

        var localGroupNameData = CreateLocalGroupNameData(GroupNameId, GroupName1);

        _windowsDeviceGroupProviderMock.Setup(x => x.GetLocalGroupNameDataByLocalGroupName(It.IsAny<string>()))
            .Returns(localGroupNameData).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetGroupNameIdsByNames([GroupName1]);

        _windowsDeviceGroupProviderMock.Verify(x => x.GetLocalGroupNameDataByLocalGroupName(It.IsAny<string>()), Times.Once);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(GroupNameId, result[GroupName1]);
    }

    [Test]
    public void GetGroupNameIdsByNames_Success_ReturnFromDb_MultipleGroups()
    {
        _windowsDeviceLocalGroupsService.InvalidateCache();

        var groupDataFromDb = new Dictionary<string, WindowsDeviceLocalGroupNameData>
        {
            {
                GroupName1,
                new WindowsDeviceLocalGroupNameData
                {
                    GroupNameId = GroupNameId,
                    GroupName = GroupName1
                }
            },
            {
                GroupName2,
                new WindowsDeviceLocalGroupNameData
                {
                    GroupNameId = GroupNameId + 1,
                    GroupName = GroupName2
                }
            }
        };

        _windowsDeviceGroupProviderMock
            .Setup(x => x.GetLocalGroupNameDataByLocalGroupNames(It.IsAny<IEnumerable<string>>()))
            .Returns(groupDataFromDb).Verifiable();

        var result = _windowsDeviceLocalGroupsService.GetGroupNameIdsByNames([GroupName1, GroupName2]);

        _windowsDeviceGroupProviderMock
            .Verify(x => x.GetLocalGroupNameDataByLocalGroupNames(It.IsAny<IEnumerable<string>>()), Times.Once);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(GroupNameId, result[GroupName1]);
        Assert.AreEqual(GroupNameId + 1, result[GroupName2]);
    }

    private static WindowsDeviceLocalGroupNameData CreateLocalGroupNameData(int groupNameId, string groupName)
    {
        return new WindowsDeviceLocalGroupNameData()
        {
            GroupNameId = groupNameId,
            GroupName = groupName
        };
    }

    private static WindowsDeviceLocalGroupData CreateLocalGroupData(int groupId, int deviceId, int groupNameId, bool isAdminGroup)
    {
        return new WindowsDeviceLocalGroupData()
        {
            GroupId = groupId,
            DeviceId = deviceId,
            GroupNameId = groupNameId,
            IsAdminGroup = isAdminGroup
        };
    }

    private static WindowsDeviceLocalGroupUserGroupNameData CreateUserGroupsData(int windowsDeviceUserId, int groupNameId, string groupName)
    {
        return new WindowsDeviceLocalGroupUserGroupNameData
        {
            WindowsDeviceUserId = windowsDeviceUserId,
            WindowsDeviceLocalGroupNameId = groupNameId,
            LocalGroupName = groupName
        };
    }
}
