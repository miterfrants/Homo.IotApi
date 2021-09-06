using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Homo.AuthApi;
using Homo.Core.Constants;

namespace Homo.IotApi
{
    [IotAuthorizeFactory]
    [Route("v1/devices")]
    public class DeviceController : ControllerBase
    {
        private readonly IotDbContext _dbContext;
        public DeviceController(IotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<dynamic> getList([FromQuery] int limit, [FromQuery] int page)
        {
            List<Device> records = DeviceDataservice.GetList(_dbContext, page, limit);
            return new
            {
                devices = records,
                rowNums = DeviceDataservice.GetRowNum(_dbContext)
            };
        }

        [HttpGet]
        [Route("all")]
        public ActionResult<dynamic> getAll()
        {
            return DeviceDataservice.GetAll(_dbContext);
        }

        [HttpPost]
        public ActionResult<dynamic> create([FromBody] DTOs.Device dto, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            long createdBy = extraPayload.Id;
            Device rewRecord = DeviceDataservice.Create(_dbContext, createdBy, dto);
            return rewRecord;
        }

        [HttpDelete]
        public ActionResult<dynamic> batchDelete([FromBody] List<long> ids, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            long editedBy = extraPayload.Id;
            DeviceDataservice.BatchDelete(_dbContext, editedBy, ids);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<dynamic> get([FromRoute] int id, dynamic extraPayload)
        {
            Device record = DeviceDataservice.GetOne(_dbContext, id); 
            if (record == null)
            {
                throw new CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            return record;
        }

        [HttpPatch]
        [Route("{id}")]
        public ActionResult<dynamic> update([FromRoute] int id, [FromBody] DTOs.Device dto, dynamic extraPayload)
        {
            long editedBy = extraPayload.Id;
            DeviceDataservice.Update(_dbContext, id, editedBy, dto);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [HttpDelete]
        [Route("{id}")]
        public ActionResult<dynamic> delete([FromRoute] long id, dynamic extraPayload)
        {
            long editedBy = extraPayload.Id;
            DeviceDataservice.Delete(_dbContext, id, editedBy);
            return new { status = CUSTOM_RESPONSE.OK };
        }

    }
}
