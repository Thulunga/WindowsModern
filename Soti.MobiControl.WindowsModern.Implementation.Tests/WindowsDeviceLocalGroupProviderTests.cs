using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.Test.Common.SensitiveDataProtection;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.WindowsModern.Models;
using Soti.SensitiveDataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
public sealed class WindowsDeviceLocalGroupProviderTests : ProviderTestsBase
{
    private static readonly TestDeviceProvider TestDeviceProvider = new TestDeviceProvider();
    private IWindowsDeviceUserProvider _windowsDeviceUserProvider;
    private WindowsDeviceLocalGroupProvider _windowsDeviceLocalGroupProvider;
    private WindowsDeviceDataProvider _windowsDeviceProvider;
    private IDataKeyService _dataKeyService;
    private Mock<IProgramTrace> _programTraceMock;

    private readonly string _devId1 = Guid.NewGuid().ToString();
    private readonly string _devId2 = Guid.NewGuid().ToString();
    private int _deviceId1;
    private int _deviceId2;

    private const string TestGroup1 = "TestGroup1";
    private const string TestGroup2 = "TestGroup2";
    private const string UserName = "UserName";
    private const string UserSid = "S-1-2-3-4";
    private const int GroupNameId = 11;
    private const string LocalGroupIdParameter = "localGroupId";
    private const string GroupNameIdParameter = "groupNameId";
    private const string DeviceIdParameter = "deviceId";
    private const string EntriesParameter = "entries";
    private const string GroupNameIdsParameter = "groupNameIds";
    private const int InvalidLocalGroupId = -1;
    private const int InvalidGroupNameId = -1;
    private const int InvalidDeviceId = -1;
    private const int ManagementDevicesGroupId = 2;

    [SetUp]
    public void Setup()
    {
        _programTraceMock = new Mock<IProgramTrace>();
        _windowsDeviceUserProvider = new WindowsDeviceUserProvider(Database, _programTraceMock.Object);
        _windowsDeviceLocalGroupProvider = new WindowsDeviceLocalGroupProvider(Database);
        _windowsDeviceProvider = new WindowsDeviceDataProvider(Database);
        _dataKeyService = new TestDataKeyService(Database);
    }

    [TearDown]
    public void TearDown()
    {
        DeleteTestData(_deviceId1);
        DeleteTestData(_deviceId2);
    }

    [Test]
    public void GetAllLocalGroupsDataByDeviceId_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(InvalidDeviceId));
    }

    [Test]
    public void GetAllLocalGroupsDataByDeviceId_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };

        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);
        var localGroupsData = _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(_deviceId1);
        foreach (var localGroup in localGroupsData)
        {
            Assert.AreEqual(localGroup.GroupNameId, windowsDeviceLocalGroupData1.GroupNameId);
            Assert.AreEqual(localGroup.IsAdminGroup, windowsDeviceLocalGroupData1.IsAdminGroup);
        }
    }

    [Test]
    public void GetDistinctLocalGroupNameIdsByDeviceIds_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetDistinctLocalGroupNameIdsByDeviceIds(null));
    }

    [Test]
    public void GetDistinctLocalGroupNameIdsByDeviceIds_EmptyListTest()
    {
        var result = _windowsDeviceLocalGroupProvider.GetDistinctLocalGroupNameIdsByDeviceIds(new List<int>());
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetDistinctLocalGroupNameIdsByDeviceIds_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };
        _windowsDeviceProvider.Insert(windowsDeviceData);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);
        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, false);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };
        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var result = _windowsDeviceLocalGroupProvider.GetDistinctLocalGroupNameIdsByDeviceIds([_deviceId1]);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(groupNameId1, result[0]);
    }

    [Test]
    public void GetDistinctLocalGroupNameIdsByDeviceGroupIds_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetDistinctLocalGroupNameIdsByDeviceGroupIds(null));
    }

    [Test]
    public void GetDistinctLocalGroupNameIdsByDeviceGroupIds_EmptyListTest()
    {
        var result = _windowsDeviceLocalGroupProvider.GetDistinctLocalGroupNameIdsByDeviceGroupIds(new HashSet<int>());
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetDistinctLocalGroupNameIdsByDeviceGroupIds_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };
        _windowsDeviceProvider.Insert(windowsDeviceData);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);
        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, false);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };
        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var result = _windowsDeviceLocalGroupProvider.GetDistinctLocalGroupNameIdsByDeviceGroupIds(new HashSet<int> { ManagementDevicesGroupId });

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(groupNameId1, result[0]);
    }

    [Test]
    public void GetAllLocalGroupNames_ShouldReturnCorrectData_WhenRecordsExist()
    {
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out _);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup2, out _);

        var result = _windowsDeviceLocalGroupProvider.GetAllLocalGroupNames().ToList();

        Assert.IsTrue(result.Any(x => x.GroupName == TestGroup1));
        Assert.IsTrue(result.Any(x => x.GroupName == TestGroup2));
    }

    [Test]
    public void GetLocalGroupNameDataByLocalGroupName_ShouldThrowArgumentNullException_WhenLocalGroupNameIsNullOrWhiteSpace()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupProvider.GetLocalGroupNameDataByLocalGroupName(null));
    }

    [Test]
    public void GetLocalGroupNameDataByLocalGroupName_ShouldReturnCorrectData_WhenLocalGroupNameExists()
    {
        var expectedData = new WindowsDeviceLocalGroupNameData { GroupName = TestGroup1 };

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out _);

        var result = _windowsDeviceLocalGroupProvider.GetLocalGroupNameDataByLocalGroupName(TestGroup1);

        Assert.NotNull(result);
        Assert.AreEqual(expectedData.GroupName, result.GroupName);
    }

    [Test]
    public void GetAllLocalGroupUserDataByDeviceId_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };

        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);
        var localGroupsData = _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(_deviceId1);
        foreach (var localGroup in localGroupsData)
        {
            Assert.AreEqual(localGroup.GroupNameId, windowsDeviceLocalGroupData1.GroupNameId);
            Assert.AreEqual(localGroup.IsAdminGroup, windowsDeviceLocalGroupData1.IsAdminGroup);
        }
    }

    [Test]
    public void LocalGroupTableBulkModify_ShouldThrowArgumentOutOfRangeException_WhenDeviceIdIsInvalid()
    {
        const int invalidDeviceId = 0;
        var validEntries = new List<WindowsDeviceLocalGroupData>
        {
            new() { GroupId = 1, DeviceId = 1, GroupNameId = 1, IsAdminGroup = true }
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(invalidDeviceId, validEntries));
    }

    [Test]
    public void LocalGroupTableBulkModify_ShouldThrowArgumentNullException_WhenEntriesIsEmpty()
    {
        const int validDeviceId = 1;
        var emptyEntries = new List<WindowsDeviceLocalGroupData>();

        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(validDeviceId, emptyEntries));
    }

    [Test]
    public void LocalGroupUserTableBulkModify_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId);

        var windowsDeviceLocalGroupData = GetWindowsDeviceGroupData(_deviceId1, groupNameId, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData };

        var windowsDeviceUserData = GetWindowsDeviceUserData(UserName, _deviceId1, false, UserSid, [TestGroup1], GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceUserData);

        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var windowsDeviceLocalGroupUserData = GetWindowsDeviceUserGroupData(_windowsDeviceLocalGroupProvider.GetGroupIdByNameId(_deviceId1, groupNameId), windowsDeviceUserData.WindowsDeviceUserId);
        var windowsDeviceLocalGroupUserDataEntries = new List<WindowsDeviceLocalGroupUserData> { windowsDeviceLocalGroupUserData };

        _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModify(_deviceId1, windowsDeviceLocalGroupUserDataEntries);

        Assert.DoesNotThrow(() => _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModify(_deviceId1, windowsDeviceLocalGroupUserDataEntries));
    }

    [Test]
    public void LocalGroupUserTableBulkModify_ShouldThrowArgumentOutOfRangeException_WhenDeviceIdIsInvalid()
    {
        const int invalidDeviceId = 0;
        var validEntries = new List<WindowsDeviceLocalGroupUserData>
        {
            new() { GroupId = 1, UserId = 1 }
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModify(invalidDeviceId, validEntries));
    }

    [Test]
    public void LocalGroupUserTableBulkModify_ShouldThrowArgumentNullException_WhenEntriesIsNull()
    {
        const int validDeviceId = 1;
        List<WindowsDeviceLocalGroupUserData> nullEntries = null;

        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModify(validDeviceId, nullEntries));
    }

    [Test]
    public void DeleteByDeviceId_ShouldThrowArgumentOutOfRangeException_WhenDeviceIdIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.DeleteByDeviceId(0));
    }

    [Test]
    public void InsertLocalGroupName_ShouldThrowArgumentNullException_WhenLocalGroupNameIsEmpty()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.InsertLocalGroupName("", out _));
    }

    [Test]
    public void GetGroupIdByNameId_ShouldThrowArgumentOutOfRangeException_WhenDeviceIdIsInvalid()
    {
        const int invalidDeviceId = 0;
        const int groupNameId = 1;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetGroupIdByNameId(invalidDeviceId, groupNameId));
    }

    [Test]
    public void GetGroupIdByNameId_ShouldThrowArgumentOutOfRangeException_WhenGroupNameIsInvalid()
    {
        const int deviceId = 1;
        const int invalidGroupNameId = 0;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetGroupIdByNameId(deviceId, invalidGroupNameId));
    }

    [Test]
    public void GetGroupIdByNameId_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };
        _windowsDeviceProvider.Insert(windowsDeviceData);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);
        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };
        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);
        var groupId = _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(_deviceId1)[0].GroupId;

        var result = _windowsDeviceLocalGroupProvider.GetGroupIdByNameId(_deviceId1, groupNameId1);

        Assert.AreEqual(result, groupId);
    }

    [Test]
    public void LocalGroupUserTableBulkModifyForGroup_ValidInput_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);

        var windowsDeviceLocalGroupData = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData };
        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var windowsDeviceUserData = GetWindowsDeviceUserData(UserName, _deviceId1, false, UserSid, [TestGroup1], GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceUserData);

        var getGroupIdByGroupNameId = _windowsDeviceLocalGroupProvider.GetGroupIdByNameId(_deviceId1, groupNameId1);
        var windowsDeviceLocalGroupUserData = GetWindowsDeviceUserGroupData(getGroupIdByGroupNameId, windowsDeviceUserData.WindowsDeviceUserId);
        var windowsDeviceUserIdList = new List<int> { windowsDeviceLocalGroupUserData.UserId };

        _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModifyForGroup(windowsDeviceUserIdList, getGroupIdByGroupNameId);

        Assert.DoesNotThrow(() => _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModifyForGroup(windowsDeviceUserIdList, getGroupIdByGroupNameId));
    }

    [Test]
    public void LocalGroupUserTableBulkModifyForGroup_InvalidLocalGroupId_ThrowsArgumentOutOfRangeException()
    {
        var validUserIdEntries = new List<int> { 1, 2, 3 };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModifyForGroup(validUserIdEntries, InvalidLocalGroupId));

        Assert.That(ex?.ParamName, Is.EqualTo(LocalGroupIdParameter));
    }

    [Test]
    public void LocalGroupUserTableBulkModifyForGroup_NullUserIdEntries_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModifyForGroup(null, 1));

        Assert.That(ex?.ParamName, Is.EqualTo(EntriesParameter));
    }

    [Test]
    public void GetLocalGroupUserDetailsByDeviceIdAndGroupId_ValidInput_ReturnsUserDetails()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };
        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var windowsDeviceUserData = GetWindowsDeviceUserData(UserName, _deviceId1, false, UserSid, [TestGroup1], GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceUserData);

        var getGroupIdByGroupNameId = _windowsDeviceLocalGroupProvider.GetGroupIdByNameId(_deviceId1, groupNameId1);
        var windowsDeviceLocalGroupUserData = GetWindowsDeviceUserGroupData(getGroupIdByGroupNameId, windowsDeviceUserData.WindowsDeviceUserId);
        var windowsDeviceLocalGroupUserDataEntries = new List<WindowsDeviceLocalGroupUserData> { windowsDeviceLocalGroupUserData };

        _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModify(_deviceId1, windowsDeviceLocalGroupUserDataEntries);

        var expectedUserDetails = new List<WindowsDeviceLocalGroupUserDetailsData>
        {
            new()
            {
                WindowsDeviceUserId = windowsDeviceUserData.WindowsDeviceUserId,
                UserName = UserName,
                UserSid = UserSid,
                DataKeyId = GenerateDataKey(),
                IsAdminGroup = true
            },
        };

        var result = _windowsDeviceLocalGroupProvider.GetLocalGroupUserDetailsByDeviceIdAndGroupId(_deviceId1, groupNameId1);

        var windowsDeviceLocalGroupUserDetailsDataResult = result.ToList()[0];
        Assert.That(windowsDeviceLocalGroupUserDetailsDataResult, Is.Not.Null);
        Assert.AreEqual(windowsDeviceLocalGroupUserDetailsDataResult.WindowsDeviceUserId, expectedUserDetails[0].WindowsDeviceUserId);
        Assert.AreEqual(windowsDeviceLocalGroupUserDetailsDataResult.UserName, expectedUserDetails[0].UserName);
    }

    [Test]
    public void GetLocalGroupUserDetailsByDeviceIdAndGroupId_InvalidDeviceId_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupUserDetailsByDeviceIdAndGroupId(InvalidDeviceId, GroupNameId));

        Assert.That(ex?.ParamName, Is.EqualTo(DeviceIdParameter));
    }

    [Test]
    public void GetLocalGroupUserDetailsByDeviceIdAndGroupId_InvalidGroupNameId_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupUserDetailsByDeviceIdAndGroupId(_deviceId1, InvalidGroupNameId));

        Assert.That(ex?.ParamName, Is.EqualTo(GroupNameIdParameter));
    }

    [Test]
    public void GetLocalGroupUserDetailsByDeviceIdsAndGroupIds_ExceptionTest()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupUserDetailsByDeviceIdsAndGroupId(null, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupUserDetailsByDeviceIdsAndGroupId(new List<int>() { 1, 2 }, 0));
    }

    [Test]
    public void GetLocalGroupUserDetailsByDeviceIdsAndGroupIds_EmptyListTest()
    {
        var result = _windowsDeviceLocalGroupProvider.GetLocalGroupUserDetailsByDeviceIdsAndGroupId(new List<int>(), 1);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetLocalGroupUserDetailsByDeviceIdsAndGroupIds_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };
        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var windowsDeviceUserData = GetWindowsDeviceUserData(UserName, _deviceId1, false, UserSid, [TestGroup1], GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceUserData);

        var getGroupIdByGroupNameId = _windowsDeviceLocalGroupProvider.GetGroupIdByNameId(_deviceId1, groupNameId1);
        var windowsDeviceLocalGroupUserData = GetWindowsDeviceUserGroupData(getGroupIdByGroupNameId, windowsDeviceUserData.WindowsDeviceUserId);
        var windowsDeviceLocalGroupUserDataEntries = new List<WindowsDeviceLocalGroupUserData> { windowsDeviceLocalGroupUserData };

        _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModify(_deviceId1, windowsDeviceLocalGroupUserDataEntries);

        var expectedUserDetails = new WindowsDeviceLocalGroupUserDetailsData
        {
            DeviceId = _deviceId1,
            WindowsDeviceUserId = windowsDeviceUserData.WindowsDeviceUserId,
            UserName = UserName,
            UserSid = UserSid,
            DataKeyId = GenerateDataKey(),
            IsAdminGroup = true
        };

        var result = _windowsDeviceLocalGroupProvider.GetLocalGroupUserDetailsByDeviceIdsAndGroupId([_deviceId1], groupNameId1);

        var windowsDeviceLocalGroupUserDetailsDataResult = result.ToList()[0];
        Assert.That(windowsDeviceLocalGroupUserDetailsDataResult, Is.Not.Null);
        Assert.AreEqual(expectedUserDetails.DeviceId, windowsDeviceLocalGroupUserDetailsDataResult.DeviceId);
        Assert.AreEqual(expectedUserDetails.WindowsDeviceUserId, windowsDeviceLocalGroupUserDetailsDataResult.WindowsDeviceUserId);
        Assert.AreEqual(expectedUserDetails.UserName, windowsDeviceLocalGroupUserDetailsDataResult.UserName);
    }

    [Test]
    public void GetUserNamesByDeviceIdAndGroupNameIds_ExceptionTest()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetUserNamesByDeviceIdAndGroupNameIds(0, [1]));
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetUserNamesByDeviceIdAndGroupNameIds(1, null));
    }

    [Test]
    public void GetUserNamesByDeviceIdAndGroupNameIds_EmptyListTest()
    {
        var result = _windowsDeviceLocalGroupProvider.GetUserNamesByDeviceIdAndGroupNameIds(1, []);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetUserNamesByDeviceIdAndGroupNameIds_SuccessTest()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };
        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var windowsDeviceUserData = GetWindowsDeviceUserData(UserName, _deviceId1, false, UserSid, [TestGroup1], GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceUserData);

        var getGroupIdByGroupNameId = _windowsDeviceLocalGroupProvider.GetGroupIdByNameId(_deviceId1, groupNameId1);
        var windowsDeviceLocalGroupUserData = GetWindowsDeviceUserGroupData(getGroupIdByGroupNameId, windowsDeviceUserData.WindowsDeviceUserId);
        var windowsDeviceLocalGroupUserDataEntries = new List<WindowsDeviceLocalGroupUserData> { windowsDeviceLocalGroupUserData };

        _windowsDeviceLocalGroupProvider.LocalGroupUserTableBulkModify(_deviceId1, windowsDeviceLocalGroupUserDataEntries);

        var expectedUserDetails = new WindowsDeviceLocalGroupUserDetailsModal
        {
            GroupNameId = groupNameId1,
            WindowsDeviceUserId = windowsDeviceUserData.WindowsDeviceUserId,
            UserName = UserName
        };

        var result = _windowsDeviceLocalGroupProvider.GetUserNamesByDeviceIdAndGroupNameIds(_deviceId1, [groupNameId1]);

        var windowsDeviceLocalGroupUserDetailsDataResult = result[0];
        Assert.That(windowsDeviceLocalGroupUserDetailsDataResult, Is.Not.Null);
        Assert.AreEqual(expectedUserDetails.GroupNameId, windowsDeviceLocalGroupUserDetailsDataResult.GroupNameId);
        Assert.AreEqual(expectedUserDetails.WindowsDeviceUserId, windowsDeviceLocalGroupUserDetailsDataResult.WindowsDeviceUserId);
        Assert.AreEqual(expectedUserDetails.UserName, windowsDeviceLocalGroupUserDetailsDataResult.UserName);
    }

    [Test]
    public void GetLocalGroupIdByGroupNameId_ValidInput_ReturnsLocalGroupData()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };

        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var groupId = _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(_deviceId1).FirstOrDefault()!.GroupId;

        var expectedGroupData = new WindowsDeviceLocalGroupData
        {
            GroupId = groupId,
            GroupNameId = groupNameId1
        };

        var result = _windowsDeviceLocalGroupProvider.GetLocalGroupIdByGroupNameId(_deviceId1, groupNameId1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.GroupId, Is.EqualTo(expectedGroupData.GroupId));
    }

    [Test]
    public void GetLocalGroupIdByGroupNameId_InvalidDeviceId_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupIdByGroupNameId(InvalidDeviceId, GroupNameId));

        Assert.That(ex?.ParamName, Is.EqualTo(DeviceIdParameter));
    }

    [Test]
    public void GetLocalGroupIdByGroupNameId_InvalidGroupNameId_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupIdByGroupNameId(_deviceId1, InvalidGroupNameId));

        Assert.That(ex?.ParamName, Is.EqualTo(GroupNameIdParameter));
    }

    [Test]
    public void GetLocalGroupNameByIds_WithValidIds_ReturnsExpectedData()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1 };

        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var expectedGroupNameIds = new List<int> { groupNameId1 };

        var result = _windowsDeviceLocalGroupProvider.GetLocalGroupNameByIds(expectedGroupNameIds);

        Assert.IsNotNull(result);
        var resultList = result.ToList();
        Assert.IsTrue(resultList.Count > 0);

        foreach (var item in resultList)
        {
            Assert.Contains(item.GroupNameId, expectedGroupNameIds);
            Assert.IsFalse(string.IsNullOrWhiteSpace(item.GroupName));
        }
    }

    [Test]
    public void GetLocalGroupNameByIds_WithEmptyList_ThrowsException()
    {
        var emptyGroupNameIds = new List<int>();

        var ex = Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupNameByIds(emptyGroupNameIds));

        Assert.That(ex?.ParamName, Is.EqualTo(GroupNameIdsParameter));
    }

    [Test]
    public void GetLocalGroupNameByIds_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupNameByIds(null));
    }

    [Test]
    public void GetGroupNamesByWindowsDeviceUserIds_Exceptions()
    {
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupProvider.GetGroupNamesByWindowsDeviceUserIds(null));
        Assert.Throws<ArgumentNullException>(() => _windowsDeviceLocalGroupProvider.GetGroupNamesByWindowsDeviceUserIds(new List<int>()));
    }

    [Test]
    public void GetGroupNamesByWindowsDeviceUserIds_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };
        _windowsDeviceProvider.Insert(windowsDeviceData);

        var windowsDeviceUserData = GetWindowsDeviceUserData("User", _deviceId1, false, "S-1-2-3-4", null, GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceUserData);
        var windowsDeviceUserId = windowsDeviceUserData.WindowsDeviceUserId;

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId);
        _windowsDeviceLocalGroupProvider.UpsertWindowsDeviceLocalGroup(_deviceId1, groupNameId, out var windowsDeviceLocalGroupId);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupUser(windowsDeviceUserId, windowsDeviceLocalGroupId);

        var res = _windowsDeviceLocalGroupProvider.GetGroupNamesByWindowsDeviceUserIds(new[] { windowsDeviceUserId });
        var result = res.FirstOrDefault(x => x.WindowsDeviceUserId == windowsDeviceUserId);

        Assert.IsNotNull(res);
        Assert.IsNotNull(result);
        Assert.AreEqual(groupNameId, result.WindowsDeviceLocalGroupNameId);
        Assert.AreEqual(TestGroup1, result.LocalGroupName);
    }

    [TestCase(0, 1)]
    [TestCase(1, 0)]
    public void UpsertWindowsDeviceLocalGroup_ThrowsException(int deviceId, int groupNameId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceLocalGroupProvider.UpsertWindowsDeviceLocalGroup(deviceId, groupNameId, out var _));
    }

    [TestCase(0, 1)]
    [TestCase(1, 0)]
    public void InsertLocalGroupUser_ThrowsException(int windowsDeviceUserId, int windowsDeviceLocalGroupId)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _windowsDeviceLocalGroupProvider.InsertLocalGroupUser(windowsDeviceUserId, windowsDeviceLocalGroupId));
    }

    [Test]
    public void UpsertWindowsDeviceLocalGroup_ShouldReturnCorrectData_WhenRecordsExist()
    {
        _deviceId1 = GetDeviceId(_devId1);

        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);
        _windowsDeviceLocalGroupProvider.UpsertWindowsDeviceLocalGroup(_deviceId1, groupNameId1, out var windowsDeviceLocalGroupId);
        var result = _windowsDeviceLocalGroupProvider.GetAllLocalGroupsDataByDeviceId(_deviceId1).ToList();
        Assert.IsTrue(result.Any(x => x.GroupId == windowsDeviceLocalGroupId));

        var windowsDeviceData1 = GetWindowsDeviceUserData(UserName, _deviceId1, false, UserSid, [TestGroup1], GenerateDataKey());
        _windowsDeviceUserProvider.Insert(windowsDeviceData1);
        var data = _windowsDeviceUserProvider.GetLocalUsersDataByDeviceIdAndSid(_deviceId1, UserSid);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupUser(data.WindowsDeviceUserId, windowsDeviceLocalGroupId);
    }

    [Test]
    public void GetDeviceLocalGroupWatermarks_ShouldReturnDataFromDatabase()
    {
        _deviceId1 = GetDeviceId(_devId1);
        _deviceId2 = GetDeviceId(_devId2);

        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup2, out var groupNameId2);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupData2 = GetWindowsDeviceGroupData(_deviceId2, groupNameId2, false);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1, windowsDeviceLocalGroupData2 };

        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var deviceIds = new List<int>() { _deviceId1, _deviceId2 };

        var result = _windowsDeviceLocalGroupProvider.GetDeviceLocalGroupWatermarks(deviceIds);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count != 0, "Result should not be empty.");

        Assert.AreEqual(2, result.Count, "Expected 2 records to be returned.");
    }

    [Test]
    public void GetDeviceLocalGroupsSearchData_WithValidEntries_ReturnsExpectedResults()
    {
        _deviceId1 = GetDeviceId(_devId1);
        _deviceId2 = GetDeviceId(_devId2);

        var windowsDeviceData1 = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        var windowsDeviceData2 = new WindowsDeviceData
        {
            DeviceId = _deviceId2,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };

        _windowsDeviceProvider.Insert(windowsDeviceData1);
        _windowsDeviceProvider.Insert(windowsDeviceData2);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup2, out var groupNameId2);

        var windowsDeviceLocalGroupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var windowsDeviceLocalGroupData2 = GetWindowsDeviceGroupData(_deviceId2, groupNameId2, false);
        var windowsDeviceLocalGroupDataEntries = new List<WindowsDeviceLocalGroupData> { windowsDeviceLocalGroupData1, windowsDeviceLocalGroupData2 };

        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, windowsDeviceLocalGroupDataEntries);

        var result = _windowsDeviceLocalGroupProvider.GetDeviceLocalGroupsSearchData(windowsDeviceLocalGroupDataEntries);

        Assert.IsNotNull(result, "Result should not be null.");
        Assert.IsNotEmpty(result, "Result should contain at least one item.");
        foreach (var item in result)
        {
            Assert.IsInstanceOf<DeviceWindowsLocalGroupSearchDataSummary>(item);
            Assert.Greater(item.DeviceId, 0);
            Assert.Greater(item.GroupNameId, 0);
        }
    }

    [Test]
    public void GetLocalGroupNameDataByLocalGroupNames_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupNameDataByLocalGroupNames(null));

        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupNameDataByLocalGroupNames(Enumerable.Empty<string>()));
    }

    [Test]
    public void GetLocalGroupNameDataByLocalGroupNames_Success()
    {
        var groupNames = new List<string> { TestGroup1, TestGroup2, TestGroup1 };
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup2, out var groupNameId2);

        var result = _windowsDeviceLocalGroupProvider.GetLocalGroupNameDataByLocalGroupNames(groupNames);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(groupNameId1, result[TestGroup1].GroupNameId);
        Assert.AreEqual(groupNameId2, result[TestGroup2].GroupNameId);
    }

    [Test]
    public void GetLocalGroupsByDeviceAndNameIds_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupsByDeviceAndNameIds(_deviceId1, null));

        Assert.Throws<ArgumentNullException>(() =>
            _windowsDeviceLocalGroupProvider.GetLocalGroupsByDeviceAndNameIds(_deviceId1, Enumerable.Empty<int>()));
    }

    [Test]
    public void GetLocalGroupsByDeviceAndNameIds_Success()
    {
        _deviceId1 = GetDeviceId(_devId1);
        var windowsDeviceData = new WindowsDeviceData
        {
            DeviceId = _deviceId1,
            IsLocked = false,
            LastCheckInDeviceUserTime = DateTime.UtcNow
        };
        _windowsDeviceProvider.Insert(windowsDeviceData);

        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup1, out var groupNameId1);
        _windowsDeviceLocalGroupProvider.InsertLocalGroupName(TestGroup2, out var groupNameId2);

        var groupData1 = GetWindowsDeviceGroupData(_deviceId1, groupNameId1, true);
        var groupData2 = GetWindowsDeviceGroupData(_deviceId1, groupNameId2, false);
        var entries = new List<WindowsDeviceLocalGroupData> { groupData1, groupData2 };
        _windowsDeviceLocalGroupProvider.LocalGroupTableBulkModify(_deviceId1, entries);

        var nameIds = new List<int> { groupNameId1, groupNameId2, groupNameId1 };

        var result = _windowsDeviceLocalGroupProvider.GetLocalGroupsByDeviceAndNameIds(_deviceId1, nameIds).ToList();

        Assert.AreEqual(2, result.Count);

        Assert.IsTrue(result.Any(x => x.GroupNameId == groupNameId1));
        Assert.IsTrue(result.Any(x => x.GroupNameId == groupNameId2));

        var group1 = result.First(x => x.GroupNameId == groupNameId1);
        var group2 = result.First(x => x.GroupNameId == groupNameId2);
        Assert.IsNotNull(group1);
        Assert.IsNotNull(group2);
    }

    private static int GetDeviceId(string devId)
    {
        return TestDeviceProvider.EnsureDeviceRecord(devId, 1004);
    }

    private static WindowsDeviceLocalGroupData GetWindowsDeviceGroupData(
        int deviceId,
        int groupNameId,
        bool isAdminGroup)
    {
        return new WindowsDeviceLocalGroupData
        {
            DeviceId = deviceId,
            GroupNameId = groupNameId,
            IsAdminGroup = isAdminGroup,
        };
    }

    private static WindowsDeviceLocalGroupUserData GetWindowsDeviceUserGroupData(int groupId, int userId)
    {
        return new WindowsDeviceLocalGroupUserData
        {
            GroupId = groupId,
            UserId = userId
        };
    }

    private static WindowsDeviceUserData GetWindowsDeviceUserData(string userName, int deviceId, bool isMobiControlCreated, string userSid, IEnumerable<string> userGroup, int dataKeyId, string password = null)
    {
        return new WindowsDeviceUserData
        {
            UserName = userName,
            DeviceId = deviceId,
            IsMobiControlCreated = isMobiControlCreated,
            UserSID = userSid,
            UserGroups = userGroup,
            AutoGeneratedPassword = password != null ? Encoding.UTF8.GetBytes(password) : null,
            DataKeyId = dataKeyId,
            CreatedDate = DateTime.UtcNow
        };
    }

    private int GenerateDataKey()
    {
        return _dataKeyService.GetKey().Id;
    }

    private void DeleteTestData(int deviceId)
    {
        if (deviceId <= 0)
        {
            return;
        }

        _windowsDeviceLocalGroupProvider.DeleteByDeviceId(deviceId);
        _windowsDeviceLocalGroupProvider.CleanUpLocalGroupNamesData();
        _windowsDeviceUserProvider.DeleteByDeviceId(deviceId);
        TestDeviceProvider.DeleteDevice(deviceId);
    }
}
