using System.Collections.Generic;
using System.Linq;
using Soti.MobiControl.Events;
using Soti.MobiControl.Events.EventOriginators;
using Soti.MobiControl.Events.ModelInterfaces;
using Soti.MobiControl.WindowsModern.Implementation.Models;

namespace Soti.MobiControl.WindowsModern.Implementation.Events
{
    [EventMetadata("LocalUserRenamedFromDeviceEvent", "Local user renamed from device", "Local user username changed from {0}.", AlertManagerType.Device)]

    internal sealed class LocalUserRenamedFromDeviceEvent : DeviceOriginatedEvent, IDeviceBoundEvent, IParameterizedEvent
    {
        internal LocalUserRenamedFromDeviceEvent(int deviceId, string devId, IEnumerable<WindowsDeviceLocalUserRenameData> renameData)
            : base(deviceId, devId, EventSeverity.Information)
        {
            var renamedString = string.Join(", ", renameData.Select(data => $"\"{data.OldUserName}\" to \"{data.NewUserName}\""));
            EventAdditionalParameters = new[] { renamedString };
        }

        public string[] EventAdditionalParameters { get; }
    }
}
