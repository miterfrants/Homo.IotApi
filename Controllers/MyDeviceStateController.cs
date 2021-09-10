using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Homo.Api;
using Homo.Core.Constants;

namespace Homo.IotApi
{
    [IotAuthorizeFactory]
    [Route("v1/me/devices/{id}/state")]
    [Validate]
    public class MyDeviceStateController : ControllerBase
    {
        private readonly IotDbContext _dbContext;
        public MyDeviceStateController(IotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<dynamic> getState([FromRoute] long id, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            return DeviceStateDataservice.GetOne(id);
        }

    }
}
