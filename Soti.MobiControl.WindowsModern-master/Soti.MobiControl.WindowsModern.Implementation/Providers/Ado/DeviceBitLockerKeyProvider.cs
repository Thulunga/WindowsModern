using System;
using System.Collections.Generic;
using System.Data;
using Soti.Data.SqlClient;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado
{
    /// <inheritdoc />
    /// <seealso cref="Soti.MobiControl.WindowsModern.Implementation.Providers.IDeviceBitLockerKeyProvider" />
    internal sealed class DeviceBitLockerKeyProvider : IDeviceBitLockerKeyProvider
    {
        private readonly IDatabase _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceBitLockerKeyProvider"/> class.
        /// </summary>
        /// <param name="database">The instance of the <see cref="IDatabase"/>.</param>
        public DeviceBitLockerKeyProvider(IDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <inheritdoc />
        public bool AreBitLockerKeysAvailable(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[WindowsDeviceBitLockerKey_CheckIfExists_ByDeviceId]"];
            command.Parameters.Add("DeviceId", deviceId);
            command.Parameters.Add("IsExist", out Ref<bool> isExists);
            command.ExecuteNonQuery();
            return isExists.Value;
        }

        public IReadOnlyDictionary<int, bool> AreBitLockerKeysAvailable(HashSet<int> deviceIds)
        {
            if (deviceIds == null)
            {
                throw new ArgumentNullException(nameof(deviceIds));
            }

            var dataTable = new DataTable("GEN_UDT_NumericIdentifier");
            dataTable.Columns.Add(new DataColumn("Id", typeof(int)) { AllowDBNull = false });
            foreach (var deviceId in deviceIds)
            {
                dataTable.Rows.Add(deviceId);
            }

            var command = _database.StoredProcedures["[dbo].[WindowsDeviceBitLockerKey_CheckIfBitLockerKeyAvailable_ByDeviceIds]"];
            command.Parameters.Add("DeviceIds", dataTable, SqlDbType.Structured);
            return command.ExecuteReader(ReadCollectionForIsBitLockerKeyAvailable);
        }

        /// <inheritdoc />
        public IEnumerable<DeviceBitLockerKeyData> GetBitLockerKeys(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[WindowsDeviceBitLockerKey_Get_ByDeviceId]"];
            command.Parameters.Add("DeviceId", deviceId);
            return command.ExecuteReader(ReadCollection);
        }

        /// <inheritdoc />
        public void UpdateBitLockerKeys(int deviceId, IEnumerable<DeviceBitLockerKeyData> bitLockerKeys)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            if (bitLockerKeys == null)
            {
                throw new ArgumentNullException(nameof(bitLockerKeys));
            }

            var dataTable = new DataTable("dbo.GEN_UDTTable_WindowsDeviceBitLockerKey");
            dataTable.Columns.AddRange(new[]
            {
                new DataColumn("DeviceId", typeof(int)) { AllowDBNull = false },
                new DataColumn("RecoveryKeyId", typeof(Guid)) { AllowDBNull = false },
                new DataColumn("DriveName", typeof(char)) { AllowDBNull = false },
                new DataColumn("RecoveryKeyContent", typeof(byte[])) { AllowDBNull = false },
                new DataColumn("DataKeyId", typeof(int)) { AllowDBNull = false }
            });

            foreach (var key in bitLockerKeys)
            {
                dataTable.Rows.Add(deviceId, key.RecoveryKeyId, key.DriveName, key.RecoveryKey, key.DataKeyId);
            }

            var command = _database.StoredProcedures["[dbo].[WindowsDeviceBitLockerKey_BulkModify]"];
            command.Parameters.Add("Records", dataTable, SqlDbType.Structured);
            command.Parameters.Add("DeviceId", deviceId);
            command.ExecuteNonQuery();
        }

        /// <inheritdoc />
        public void DeleteBitLockerKeys(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[GEN_WindowsDeviceBitLockerKey_DeleteByFk_DeviceId]"];
            command.Parameters.Add("DeviceId", deviceId);
            command.ExecuteNonQuery();
        }

        private static IEnumerable<DeviceBitLockerKeyData> ReadCollection(IDataReader reader)
        {
            var result = new List<DeviceBitLockerKeyData>();
            var mapping = GetColumnMapping(reader);
            while (reader.Read())
            {
                result.Add(ParseEntity(reader, mapping));
            }

            return result;
        }

        private static OrdinalColumnMapping GetColumnMapping(IDataRecord record)
        {
            var mapping = new OrdinalColumnMapping
            {
                DriveName = record.GetOrdinal("DriveName"),
                RecoveryKeyId = record.GetOrdinal("RecoveryKeyId"),
                RecoveryKey = record.GetOrdinal("RecoveryKeyContent"),
                DataKeyId = record.GetOrdinal("DataKeyId")
            };

            return mapping;
        }

        private static DeviceBitLockerKeyData ParseEntity(IDataRecord record, OrdinalColumnMapping mapping)
        {
            return new DeviceBitLockerKeyData
            {
                DriveName = record.GetString(mapping.DriveName),
                RecoveryKeyId = record.GetGuid(mapping.RecoveryKeyId),
                RecoveryKey = (byte[])record.GetValue(mapping.RecoveryKey),
                DataKeyId = record.GetInt32(mapping.DataKeyId)
            };
        }

        private static IReadOnlyDictionary<int, bool> ReadCollectionForIsBitLockerKeyAvailable(IDataReader reader)
        {
            var result = new Dictionary<int, bool>();
            var mapping = GetColumnMappingForIsBitLockerKeyAvailable(reader);
            while (reader.Read())
            {
                var data = ParseEntityForIsBitLockerKeyAvailable(reader, mapping);
                result[data.Key] = data.Value;
            }

            return result;
        }

        private static OrdinalColumnMapping GetColumnMappingForIsBitLockerKeyAvailable(IDataRecord record)
        {
            var mapping = new OrdinalColumnMapping
            {
                DeviceId = record.GetOrdinal("DeviceId"),
                IsBitLockerKeyAvailable = record.GetOrdinal("IsBitLockerKeyAvailable")
            };

            return mapping;
        }

        private static KeyValuePair<int, bool> ParseEntityForIsBitLockerKeyAvailable(IDataRecord record, OrdinalColumnMapping mapping)
        {
            return new KeyValuePair<int, bool>(
                record.GetInt32(mapping.DeviceId),
                record.GetBoolean(mapping.IsBitLockerKeyAvailable));
        }

        private class OrdinalColumnMapping
        {
            public int DeviceId { get; internal set; }

            public int IsBitLockerKeyAvailable { get; internal set; }

            public int DriveName { get; internal set; }

            public int RecoveryKeyId { get; internal set; }

            public int RecoveryKey { get; internal set; }

            public int DataKeyId { get; internal set; }
        }
    }
}
