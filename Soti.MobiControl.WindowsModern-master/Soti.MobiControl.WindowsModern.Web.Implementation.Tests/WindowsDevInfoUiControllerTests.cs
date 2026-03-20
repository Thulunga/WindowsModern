using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Moq;
using Soti.MobiControl.Devices.DevInfo;

namespace Soti.MobiControl.WindowsModern.Web.Implementation.Tests;

[TestFixture]
internal sealed class WindowsDevInfoUiControllerTests
{
    private Mock<IDevInfoService> _devInfoService;
    private IWindowsDevInfoUiController _controller;

    [SetUp]
    public void Setup()
    {
        _devInfoService = new Mock<IDevInfoService>();

        _controller = new WindowsDevInfoUiController(_devInfoService.Object);
    }

    [Test]
    public void GetModelsByManufacturer_Throws_Validation_Exception()
    {
        Assert.Throws<ValidationException>(() => _controller.GetModelsByManufacturer(null));
    }

    [Test]
    public void GetModelsByManufacturer_Throws_ArgumentOutofRange_Exception()
    {
        var manufacturer = "Samsung";
        Assert.Throws<ValidationException>(() => _controller.GetModelsByManufacturer(manufacturer));
    }

    [Test]
    public void GetModelsByManufacturer_Success()
    {
        var manufacturer = (Manufacturer)1;
        _devInfoService.Setup(a => a.GetModelsByManufacturer(manufacturer)).Returns(new List<string> { "Acer" });

        var model = _controller.GetModelsByManufacturer("Acer");
        Assert.IsNotNull(model);

        _devInfoService.Verify(q => q.GetModelsByManufacturer(manufacturer), Times.Once);
    }
}
