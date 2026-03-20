using Moq;
using NUnit.Framework;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Models;
using System;
using System.Collections.Generic;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests
{
    [TestFixture]
    public class WindowsDeviceBootPriorityServiceTests
    {
        private Mock<IWindowsDeviceBootPriorityDataProvider> _providerMock;
        private WindowsDeviceBootPriorityService _service;

        [SetUp]
        public void SetUp()
        {
            _providerMock = new Mock<IWindowsDeviceBootPriorityDataProvider>();
            _service = new WindowsDeviceBootPriorityService(_providerMock.Object);
        }

        [Test]
        public void DeleteByDeviceId_DelegatesToProvider()
        {
            var deviceId = 42;
            _service.DeleteByDeviceId(deviceId);
            _providerMock.Verify(p => p.DeleteByDeviceId(deviceId), Times.Once);
        }

        [Test]
        public void GetByDeviceId_DelegatesToProvider_ReturnsResult()
        {
            var deviceId = 42;
            var expected = new List<WindowsDeviceBootPriority>();
            _providerMock.Setup(p => p.GetByDeviceId(deviceId)).Returns(expected);

            var result = _service.GetByDeviceId(deviceId);

            Assert.AreEqual(expected, result);
            _providerMock.Verify(p => p.GetByDeviceId(deviceId), Times.Once);
        }

        [Test]
        public void BulkModify_DelegatesToProvider()
        {
            var data = new List<WindowsDeviceBootPriority> { new WindowsDeviceBootPriority() };
            _service.BulkModify(data);
            _providerMock.Verify(p => p.BulkModify(It.IsAny<List<WindowsDeviceBootPriority>>()), Times.Once);
        }

        [Test]
        public void BulkModifyForDevice_DelegatesToProvider()
        {
            var data = new List<WindowsDeviceBootPriority> { new WindowsDeviceBootPriority() };
            var deviceId = 101;
            _service.BulkModifyForDevice(data, deviceId);
            _providerMock.Verify(p => p.BulkModifyForDevice(It.IsAny<List<WindowsDeviceBootPriority>>(), deviceId), Times.Once);
        }

        [Test]
        public void BulkGetByDeviceId_DelegatesToProvider_ReturnsResult()
        {
            var ids = new List<int> { 1, 2 };
            var expected = new List<WindowsDeviceBootPriority>();
            _providerMock.Setup(p => p.BulkGetByDeviceId(ids)).Returns(expected);

            var result = _service.BulkGetByDeviceId(ids);

            Assert.AreEqual(expected, result);
            _providerMock.Verify(p => p.BulkGetByDeviceId(ids), Times.Once);
        }

        [Test]
        public void BulkInsertAndGet_DelegatesToProvider_ReturnsResult()
        {
            var names = new List<string> { "PXE", "USB" };
            var expected = new List<WindowsBootPriority>();
            _providerMock.Setup(p => p.BulkInsertAndGet(names)).Returns(expected);

            var result = _service.BulkInsertAndGet(names);

            Assert.AreEqual(expected, result);
            _providerMock.Verify(p => p.BulkInsertAndGet(names), Times.Once);
        }

        [Test]
        public void BulkGetByIds_DelegatesToProvider_ReturnsResult()
        {
            var ids = new List<short> { 1, 2 };
            var expected = new List<WindowsBootPriority>();
            _providerMock.Setup(p => p.BulkGetByIds(ids)).Returns(expected);

            var result = _service.BulkGetByIds(ids);

            Assert.AreEqual(expected, result);
            _providerMock.Verify(p => p.BulkGetByIds(ids), Times.Once);
        }

        [TestCase("BulkModify")]
        [TestCase("BulkModifyForDevice")]
        [TestCase("BulkGetByDeviceId")]
        [TestCase("BulkInsertAndGet")]
        [TestCase("BulkGetByIds")]
        public void Methods_NullParameters_ThrowArgumentNullException(string methodName)
        {
            switch (methodName)
            {
                case "BulkModify":
                    Assert.Throws<ArgumentNullException>(() => _service.BulkModify(null));
                    break;
                case "BulkModifyForDevice":
                    Assert.Throws<ArgumentNullException>(() => _service.BulkModifyForDevice(null, 1));
                    break;
                case "BulkGetByDeviceId":
                    Assert.Throws<ArgumentNullException>(() => _service.BulkGetByDeviceId(null));
                    break;
                case "BulkInsertAndGet":
                    Assert.Throws<ArgumentNullException>(() => _service.BulkInsertAndGet(null));
                    break;
                case "BulkGetByIds":
                    Assert.Throws<ArgumentNullException>(() => _service.BulkGetByIds(null));
                    break;
            }
        }

        [TestCase("DeleteByDeviceId")]
        [TestCase("GetByDeviceId")]
        [TestCase("BulkModifyForDevice")]
        public void Methods_InvalidDeviceId_ThrowArgumentOutOfRangeException(string methodName)
        {
            var validData = new List<WindowsDeviceBootPriority> { new WindowsDeviceBootPriority() };

            switch (methodName)
            {
                case "DeleteByDeviceId":
                    Assert.Throws<ArgumentOutOfRangeException>(() => _service.DeleteByDeviceId(0));
                    break;
                case "GetByDeviceId":
                    Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetByDeviceId(-5));
                    break;
                case "BulkModifyForDevice":
                    Assert.Throws<ArgumentOutOfRangeException>(() => _service.BulkModifyForDevice(validData, 0));
                    break;
            }
        }

        [TestCase("BulkModify")]
        [TestCase("BulkModifyForDevice")]
        [TestCase("BulkGetByDeviceId")]
        [TestCase("BulkInsertAndGet")]
        [TestCase("BulkGetByIds")]
        public void Methods_EmptyList_ThrowArgumentException(string methodName)
        {
            switch (methodName)
            {
                case "BulkModify":
                    Assert.Throws<ArgumentException>(() => _service.BulkModify(new List<WindowsDeviceBootPriority>()));
                    break;
                case "BulkModifyForDevice":
                    Assert.Throws<ArgumentException>(() => _service.BulkModifyForDevice(new List<WindowsDeviceBootPriority>(), 1));
                    break;
                case "BulkGetByDeviceId":
                    Assert.Throws<ArgumentException>(() => _service.BulkGetByDeviceId(new List<int>()));
                    break;
                case "BulkInsertAndGet":
                    Assert.Throws<ArgumentException>(() => _service.BulkInsertAndGet(new List<string>()));
                    break;
                case "BulkGetByIds":
                    Assert.Throws<ArgumentException>(() => _service.BulkGetByIds(new List<short>()));
                    break;
            }
        }
    }
}
