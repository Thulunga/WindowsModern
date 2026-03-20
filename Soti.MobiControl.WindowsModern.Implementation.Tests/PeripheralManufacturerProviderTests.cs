using NUnit.Framework;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using System;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
[IntegrationTest]
internal sealed class PeripheralManufacturerProviderTests : ProviderTestsBase
{
    private PeripheralManufacturerProvider _manufacturerProvider;

    [SetUp]
    public void Setup()
    {
        _manufacturerProvider = new PeripheralManufacturerProvider(Database);
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _ = new PeripheralManufacturerProvider(null));
    }

    [TestCase(null)]
    [TestCase(" ")]
    public void GetPeripheralManufacturerByManufacturerCode_ArgumentException(string manufacturerCode)
    {
        Assert.Throws<ArgumentException>(() =>
            _manufacturerProvider.GetPeripheralManufacturerByManufacturerCode(manufacturerCode));
    }

    [Test]
    public void GetPeripheralManufacturerByManufacturerCode_Success()
    {
        var result = _manufacturerProvider.GetPeripheralManufacturerByManufacturerCode("047F");
        Assert.IsNotNull(result);
    }

    [Test]
    public void GetAllPeripheralManufacturerData_Success()
    {
        var result = _manufacturerProvider.GetAllPeripheralManufacturerData();
        Assert.IsNotNull(result);
    }

    [Test]
    public void InsertPeripheralManufacturerData_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _manufacturerProvider.InsertPeripheralManufacturerData(null));
    }

    [TestCase(" ", "17EF")]
    [TestCase(null, "17EF")]
    [TestCase("Dell", " ")]
    [TestCase("Dell", null)]
    public void InsertPeripheralManufacturerData_ArgumentException(string name, string code)
    {
        var data = new PeripheralManufacturerData
        {
            ManufacturerName = name,
            ManufacturerCode = code
        };
        Assert.Throws<ArgumentException>(() =>
            _manufacturerProvider.InsertPeripheralManufacturerData(data));
    }
}
