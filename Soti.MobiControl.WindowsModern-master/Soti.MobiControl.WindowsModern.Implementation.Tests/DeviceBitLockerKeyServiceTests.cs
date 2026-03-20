using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Soti.Diagnostics;
using Soti.MobiControl.DeploymentServerExtensions.Contracts;
using Soti.MobiControl.Messaging;
using Soti.MobiControl.WindowsModern.Implementation.Models;
using Soti.MobiControl.WindowsModern.Implementation.Providers;
using Soti.SensitiveDataProtection;
using Soti.SensitiveDataProtection.Model;

namespace Soti.MobiControl.WindowsModern.Implementation.Tests;

[TestFixture]
public class DeviceBitLockerKeyServiceTests
{
    private const string LogName = "WindowsBitLocker";
    private const int DeviceId = 1000001;

    private Mock<IProgramTrace> _programTraceMock;
    private Mock<IDataKeyService> _dataKeyServiceMock;
    private Mock<ISensitiveDataEncryptionService> _sensitiveDataEncryptionServiceMock;
    private Mock<IDeviceBitLockerKeyProvider> _deviceBitLockerKeyProviderMock;
    private Mock<IMessagePublisher> _messagePublisherMock;
    private Mock<IDeviceAdministrationCallback> _deviceAdministrationCallbackMock;
    private IDeviceBitLockerKeyService _deviceBitLockerKeyService;

    [SetUp]
    public void SetUp()
    {
        _programTraceMock = new Mock<IProgramTrace>(MockBehavior.Strict);
        _dataKeyServiceMock = new Mock<IDataKeyService>(MockBehavior.Strict);
        _sensitiveDataEncryptionServiceMock = new Mock<ISensitiveDataEncryptionService>(MockBehavior.Strict);
        _deviceBitLockerKeyProviderMock = new Mock<IDeviceBitLockerKeyProvider>(MockBehavior.Strict);
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _deviceAdministrationCallbackMock = new Mock<IDeviceAdministrationCallback>();

        _deviceBitLockerKeyService = new DeviceBitLockerKeyService(
            _programTraceMock.Object,
            _dataKeyServiceMock.Object,
            _sensitiveDataEncryptionServiceMock.Object,
            _deviceBitLockerKeyProviderMock.Object,
            new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object));
    }

    [Test]
    public void ConstructorTests()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceBitLockerKeyService(null, _dataKeyServiceMock.Object, _sensitiveDataEncryptionServiceMock.Object,
            _deviceBitLockerKeyProviderMock.Object, new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceBitLockerKeyService(_programTraceMock.Object, null, _sensitiveDataEncryptionServiceMock.Object,
            _deviceBitLockerKeyProviderMock.Object, new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceBitLockerKeyService(_programTraceMock.Object, _dataKeyServiceMock.Object, null,
            _deviceBitLockerKeyProviderMock.Object, new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceBitLockerKeyService(_programTraceMock.Object, _dataKeyServiceMock.Object, _sensitiveDataEncryptionServiceMock.Object,
            null, new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceBitLockerKeyService(_programTraceMock.Object, _dataKeyServiceMock.Object, _sensitiveDataEncryptionServiceMock.Object,
            _deviceBitLockerKeyProviderMock.Object, null,
            new Lazy<IDeviceAdministrationCallback>(() => _deviceAdministrationCallbackMock.Object)));
        Assert.Throws<ArgumentNullException>(() => _ = new DeviceBitLockerKeyService(_programTraceMock.Object, _dataKeyServiceMock.Object, _sensitiveDataEncryptionServiceMock.Object,
            _deviceBitLockerKeyProviderMock.Object, new Lazy<IMessagePublisher>(() => _messagePublisherMock.Object),
            null));
    }

    [Test]
    public void AreBitLockerKeysAvailableTests()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyService.AreBitLockerKeysAvailable(0));

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.AreBitLockerKeysAvailable(DeviceId)).Returns(true);

        var result = _deviceBitLockerKeyService.AreBitLockerKeysAvailable(DeviceId);
        var resultFromCache = _deviceBitLockerKeyService.AreBitLockerKeysAvailable(DeviceId);

        Assert.That(result, Is.True);
        Assert.AreEqual(result, resultFromCache);

        _deviceBitLockerKeyProviderMock.Verify(provider => provider.AreBitLockerKeysAvailable(DeviceId), Times.Once);

        _deviceBitLockerKeyService.InvalidateBitLockerCache(DeviceId, false);
        _deviceBitLockerKeyProviderMock.Invocations.Clear();

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.AreBitLockerKeysAvailable(DeviceId)).Returns(false);

        result = _deviceBitLockerKeyService.AreBitLockerKeysAvailable(DeviceId);

        Assert.That(result, Is.False);

        _deviceBitLockerKeyProviderMock.Verify(provider => provider.AreBitLockerKeysAvailable(DeviceId), Times.Once);

        _deviceBitLockerKeyService.InvalidateBitLockerCache(DeviceId, false);
    }

    [Test]
    public void AreBitLockerKeysAvailable_BulkDevices_Tests()
    {
        Assert.Throws<ArgumentNullException>(() => _deviceBitLockerKeyService.AreBitLockerKeysAvailable(null));

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.AreBitLockerKeysAvailable(new HashSet<int>()))
            .Returns(new Dictionary<int, bool>());

        var result = _deviceBitLockerKeyService.AreBitLockerKeysAvailable(new HashSet<int>());

        Assert.IsNotNull(result);
        Assert.IsTrue(!result.Any());

        var data = new Dictionary<int, bool> { { 100001, true }, { 100002, false } };

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.AreBitLockerKeysAvailable(new HashSet<int> { 100001, 100002 })).Returns(data);

        result = _deviceBitLockerKeyService.AreBitLockerKeysAvailable(new HashSet<int> { 100001, 100002 });

        Assert.IsNotNull(result);
        Assert.AreEqual(result.Count(), 2);
    }

    [Test]
    public void GetBitLockerKeysTests()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyService.GetBitLockerKeys(0));

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.GetBitLockerKeys(DeviceId)).Returns(Enumerable.Empty<DeviceBitLockerKeyData>());

        var result = _deviceBitLockerKeyService.GetBitLockerKeys(DeviceId);

        _deviceBitLockerKeyProviderMock.Verify(provider => provider.GetBitLockerKeys(DeviceId), Times.Once);
        _dataKeyServiceMock.Verify(provider => provider.GetKey(It.IsAny<int>()), Times.Never);
        _sensitiveDataEncryptionServiceMock.Verify(provider => provider.Decrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>()), Times.Never);

        Assert.That(result, Is.Empty);

        _deviceBitLockerKeyProviderMock.Invocations.Clear();

        var keyData = new DeviceBitLockerKeyData
        {
            DriveName = "C",
            RecoveryKeyId = Guid.NewGuid(),
            RecoveryKey = Encoding.UTF8.GetBytes("testKey"),
            DataKeyId = 1
        };

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.GetBitLockerKeys(DeviceId)).Returns(new List<DeviceBitLockerKeyData> { keyData });

        var dataKey = GetDataKey(keyData.DataKeyId);

        _dataKeyServiceMock.Setup(service => service.GetKey(keyData.DataKeyId)).Returns(dataKey);

        _sensitiveDataEncryptionServiceMock.Setup(service => service.Decrypt(keyData.RecoveryKey, dataKey))
            .Returns(keyData.RecoveryKey);

        result = _deviceBitLockerKeyService.GetBitLockerKeys(DeviceId).ToArray();

        _deviceBitLockerKeyProviderMock.Verify(provider => provider.GetBitLockerKeys(DeviceId), Times.Once);
        _dataKeyServiceMock.Verify(provider => provider.GetKey(It.IsAny<int>()), Times.Once);
        _sensitiveDataEncryptionServiceMock.Verify(provider => provider.Decrypt(It.IsAny<byte[]>(), It.IsAny<DataKey>()), Times.Once);

        Assert.That(result, Is.Not.Null);
        Assert.IsTrue(result.Count().Equals(1));
        Assert.IsTrue(result.First().DriveName.Equals(keyData.DriveName));
        Assert.IsTrue(result.First().RecoveryKeyId.Equals(keyData.RecoveryKeyId));
        Assert.IsTrue(result.First().RecoveryKey.Equals(Encoding.UTF8.GetString(keyData.RecoveryKey)));
    }

    [TestCase("")]
    [TestCase("ENCRYPTED_DRIVES_RESET")]
    public void ProcessBitLockerKeys_NullData_Tests(string bitLockerKeysData)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyService.ProcessBitLockerKeysData(0, bitLockerKeysData));

        var logMsg = $"BitLocker key(s) information deleted for {DeviceId}";

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.DeleteBitLockerKeys(DeviceId));
        _programTraceMock.Setup(trace =>
            trace.Write(TraceLevel.Verbose, LogName, logMsg));

        _deviceBitLockerKeyService.ProcessBitLockerKeysData(DeviceId, bitLockerKeysData);

        _deviceBitLockerKeyProviderMock.Verify(provider => provider.DeleteBitLockerKeys(DeviceId), Times.Once);
        _programTraceMock.Verify(trace => trace.Write(TraceLevel.Verbose, LogName, logMsg), Times.Once);

        _deviceBitLockerKeyProviderMock.Invocations.Clear();
    }

    [Test]
    public void ProcessBitLockerKeys_InvalidData_Tests()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyService.ProcessBitLockerKeysData(0, string.Empty));

        var logMsg = $"Unable to process BitLocker key information for {DeviceId} as the input string was not in the proper format.";

        _programTraceMock.Setup(trace => trace.Write(TraceLevel.Error, LogName, logMsg));

        _deviceBitLockerKeyService.ProcessBitLockerKeysData(DeviceId, "testInvalidString");

        _programTraceMock.Verify(trace => trace.Write(TraceLevel.Error, LogName, logMsg), Times.Once);

        _programTraceMock.Invocations.Clear();

        var errorMsg = $"Unable to process BitLocker key information for {DeviceId} as the input string was not in the proper format.";

        const string inputString = "DriveLetter:C:,KeyProtectorID:{12341234-1234-1234-1234-7099-3361-34},NumericalPassword:135652-039402-398794-547756-167629-716947-111012-392832";

        _programTraceMock.Setup(trace => trace.Write(TraceLevel.Error, LogName, errorMsg));
        _programTraceMock.Setup(trace => trace.Write(TraceLevel.Error, LogName, It.IsAny<string>()));

        _deviceBitLockerKeyService.ProcessBitLockerKeysData(DeviceId, inputString);

        _programTraceMock.Verify(trace => trace.Write(TraceLevel.Error, LogName, errorMsg), Times.Once);
    }

    [TestCase("C", "{6C0ABADA-DFDC-46CF-9ED8-70997336B7B8}", "135652-039402-398794-547756-167629-716947-111012-392832")]
    [TestCase("D", "{6C0ABADA-DFDC-46CF-9ED8-70997336B7B9}", "")]
    public void ProcessBitLockerKeys_WithoutUpdate_Tests(string driveName, string recoveryKeyId, string recoveryKey)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyService.ProcessBitLockerKeysData(0, string.Empty));

        var bitLockerKey = new DeviceBitLockerKeyData
        {
            DriveName = driveName,
            RecoveryKeyId = Guid.Parse(recoveryKeyId),
            RecoveryKey = Encoding.UTF8.GetBytes(recoveryKey)
        };

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.GetBitLockerKeys(DeviceId)).Returns(new List<DeviceBitLockerKeyData> { bitLockerKey });

        var inputString = $"DriveLetter:{bitLockerKey.DriveName}:,KeyProtectorID:{recoveryKeyId},NumericalPassword:{recoveryKey}";

        _deviceBitLockerKeyService.ProcessBitLockerKeysData(DeviceId, inputString);

        _deviceBitLockerKeyProviderMock.Verify(provider => provider.GetBitLockerKeys(DeviceId), Times.Once);
        _deviceBitLockerKeyProviderMock.Verify(provider => provider.UpdateBitLockerKeys(DeviceId, It.IsAny<IEnumerable<DeviceBitLockerKeyData>>()), Times.Never);
    }

    [TestCase("C", "{6C0ABADA-DFDC-46CF-9ED8-70997336B7B8}", "")]
    [TestCase("D", "{6C0ABADA-DFDC-46CF-9ED8-70997336B7B9}", "135652-039402-398794-547756-167629-716947-111012-392832")]
    public void ProcessBitLockerKeys_WithUpdate_Tests(string driveName, string recoveryKeyId, string recoveryKey)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyService.ProcessBitLockerKeysData(0, string.Empty));

        var bitLockerKey = new DeviceBitLockerKeyData
        {
            DriveName = driveName,
            RecoveryKeyId = Guid.NewGuid(),
            RecoveryKey = Encoding.UTF8.GetBytes(recoveryKey)
        };

        var savedBitLockerKey = new DeviceBitLockerKeyData
        {
            DriveName = "E",
            RecoveryKeyId = Guid.Parse(recoveryKeyId),
            RecoveryKey = Encoding.UTF8.GetBytes(recoveryKey)
        };

        var inputString = $"DriveLetter:{bitLockerKey.DriveName}:,KeyProtectorID:{recoveryKeyId},NumericalPassword:{recoveryKey}";

        var dataKey = GetDataKey(1);
        var logMsg = $"BitLocker key(s) information updated for {DeviceId}";
        _deviceBitLockerKeyProviderMock.Setup(provider => provider.GetBitLockerKeys(DeviceId)).Returns(new List<DeviceBitLockerKeyData> { savedBitLockerKey });
        _dataKeyServiceMock.Setup(service => service.GetKey()).Returns(dataKey);
        _dataKeyServiceMock.Setup(service => service.GetKey(0)).Returns(dataKey);
        _sensitiveDataEncryptionServiceMock.Setup(service => service.Encrypt(bitLockerKey.RecoveryKey, dataKey)).Returns(bitLockerKey.RecoveryKey);
        _sensitiveDataEncryptionServiceMock.Setup(service => service.Decrypt(bitLockerKey.RecoveryKey, dataKey)).Returns(savedBitLockerKey.RecoveryKey);
        _deviceBitLockerKeyProviderMock.Setup(provider => provider.UpdateBitLockerKeys(DeviceId, It.IsAny<IEnumerable<DeviceBitLockerKeyData>>()));
        _programTraceMock.Setup(trace => trace.Write(TraceLevel.Verbose, LogName, logMsg));

        _deviceBitLockerKeyService.ProcessBitLockerKeysData(DeviceId, inputString);

        _deviceBitLockerKeyProviderMock.Verify(provider => provider.GetBitLockerKeys(DeviceId), Times.Once);
        _deviceBitLockerKeyProviderMock.Verify(provider => provider.UpdateBitLockerKeys(DeviceId, It.IsAny<IEnumerable<DeviceBitLockerKeyData>>()), Times.Once);
        _programTraceMock.Verify(trace => trace.Write(TraceLevel.Verbose, LogName, logMsg), Times.Once);
    }

    [Test]
    public void DeleteBitLockerKeysDataTests()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _deviceBitLockerKeyService.DeleteBitLockerKeysData(0));
        var logMsg = $"BitLocker key(s) information deleted for {DeviceId}";
        _programTraceMock.Setup(trace =>
            trace.Write(TraceLevel.Verbose, LogName, logMsg));

        _deviceBitLockerKeyProviderMock.Setup(provider => provider.DeleteBitLockerKeys(DeviceId));

        Assert.DoesNotThrow(() => _deviceBitLockerKeyService.DeleteBitLockerKeysData(DeviceId));

        _deviceBitLockerKeyProviderMock.Verify(provider => provider.DeleteBitLockerKeys(DeviceId), Times.Once);

        _programTraceMock.Verify(trace => trace.Write(TraceLevel.Verbose, LogName, logMsg), Times.Once);
    }

    [Test]
    public void InvalidateBitLockerCacheTest()
    {
        _messagePublisherMock.Setup(x => x.Publish(It.IsAny<InvalidateBitLockerCacheMessage>(), ApplicableServer.Dse, ApplicableServer.Ms));

        _deviceBitLockerKeyService.InvalidateBitLockerCache(DeviceId, true);

        _messagePublisherMock.Verify(x => x.Publish(It.IsAny<InvalidateBitLockerCacheMessage>(), ApplicableServer.Dse, ApplicableServer.Ms), Times.Once);
    }

    private static DataKey GetDataKey(int dataKeyId)
    {
        return new DataKey
        {
            Id = dataKeyId,
            KeyData = Encoding.UTF8.GetBytes("1234"),
            DataKeyAlgorithm = DataKeyAlgorithm.Aes256,
            CreationTime = DateTime.MinValue,
            MasterKeyId = null
        };
    }
}
