using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.MobiControl.Test.Common.Database;
using System;
using System.Linq;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
internal sealed class WindowsDeviceBootPriorityDataProviderTests : ProviderTestsBase
{
    private WindowsDeviceBootPriorityDataProvider _provider;
    private WindowsDeviceDataProvider _deviceDataProvider;
    private readonly TestDeviceProvider _testDeviceProvider = new();
    private int _deviceId;
    private readonly string _devId = Guid.NewGuid().ToString();
    private const int DeviceId = 1004;

    [SetUp]
    public void SetUp()
    {
        _provider = new WindowsDeviceBootPriorityDataProvider(Database);
        _deviceDataProvider = new WindowsDeviceDataProvider(Database);
    }

    [TearDown]
    public void TearDown()
    {
        if (_deviceId > 0)
        {
            _provider.DeleteByDeviceId(_deviceId);
            DeleteTestDevice(_deviceId);
        }
    }

    [Test]
    public void GetByDeviceId_WithExistingData_ReturnsCorrectData()
    {
        _deviceId = GetDeviceId(_devId);

        _deviceDataProvider.Insert(new WindowsDeviceData
        {
            DeviceId = _deviceId
        });

        var bootPriorityData = new[]
        {
            new WindowsDeviceBootPriority { DeviceId = _deviceId, BootPriorityId = 1, BootOrder = 2 }
        };

        _provider.BulkModify(bootPriorityData);

        var result = _provider.GetByDeviceId(_deviceId);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);

        Assert.IsTrue(result.Any(x => x.BootPriorityId == 1 && x.BootOrder == 2));
    }

    [Test]
    public void BulkModify_WithValidData_ModifiesBootPriorityData()
    {
        _deviceId = GetDeviceId(_devId);

        _deviceDataProvider.Insert(new WindowsDeviceData
        {
            DeviceId = _deviceId
        });

        var bootPriorityData = new[]
        {
            new WindowsDeviceBootPriority { DeviceId = _deviceId, BootPriorityId = 1, BootOrder = 1 },
            new WindowsDeviceBootPriority { DeviceId = _deviceId, BootPriorityId = 2, BootOrder = 2 }
        };

        _provider.BulkModify(bootPriorityData);

        var result = _provider.GetByDeviceId(_deviceId);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.BootPriorityId == 1 && x.BootOrder == 1));
        Assert.IsTrue(result.Any(x => x.BootPriorityId == 2 && x.BootOrder == 2));
    }

    [Test]
    public void BulkGetByDeviceId_WithValidDeviceIds_ReturnsCorrectData()
    {
        _deviceId = GetDeviceId(_devId);

        _deviceDataProvider.Insert(new WindowsDeviceData
        {
            DeviceId = _deviceId
        });

        var priorities = _provider.BulkInsertAndGet(new[] { "CD/DVD", "HDD" })
            .OrderBy(p => p.BootPriorityName)
            .ToList();

        var cdDvdPriority = priorities.FirstOrDefault(p => p.BootPriorityName == "CD/DVD");
        var hddPriority = priorities.FirstOrDefault(p => p.BootPriorityName == "HDD");

        Assert.IsNotNull(cdDvdPriority, "CD/DVD boot priority not found.");
        Assert.IsNotNull(hddPriority, "HDD boot priority not found.");

        var cdDvdPriorityId = cdDvdPriority.BootPriorityId;
        var hddPriorityId = hddPriority.BootPriorityId;

        var bootPriorityData = new[]
        {
            new WindowsDeviceBootPriority { DeviceId = _deviceId, BootPriorityId = cdDvdPriorityId, BootOrder = 1 },
            new WindowsDeviceBootPriority { DeviceId = _deviceId, BootPriorityId = hddPriorityId, BootOrder = 2 }
        };

        _provider.BulkModify(bootPriorityData);

        var result = _provider.BulkGetByDeviceId(new[] { _deviceId });

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.DeviceId == _deviceId && x.BootPriorityId == cdDvdPriorityId && x.BootOrder == 1));
        Assert.IsTrue(result.Any(x => x.DeviceId == _deviceId && x.BootPriorityId == hddPriorityId && x.BootOrder == 2));
    }

    [Test]
    public void BulkInsertAndGet_WithValidPriorityNames_ReturnsCorrectData()
    {
        _deviceId = GetDeviceId(_devId);

        _deviceDataProvider.Insert(new WindowsDeviceData
        {
            DeviceId = _deviceId
        });

        var priorityNames = new[] { "USB", "HDD" };

        var result = _provider.BulkInsertAndGet(priorityNames);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.BootPriorityName == "USB"));
        Assert.IsTrue(result.Any(x => x.BootPriorityName == "HDD"));
    }

    [Test]
    public void DeleteByDeviceId_WithExistingDeviceId_DeletesData()
    {
        _deviceId = GetDeviceId(_devId);

        _deviceDataProvider.Insert(new WindowsDeviceData
        {
            DeviceId = _deviceId
        });

        var bootPriorityData = new[]
        {
            new WindowsDeviceBootPriority { DeviceId = _deviceId, BootPriorityId = 1, BootOrder = 1 }
        };

        _provider.BulkModify(bootPriorityData);

        _provider.DeleteByDeviceId(_deviceId);

        var result = _provider.GetByDeviceId(_deviceId);
        Assert.IsEmpty(result);
    }

    [Test]
    public void BulkGetByIds_ValidIds_ReturnsExpectedBootPriorities()
    {
        _deviceId = GetDeviceId(_devId);

        _deviceDataProvider.Insert(new WindowsDeviceData
        {
            DeviceId = _deviceId
        });

        var all = _provider.BulkInsertAndGet(new[] { "CD/DVD", "HDD" });

        var expected = all.OrderBy(p => p.BootPriorityName).ToList();
        var idsToQuery = expected.Select(p => (short)p.BootPriorityId);

        var result = _provider.BulkGetByIds(idsToQuery);

        Assert.AreEqual(expected.Count, result.Count);
        foreach (var exp in expected)
        {
            var actual = result.SingleOrDefault(r => r.BootPriorityId == exp.BootPriorityId);
            Assert.IsNotNull(actual);
            Assert.AreEqual(exp.BootPriorityName, actual.BootPriorityName);
        }
    }

    [Test]
    public void BulkModifyForDevice_ValidInput_UpdatesDeviceBootPriority()
    {
        _deviceId = GetDeviceId(_devId);

        _deviceDataProvider.Insert(new WindowsDeviceData
        {
            DeviceId = _deviceId
        });

        var initialData = new[]
        {
            new WindowsDeviceBootPriority { DeviceId = _deviceId, BootPriorityId = 1, BootOrder = 1 },
            new WindowsDeviceBootPriority { DeviceId = _deviceId, BootPriorityId = 2, BootOrder = 2 }
        };

        _provider.BulkModify(initialData);

        var updatedData = new[]
        {
            new WindowsDeviceBootPriority { BootPriorityId = 1, BootOrder = 2 },
            new WindowsDeviceBootPriority { BootPriorityId = 2, BootOrder = 1 }
        };

        _provider.BulkModifyForDevice(updatedData, _deviceId);

        var result = _provider.GetByDeviceId(_deviceId);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.BootPriorityId == 1 && x.BootOrder == 2));
        Assert.IsTrue(result.Any(x => x.BootPriorityId == 2 && x.BootOrder == 1));
    }

    [TestCase("DeleteByDeviceId", 0, typeof(ArgumentOutOfRangeException))]
    [TestCase("GetByDeviceId", -1, typeof(ArgumentOutOfRangeException))]
    public void ProviderMethods_InvalidIntParams_ThrowExpectedException(string methodName, int param, Type expectedException)
    {
        Assert.Throws(expectedException, () =>
        {
            switch (methodName)
            {
                case "DeleteByDeviceId":
                    _provider.DeleteByDeviceId(param);
                    break;
                case "GetByDeviceId":
                    _provider.GetByDeviceId(param);
                    break;
                default:
                    throw new NotSupportedException("Unsupported test case.");
            }
        });
    }

    [TestCase("BulkModify_Null", typeof(ArgumentNullException))]
    [TestCase("BulkModifyForDevice_NullData", typeof(ArgumentNullException))]
    [TestCase("BulkGetByDeviceId_NullList", typeof(ArgumentNullException))]
    public void ProviderMethods_NullParams_ThrowArgumentNullException(string methodName, Type expectedException)
    {
        Assert.Throws(expectedException, () =>
        {
            switch (methodName)
            {
                case "BulkModify_Null":
                    _provider.BulkModify(null);
                    break;
                case "BulkModifyForDevice_NullData":
                    _provider.BulkModifyForDevice(null, 1);
                    break;
                case "BulkGetByDeviceId_NullList":
                    _provider.BulkGetByDeviceId(null);
                    break;
                default:
                    throw new NotSupportedException("Unsupported test case.");
            }
        });
    }

    [TestCase("BulkGetByDeviceId_EmptyList")]
    [TestCase("BulkInsertAndGet_EmptyList")]
    [TestCase("BulkGetByIds_EmptyList")]
    public void ProviderMethods_EmptyListParams_ThrowArgumentException(string methodName)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            switch (methodName)
            {
                case "BulkGetByDeviceId_EmptyList":
                    _provider.BulkGetByDeviceId(new List<int>());
                    break;
                case "BulkInsertAndGet_EmptyList":
                    _provider.BulkInsertAndGet(new List<string>());
                    break;
                case "BulkGetByIds_EmptyList":
                    _provider.BulkGetByIds(new List<short>());
                    break;
                default:
                    throw new NotSupportedException("Unsupported test case.");
            }
        });
    }

    [Test]
    public void BulkModifyForDevice_InvalidDeviceId_ThrowsArgumentOutOfRangeException()
    {
        var bootPriorityData = new List<WindowsDeviceBootPriority>
    {
        new() { BootPriorityId = 1, BootOrder = 1 }
    };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _provider.BulkModifyForDevice(bootPriorityData, 0));
    }

    private int GetDeviceId(string devId)
    {
        return _testDeviceProvider.EnsureDeviceRecord(devId, DeviceId);
    }

    private void DeleteTestDevice(int deviceId)
    {
        if (deviceId > 0)
        {
            _testDeviceProvider.DeleteDevice(deviceId);
        }
    }
}
