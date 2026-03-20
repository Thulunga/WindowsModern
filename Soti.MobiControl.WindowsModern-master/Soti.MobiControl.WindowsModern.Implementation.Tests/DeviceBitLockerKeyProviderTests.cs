using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Soti.MobiControl.Test.Common.Database;
using Soti.MobiControl.Test.Common.SensitiveDataProtection;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.MobiControl.WindowsModern.Implementation.Providers.Ado;
using Soti.SensitiveDataProtection;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests
{
    [TestFixture]
    [IntegrationTest]
    public class DeviceBitLockerKeyProviderTests : ProviderTestsBase
    {
        private static readonly TestDeviceProvider TestDeviceProvider = new TestDeviceProvider();
        private readonly string _devId1 = Guid.NewGuid().ToString();
        private readonly string _devId2 = Guid.NewGuid().ToString();

        private DeviceBitLockerKeyProvider _deviceBitLockerKeyProvider;
        private IDataKeyService _dataKeyService;
        private int _deviceId1;
        private int _deviceId2;

        [SetUp]
        public void Setup()
        {
            _dataKeyService = new TestDataKeyService(Database);
            _deviceBitLockerKeyProvider = new DeviceBitLockerKeyProvider(Database);
        }

        [TearDown]
        public void Teardown()
        {
            DeleteTestData(_deviceId1);
            DeleteTestData(_deviceId2);
        }

        [Test]
        public void ConstructorTests()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new DeviceBitLockerKeyProvider(null));
        }

        [Test]
        public void AreBitLockerKeysAvailableTests()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyProvider.AreBitLockerKeysAvailable(0));

            _deviceId1 = GetDeviceId(_devId1);
            var result = _deviceBitLockerKeyProvider.AreBitLockerKeysAvailable(_deviceId1);
            Assert.That(result.Equals(false));

            PreOperationSetUp(_deviceId1, "C", Guid.NewGuid(), Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            result = _deviceBitLockerKeyProvider.AreBitLockerKeysAvailable(_deviceId1);
            Assert.That(result.Equals(true));
        }

        [Test]
        public void AreBitLockerKeysAvailable_BulkDevicesTests()
        {
            Assert.Throws<ArgumentNullException>(() => _deviceBitLockerKeyProvider.AreBitLockerKeysAvailable(null));

            var deviceIds = new HashSet<int>();

            _deviceId1 = GetDeviceId(_devId1);
            PreOperationSetUp(_deviceId1, "C", Guid.NewGuid(), Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

            deviceIds.Add(_deviceId1);
            var result = _deviceBitLockerKeyProvider.AreBitLockerKeysAvailable(deviceIds).ToArray();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length == 1);
            Assert.IsTrue(result.Single(x => x.Key == _deviceId1).Value);

            _deviceId2 = GetDeviceId(_devId2);
            deviceIds.Add(_deviceId2);
            result = _deviceBitLockerKeyProvider.AreBitLockerKeysAvailable(deviceIds).ToArray();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length == 2);
            Assert.IsTrue(result.First(x => x.Key == _deviceId1).Value);
            Assert.IsFalse(result.First(x => x.Key == _deviceId2).Value);
        }

        [Test]
        public void GetBitLockerKeysTests()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyProvider.GetBitLockerKeys(0));

            _deviceId1 = GetDeviceId(_devId1);
            var result = _deviceBitLockerKeyProvider.GetBitLockerKeys(_deviceId1);
            Assert.IsTrue(!result.Any());
        }

        [Test]
        public void UpdateBitLockerKeysTests()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyProvider.UpdateBitLockerKeys(0, null));
            Assert.Throws<ArgumentNullException>(() => _deviceBitLockerKeyProvider.UpdateBitLockerKeys(12345, null));

            _deviceId1 = GetDeviceId(_devId1);
            const string driveName = "C";
            var recoveryKeyId = Guid.NewGuid();
            var recoveryKey = Guid.NewGuid().ToString();
            PreOperationSetUp(_deviceId1, driveName, recoveryKeyId, Encoding.UTF8.GetBytes(recoveryKey));

            var result = _deviceBitLockerKeyProvider.GetBitLockerKeys(_deviceId1).FirstOrDefault();
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(result.DataKeyId, GenerateDataKey());
            Assert.AreEqual(result.DriveName, driveName);
            Assert.AreEqual(result.RecoveryKeyId, recoveryKeyId);
            Assert.AreEqual(Encoding.UTF8.GetString(result.RecoveryKey), recoveryKey);
        }

        [Test]
        public void DeleteBitLockerKeysTests()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyProvider.DeleteBitLockerKeys(0));

            _deviceId1 = GetDeviceId(_devId1);
            PreOperationSetUp(_deviceId1, "C", Guid.NewGuid(), Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            var result = _deviceBitLockerKeyProvider.GetBitLockerKeys(_deviceId1);
            Assert.AreEqual(result.Count(), 1);

            _deviceBitLockerKeyProvider.DeleteBitLockerKeys(_deviceId1);

            result = _deviceBitLockerKeyProvider.GetBitLockerKeys(_deviceId1);
            Assert.AreEqual(result.Count(), 0);
        }

        private static int GetDeviceId(string devId)
        {
            return TestDeviceProvider.EnsureDeviceRecord(devId, 1004);
        }

        private void PreOperationSetUp(int deviceId, string driveName, Guid recoveryKeyId, byte[] recoveryKey)
        {
            var deviceBitLockerKeyData = new DeviceBitLockerKeyData
            {
                DataKeyId = GenerateDataKey(),
                DriveName = driveName,
                RecoveryKey = recoveryKey,
                RecoveryKeyId = recoveryKeyId
            };
            _deviceBitLockerKeyProvider.UpdateBitLockerKeys(deviceId, new[] { deviceBitLockerKeyData });
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

            _deviceBitLockerKeyProvider.DeleteBitLockerKeys(deviceId);
            TestDeviceProvider.DeleteDevice(deviceId);
        }
    }
}
