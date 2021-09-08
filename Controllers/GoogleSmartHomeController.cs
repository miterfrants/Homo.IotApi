using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Homo.AuthApi;
using Homo.Core.Constants;

namespace Homo.IotApi
{
    [IotAuthorizeFactory]
    [Route("v1/google-smart-home")]
    public class GoogleSmartHomeController : ControllerBase
    {
        private readonly IotDbContext _dbContext;
        public GoogleSmartHomeController(IotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public ActionResult<dynamic> fulfillment([FromBody] DTOs.GoogleSmartHome dto, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            long ownerId = extraPayload.Id;
            List<Device> devices = DeviceDataservice.GetAll(_dbContext, ownerId);
            return new
            {
                RequestId = dto.RequestId,
                Payload = new
                {
                    AgentUserId = ownerId,
                    Devices = devices
                }
            };
        }

        private ActionResult<dynamic> onSync([FromBody] DTOs.GoogleSmartHome dto, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            return new { };
        }
    }
}
