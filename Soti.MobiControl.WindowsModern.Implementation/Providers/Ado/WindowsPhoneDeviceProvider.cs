using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Soti.Data.SqlClient;
using Soti.MobiControl.WindowsModern.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Providers.Ado
{
    internal sealed class WindowsPhoneDeviceProvider
    {
        private readonly IDatabase _database;

        /// <summary>
        ///  Initializes a new instance of the <see cref="WindowsPhoneDeviceProvider"/> class.
        /// </summary>
        /// <param name="database">Database interface.</param>
        public WindowsPhoneDeviceProvider(IDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public void Update(WindowsPhoneDeviceInfo deviceInfo, WindowsPhoneInfoUpdateColumns updateColumns)
        {
            if (deviceInfo == null)
            {
                throw new ArgumentNullException(nameof(deviceInfo));
            }

            var command = _database.StoredProcedures["[dbo].[WP8Device_GetCount]"];
            command.Parameters.Add("DeviceId", deviceInfo.DeviceId);
            var count = command.ExecuteScalar<int>();

            if (count <= 0)
            {
                // New Device
                InsertInternal(deviceInfo, _database);

                return;
            }

            UpdateInternal(deviceInfo, updateColumns, _database);
        }

        public void UpdateEnrollmentId(int deviceId, int enrollmentId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var info = GetByDeviceId(deviceId) ?? new WindowsPhoneDeviceInfo { DeviceId = deviceId };
            info.EnrollmentInfoId = enrollmentId;

            Update(info, WindowsPhoneInfoUpdateColumns.EnrollmentInfoId);
        }

        public int GetEnrollmentId(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[WP8Device_GetEnrollmentId]"];
            command.Parameters.Add("DeviceId", deviceId);
            var list = command.ExecuteReader(reader =>
            {
                var result = new List<int>();
                while (reader.Read())
                {
                    result.Add((int)reader.GetValue(0));
                }

                return result;
            });

            var enrollmentInfoId = list.SingleOrDefault();

            return enrollmentInfoId;
        }

        public TpmVersion GetTpmVersion(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException("deviceId");
            }

            var deviceInfo = GetByDeviceId(deviceId);
            if (string.IsNullOrWhiteSpace(deviceInfo?.TpmSpecVersion))
            {
                return null;
            }

            return new TpmVersion
            {
                TpmSpecVersion = deviceInfo.TpmSpecVersion.Trim(),
                TpmSpecLevel = deviceInfo.TpmSpecLevel?.Trim(),
                TpmSpecRevision = deviceInfo.TpmSpecRevision?.Trim()
            };
        }

        public Dictionary<int, TpmVersion> GetTpmVersions(IEnumerable<int> deviceIds)
        {
            if (deviceIds == null)
            {
                throw new ArgumentNullException(nameof(deviceIds));
            }

            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("filter", typeof(int)) { AllowDBNull = false });

            foreach (var filterEntry in deviceIds.Distinct())
            {
                var row = dataTable.NewRow();
                row["filter"] = filterEntry;

                dataTable.Rows.Add(row);
            }

            var command = _database.StoredProcedures["[dbo].[GEN_WP8Device_GetByFK_FK_WP8Device_DeviceId]"];
            command.Parameters.Add("Filter", dataTable, SqlDbType.Structured);

            var tmpVersions = command.ExecuteReader((reader) =>
            {
                var result = new Dictionary<int, TpmVersion>();

                while (reader.Read())
                {
                    var deviceId = (int?)reader.GetValue(reader.GetOrdinal("DeviceId"));

                    var tpmSpecVersion = (string)reader.GetValue(reader.GetOrdinal("TpmSpecVersion"));
                    var tpmSpecLevel = (string)reader.GetValue(reader.GetOrdinal("TpmSpecLevel"));
                    var tpmSpecRevision = (string)reader.GetValue(reader.GetOrdinal("TpmSpecRevision"));

                    if (deviceId == null || tpmSpecVersion == null)
                    {
                        continue;
                    }

                    var tpmVersion = new TpmVersion
                    {
                        TpmSpecVersion = tpmSpecVersion.Trim(),
                        TpmSpecLevel = tpmSpecLevel?.Trim(),
                        TpmSpecRevision = tpmSpecRevision?.Trim()
                    };

                    result.Add(deviceId.Value, tpmVersion);
                }

                return result;
            });

            return tmpVersions;
        }

        public bool CheckChannel(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var info = GetWnsChannelInfo(deviceId);
            if (info == null)
            {
                return false;
            }

            var status = (WnsChannelStatus)info.Item2;
            return status == WnsChannelStatus.Success && !string.IsNullOrEmpty(info.Item1);
        }

        public Tuple<string, int> GetWnsChannelInfo(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[WP8Device_GetChannelUri]"];
            command.Parameters.Add("DeviceId", deviceId);

            var wnsChannelInfo = command.ExecuteReader(reader =>
                {
                    if (reader.Read())
                    {
                        var channelUri = (string)reader.GetValue(reader.GetOrdinal("ChannelUri")) ?? string.Empty;
                        var channelStatus = (int)reader.GetValue(reader.GetOrdinal("ChannelStatus"));
                        return new Tuple<string, int>(channelUri, channelStatus);
                    }

                    return null;
                });

            return wnsChannelInfo;
        }

        public string GetSessionContextById(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[WP8Device_GetSessionContext]"];
            command.Parameters.Add("DeviceId", deviceId);

            var sessionContext = command.ExecuteReader(reader =>
            {
                if (reader.Read())
                {
                    var value = reader.GetValue(reader.GetOrdinal("SessionContext"));
                    if (value != null)
                    {
                        return (string)value;
                    }
                }

                return string.Empty;
            });

            return sessionContext;
        }

        public int GetChannelStatus(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[WP8Device_GetChannelStatus_ByDeviceId]"];
            command.Parameters.Add("DeviceId", deviceId);

            var channelStatus = command.ExecuteReader(reader =>
            {
                if (reader.Read())
                {
                    var value = reader.GetValue(reader.GetOrdinal("ChannelStatus"));
                    return (int)value;
                }

                return (int)WnsChannelStatus.NoChannelAssigned;
            });

            return channelStatus;
        }

        public void BlockChannel(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[WP8Device_UpdateChannelStatus_ByDeviceId]"];
            command.Parameters.Add("DeviceId", deviceId);
            command.Parameters.Add("ChannelStatus", (int)WnsChannelStatus.ChannelExpired);

            command.ExecuteNonQuery();
        }

        public IEnumerable<int> GetWnsListeners()
        {
            var command = _database.StoredProcedures["[dbo].[WP8Device_GetWnsListeners]"];
            var wnsListeners = command.ExecuteReader(reader =>
            {
                var result = new List<int>();
                while (reader.Read())
                {
                    result.Add((int)reader.GetValue(reader.GetOrdinal("DeviceId")));
                }
                return result;
            });

            return wnsListeners;
        }

        public WindowsPhoneDeviceInfo GetByDeviceId(int deviceId)
        {
            if (deviceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deviceId));
            }

            var command = _database.StoredProcedures["[dbo].[GEN_WP8Device_GetByIndex_DeviceId]"];
            command.Parameters.Add("DeviceId", deviceId);

            var info = command.ExecuteReader(reader =>
            {
                if (reader.Read())
                {
                    return ReadWindowsPhoneDeviceInfo(reader);
                }
                return null;
            });

            return info;
        }

        private static void UpdateInternal(WindowsPhoneDeviceInfo deviceInfo, WindowsPhoneInfoUpdateColumns updateColumns, IDatabase database)
        {
            var command = database.StoredProcedures["[dbo].[GEN_WP8Device_Update]"];

            command.Parameters.Add("Id", deviceInfo.Id);

            command.Parameters.Add("DeviceId", deviceInfo.DeviceId);
            command.Parameters.Add("DeviceIdHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.DeviceId) == 0 ? 0 : 1);

            command.Parameters.Add("ChannelUri", deviceInfo.Channel);
            command.Parameters.Add("ChannelUriHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.ChannelUri) == 0 ? 0 : 1);

            command.Parameters.Add("ChannelStatus", deviceInfo.ChannelStatus);
            command.Parameters.Add("ChannelStatusHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.ChannelStatus) == 0 ? 0 : 1);

            command.Parameters.Add("SessionContext", deviceInfo.SessionContext);
            command.Parameters.Add("SessionContextHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.SessionContext) == 0 ? 0 : 1);

            command.Parameters.Add("SessionWatermark", deviceInfo.SessionWatermark);
            command.Parameters.Add("SessionWatermarkHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.SessionWatermark) == 0 ? 0 : 1);

            command.Parameters.Add("TimeStamp", deviceInfo.Timestamp == DateTime.MinValue ? (DateTime?)null : DateTime.SpecifyKind(deviceInfo.Timestamp, DateTimeKind.Utc));
            command.Parameters.Add("TimeStampHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.Timestamp) == 0 ? 0 : 1);

            command.Parameters.Add("EnrollmentInfoId", deviceInfo.EnrollmentInfoId);
            command.Parameters.Add("EnrollmentInfoIdHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.EnrollmentInfoId) == 0 ? 0 : 1);

            command.Parameters.Add("IsRoboSupport", deviceInfo.IsRoboSupport);
            command.Parameters.Add("IsROBOSupportHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.IsRoboSupport) == 0 ? 0 : 1);

            command.Parameters.Add("RenewPeriodDays", deviceInfo.RenewPeriodDays);
            command.Parameters.Add("RenewPeriodDaysHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.RenewPeriodDays) == 0 ? 0 : 1);

            command.Parameters.Add("RetryIntervalDays", deviceInfo.RetryIntervalDays);
            command.Parameters.Add("RetryIntervalDaysHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.RetryIntervalDays) == 0 ? 0 : 1);

            command.Parameters.Add("RoboStatusId", deviceInfo.RoboStatusId);
            command.Parameters.Add("RoboStatusIdHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.RoboStatusId) == 0 ? 0 : 1);

            command.Parameters.Add("TpmSpecVersion", deviceInfo.TpmSpecVersion);
            command.Parameters.Add("TpmSpecVersionHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.TpmSpecDetails) == 0 ? 0 : 1);

            command.Parameters.Add("TpmSpecLevel", deviceInfo.TpmSpecLevel);
            command.Parameters.Add("TpmSpecLevelHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.TpmSpecDetails) == 0 ? 0 : 1);

            command.Parameters.Add("TpmSpecRevision", deviceInfo.TpmSpecRevision);
            command.Parameters.Add("TpmSpecRevisionHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.TpmSpecDetails) == 0 ? 0 : 1);

            command.Parameters.Add("IsSMode", deviceInfo.IsSMode);
            command.Parameters.Add("IsSModeHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.IsSMode) == 0 ? 0 : 1);

            command.Parameters.Add("IsUpdateApprovalRequired", deviceInfo.IsUpdateApprovalRequired);
            command.Parameters.Add("IsUpdateApprovalRequiredHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.@IsUpdateApprovalRequired) == 0 ? 0 : 1);

            command.Parameters.Add("IsManageUpdates", deviceInfo.IsManageUpdates);
            command.Parameters.Add("IsManageUpdatesHasValue", (updateColumns & WindowsPhoneInfoUpdateColumns.@IsManageUpdates) == 0 ? 0 : 1);

            command.ExecuteNonQuery();
        }

        private static void InsertInternal(WindowsPhoneDeviceInfo deviceInfo, IDatabase database)
        {
            var command = database.StoredProcedures["[dbo].[GEN_WP8Device_Insert]"];
            command.Parameters.Add("DeviceId", deviceInfo.DeviceId);
            command.Parameters.Add("ChannelStatus", deviceInfo.ChannelStatus);
            command.Parameters.Add("IsRoboSupport", deviceInfo.IsRoboSupport);
            command.Parameters.Add("RoboStatusId", deviceInfo.RoboStatusId);
            command.Parameters.Add("RenewPeriodDays", deviceInfo.RenewPeriodDays);
            command.Parameters.Add("RetryIntervalDays", deviceInfo.RetryIntervalDays);
            command.Parameters.Add("EnrollmentInfoId", deviceInfo.EnrollmentInfoId);
            command.Parameters.Add("ChannelUri", deviceInfo.Channel);
            command.Parameters.Add("SessionContext", deviceInfo.SessionContext);
            command.Parameters.Add("SessionWatermark", deviceInfo.SessionWatermark);
            command.Parameters.Add("EnrollmentId", deviceInfo.EnrollmentId);
            command.Parameters.Add("Timestamp", deviceInfo.Timestamp == DateTime.MinValue ? (DateTime?)null : DateTime.SpecifyKind(deviceInfo.Timestamp, DateTimeKind.Utc));
            command.Parameters.Add("TpmSpecVersion", deviceInfo.TpmSpecVersion);
            command.Parameters.Add("TpmSpecLevel", deviceInfo.TpmSpecLevel);
            command.Parameters.Add("TpmSpecRevision", deviceInfo.TpmSpecRevision);
            command.Parameters.Add("IsSMode", deviceInfo.IsSMode);
            command.Parameters.Add("IsUpdateApprovalRequired", deviceInfo.IsUpdateApprovalRequired);
            command.Parameters.Add("IsManageUpdates", deviceInfo.IsManageUpdates);
            command.Parameters.Add("Id", out Ref<int> id);
            command.ExecuteNonQuery();
            deviceInfo.Id = id;
        }

        private static WindowsPhoneDeviceInfo ReadWindowsPhoneDeviceInfo(IDataReader reader)
        {
            var result = new WindowsPhoneDeviceInfo();

            result.Id = (int)reader.GetValue(reader.GetOrdinal("Id"));
            result.DeviceId = (int)reader.GetValue(reader.GetOrdinal("DeviceId"));

            var sessionContext = reader.GetValue(reader.GetOrdinal("SessionContext"));
            result.SessionContext = (string)sessionContext ?? string.Empty;

            var channelUri = reader.GetValue(reader.GetOrdinal("ChannelUri"));
            result.Channel = (string)channelUri ?? string.Empty;

            result.ChannelStatus = (int)reader.GetValue(reader.GetOrdinal("ChannelStatus"));

            var sessionWatermark = reader.GetValue(reader.GetOrdinal("SessionWatermark"));
            result.SessionWatermark = (string)sessionWatermark ?? string.Empty;

            var enrollmentInfoId = reader.GetValue(reader.GetOrdinal("EnrollmentInfoId"));
            result.EnrollmentInfoId = (int?)enrollmentInfoId ?? 0;

            var enrollmentId = reader.GetValue(reader.GetOrdinal("EnrollmentId"));
            result.EnrollmentId = (string)enrollmentId ?? string.Empty;

            var timestamp = reader.GetValue(reader.GetOrdinal("Timestamp"));
            result.Timestamp = (DateTime?)timestamp ?? DateTime.Now;

            result.IsRoboSupport = (bool)reader.GetValue(reader.GetOrdinal("IsRoboSupport"));

            var renewPeriodDays = reader.GetValue(reader.GetOrdinal("RenewPeriodDays"));
            result.RenewPeriodDays = (short?)renewPeriodDays ?? (short)-1;

            var retryIntervalDays = reader.GetValue(reader.GetOrdinal("RetryIntervalDays"));
            result.RetryIntervalDays = (short?)retryIntervalDays ?? (short)-1;

            var roboStatusId = reader.GetValue(reader.GetOrdinal("RoboStatusId"));
            result.RoboStatusId = (byte?)roboStatusId ?? byte.MaxValue;

            var tpmSpecVersion = reader.GetValue(reader.GetOrdinal("TPMSpecVersion"));
            result.TpmSpecVersion = (string)tpmSpecVersion;

            var tpmSpecLevel = reader.GetValue(reader.GetOrdinal("TPMSpecLevel"));
            result.TpmSpecLevel = (string)tpmSpecLevel;

            var tpmSpecRevision = reader.GetValue(reader.GetOrdinal("TPMSpecRevision"));
            result.TpmSpecRevision = (string)tpmSpecRevision;

            var isSMode = reader.GetValue(reader.GetOrdinal("IsSMode"));
            result.IsSMode = (bool?)isSMode;

            var isUpdateApprovalRequired = reader.GetValue(reader.GetOrdinal("IsUpdateApprovalRequired"));
            result.IsUpdateApprovalRequired = (bool?)isUpdateApprovalRequired;

            var isManageUpdates = reader.GetValue(reader.GetOrdinal("IsManageUpdates"));
            result.IsManageUpdates = (bool?)isManageUpdates;

            return result;
        }
    }
}
