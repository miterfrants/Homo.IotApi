using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Homo.IotApi
{
    public class DeviceStateDataservice
    {

        private static List<DeviceState> _deviceStates = new List<DeviceState>();
        public static DeviceState GetOne(long id)
        {
            return _deviceStates
                .Where(x =>
                    x.DeviceId == id
                )
                .FirstOrDefault();
        }
        public static List<DeviceState> GetAll(long? ownerId, List<long> deviceIds)
        {
            return _deviceStates
                .Where(x =>
                    (ownerId == null || x.OwnerId == ownerId) &&
                    (deviceIds == null || deviceIds.Contains(x.DeviceId))
                )
                .ToList<DeviceState>();
        }

        public static DeviceState Create(long ownerId, long deviceId)
        {
            var deviceState = new DeviceState()
            {
                DeviceId = deviceId,
                OwnerId = ownerId,
                Status = "SUCCESS",
                Online = true,
                On = false
            };
            _deviceStates.Add(deviceState);
            return deviceState;
        }

        public static void Update(long ownerId, long deviceId, dynamic deviceParams)
        {
            var targetDeviceState = _deviceStates.Find(x => x.DeviceId == deviceId && x.OwnerId == ownerId);
            if (targetDeviceState == null)
            {
                targetDeviceState = DeviceStateDataservice.Create(ownerId, deviceId);
            }
            var executeParams = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(deviceParams.ToString());
            foreach (string key in executeParams.Keys)
            {
                var property = targetDeviceState.GetType().GetProperty(key.Substring(0, 1).ToUpper() + key.Substring(1));
                if (property == null)
                {
                    continue;
                }
                property.SetValue(targetDeviceState, executeParams[key]);
            }
        }

    }

    public class DeviceState
    {
        public long OwnerId { get; set; }
        public long DeviceId { get; set; }
        public string Status { get; set; }
        public bool Online { get; set; }
        public bool On { get; set; }
    }


    public class LampDeviceState : DeviceState
    {
        public int Brightness { get; set; }
        public LampColor Color { get; set; }
    }

    public class LampColor
    {
        public int TemperatureK { get; set; }
    }
}
