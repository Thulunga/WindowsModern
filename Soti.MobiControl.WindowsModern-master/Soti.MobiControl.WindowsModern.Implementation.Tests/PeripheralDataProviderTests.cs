using NUnit.Framework;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using System;
using System.Linq;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
internal sealed class PeripheralDataProviderTests : ProviderTestsBase
{
    private PeripheralDataProvider _peripheralProvider;

    [SetUp]
    public void Setup()
    {
        _peripheralProvider = new PeripheralDataProvider(Database);
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new PeripheralDataProvider(null));
    }

    [Test]
    public void InsertPeripheralData_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _peripheralProvider.InsertPeripheralData(null));
    }

    [TestCase(" ")]
    [TestCase(null)]
    public void InsertPeripheralData_ArgumentException(string name)
    {
        var data = new PeripheralData
        {
            Name = name,
            ManufacturerId = 1
        };
        Assert.Throws<ArgumentException>(() =>
            _peripheralProvider.InsertPeripheralData(data));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void InsertPeripheralData_ArgumentOutOfRangeException(short manufacturerId)
    {
        var data = new PeripheralData
        {
            Name = "name",
            ManufacturerId = manufacturerId
        };
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _peripheralProvider.InsertPeripheralData(data));
    }

    [Test]
    public void GetPeripheralData_Throw_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _peripheralProvider.GetPeripheralData(0));
    }

    [Test]
    public void GetPeripheralData_Success()
    {
        var peripheralData = GetPeripheralData("peripheral7", 2);

        var peripheralId = _peripheralProvider.InsertPeripheralData(peripheralData);

        var result = _peripheralProvider.GetPeripheralData(peripheralId);
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Name, peripheralData.Name);
        Assert.AreEqual(result.ManufacturerId, peripheralData.ManufacturerId);

        // Tear down
        DeletePeripheralTestData(peripheralId);
    }

    [Test]
    public void GetAllPeripheralData_Success()
    {
        var peripheralData1 = GetPeripheralData("peripheral7", 2);
        var peripheralData2 = GetPeripheralData("peripheral5", 3);

        var peripheralId1 = _peripheralProvider.InsertPeripheralData(peripheralData1);
        var peripheralId2 = _peripheralProvider.InsertPeripheralData(peripheralData2);

        var result = _peripheralProvider.GetAllPeripheralData();

        Assert.AreEqual(2, result.Count());

        // Tear down
        DeletePeripheralTestData(peripheralId1);
        DeletePeripheralTestData(peripheralId2);
    }

    [Test]
    public void UpdatePeripheralType_ArgumentException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _peripheralProvider.UpdatePeripheralType(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _peripheralProvider.UpdatePeripheralType(1, -1));
    }

    [Test]
    public void UpdatePeripheralType_Success()
    {
        var peripheralData1 = GetPeripheralData("peripheral7", 2);
        var peripheralId1 = _peripheralProvider.InsertPeripheralData(peripheralData1);

        _peripheralProvider.UpdatePeripheralType(peripheralId1, 2);

        // Tear down
        DeletePeripheralTestData(peripheralId1);
    }

    [Test]
    public void BulkDeletePeripheral_Throw_ArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _peripheralProvider.BulkDeletePeripheralData(0));
    }

    private void DeletePeripheralTestData(int peripheralId)
    {
        _peripheralProvider.BulkDeletePeripheralData(peripheralId);
    }

    private static PeripheralData GetPeripheralData(string peripheralName, short manufacturerId)
    {
        return new PeripheralData
        {
            Name = peripheralName,
            ManufacturerId = manufacturerId,
            PeripheralTypeId = 1
        };
    }
}
