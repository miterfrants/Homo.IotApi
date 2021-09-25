using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Homo.Api;

namespace Homo.IotApi
{
    [IotAuthorizeFactory]
    [Route("v1/google-smart-home")]
    [Validate]
    public class GoogleSmartHomeController : ControllerBase
    {
        private static List<DeviceState> DeviceStates = new List<DeviceState>();
        private readonly IotDbContext _dbContext;
        public GoogleSmartHomeController(IotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public ActionResult<dynamic> fulfillment([FromBody] DTOs.GoogleSmartHome dto, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            if (dto.Inputs[0].Intent == GOOGLE_SMART_HOME_INTENT.DEVICES_SYNC)
            {
                return onSync(dto, extraPayload);
            }
            else if (dto.Inputs[0].Intent == GOOGLE_SMART_HOME_INTENT.DEVICES_QUERY)
            {
                return onQuery(dto, extraPayload);
            }
            else if (dto.Inputs[0].Intent == GOOGLE_SMART_HOME_INTENT.DEVICES_EXECUTE)
            {
                return onExecute(dto, extraPayload);
            }
            return null;
        }

        private ActionResult<dynamic> onSync([FromBody] DTOs.GoogleSmartHome dto, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            List<Device> devices = DeviceDataservice.GetAll(_dbContext, ownerId);
            return new
            {
                RequestId = dto.RequestId,
                Payload = new
                {
                    AgentUserId = ownerId,
                    Devices = devices.Select(x => Newtonsoft.Json.JsonConvert.DeserializeObject<DeviceInfo>(x.Info)).ToList<DeviceInfo>()
                }
            };
        }

        private ActionResult<dynamic> onQuery([FromBody] DTOs.GoogleSmartHome dto, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            List<Device> devices = DeviceDataservice.GetAll(_dbContext, ownerId);
            List<long> myDeviceIds = devices.Select(x => x.Id).ToList<long>();
            List<DeviceState> myDeviceStates = DeviceStateDataservice.GetAll(ownerId, myDeviceIds);
            List<long> myDevicesIdsFromMemoryDeviceStates = myDeviceStates.Select(x => x.DeviceId).ToList<long>();
            // create device state to memory
            devices.Where(x => !myDevicesIdsFromMemoryDeviceStates.Contains(x.Id)).ToList().ForEach(device =>
            {
                DeviceStateDataservice.Create(ownerId, device.Id);
            });
            return new
            {
                RequestId = dto.RequestId,
                Payload = new
                {
                    Devices = devices.Select((x) =>
                    {
                        var deviceState = myDeviceStates.Where(y => y.DeviceId == x.Id).FirstOrDefault();
                        return new
                        {
                            DeviceId = deviceState.DeviceId.ToString(),
                            On = deviceState.On,
                            Status = deviceState.Status,
                            Online = deviceState.Online,
                        };
                    }).ToDictionary(x => x.DeviceId)
                }
            };
        }

        private ActionResult<dynamic> onExecute([FromBody] DTOs.GoogleSmartHome dto, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            List<DeviceCommand> commands = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DeviceCommand>>(dto.Inputs[0].Payload.GetProperty("commands").ToString());
            List<dynamic> commandResult = new List<dynamic>();
            commands.ForEach(command =>
            {
                for (int i = 0; i < command.Devices.Count; i++)
                {
                    DeviceStateDataservice.Update(ownerId, command.Devices[i].Id, command.Execution[i].Params);
                }
            });
            List<long> myDeviceIds = commands[0].Devices.Select(x => x.Id).ToList<long>();
            List<DeviceState> states = DeviceStateDataservice.GetAll(ownerId, myDeviceIds);
            return new
            {
                RequestId = dto.RequestId,
                Payload = new
                {
                    Commands = new List<dynamic>()
                    {
                        new {
                            Ids = myDeviceIds.Select(x=> x.ToString()).ToList<string>(),
                            Status = "SUCCESS",
                            States = states.Select(x=>new {
                                DeviceId = x.DeviceId.ToString(),
                                On = x.On,
                                Status = x.Status,
                                Online = x.Online,
                            }).ToList()
                        }
                    }

                }
            };
        }
    }

    public class DeviceCommand
    {
        public List<ExecuteDevice> Devices { get; set; }
        public List<DeviceExecution> Execution { get; set; }
    }

    public class ExecuteDevice
    {
        public long Id { get; set; }

    }

    public class DeviceExecution
    {
        public string Command { get; set; }
        public dynamic Params { get; set; }
    }

    public class DeviceInfo
    {
        public string Id { get; set; }
        public DeviceName Name { get; set; }
        public List<string> Traits { get; set; }
        public string Type { get; set; }
        public bool WillReportState { get; set; }
    }

    public class DeviceName
    {
        public List<string> DefaultNames { get; set; }
        public List<string> Nicknames { get; set; }
        public string Name { get; set; }
    }

}
