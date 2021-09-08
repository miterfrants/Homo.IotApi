using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Homo.AuthApi;
using Homo.Core.Constants;

namespace Homo.IotApi
{
    [IotAuthorizeFactory]
    [Route("v1/me/zones")]
    public class MyZoneController : ControllerBase
    {
        private readonly IotDbContext _dbContext;
        public MyZoneController(IotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<dynamic> getList([FromQuery] int limit, [FromQuery] int page)
        {
            List<Zone> records = ZoneDataservice.GetList(_dbContext, page, limit);
            return new
            {
                zones = records,
                rowNums = ZoneDataservice.GetRowNum(_dbContext)
            };
        }

        [HttpGet]
        [Route("all")]
        public ActionResult<dynamic> getAll()
        {
            return ZoneDataservice.GetAll(_dbContext);
        }

        [HttpPost]
        public ActionResult<dynamic> create([FromBody] DTOs.Zone dto, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            long createdBy = extraPayload.Id;
            Zone rewRecord = ZoneDataservice.Create(_dbContext, createdBy, dto);
            return rewRecord;
        }

        [HttpDelete]
        public ActionResult<dynamic> batchDelete([FromBody] List<long> ids, Homo.AuthApi.DTOs.JwtExtraPayload extraPayload)
        {
            long editedBy = extraPayload.Id;
            ZoneDataservice.BatchDelete(_dbContext, editedBy, ids);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult<dynamic> get([FromRoute] int id, dynamic extraPayload)
        {
            Zone record = ZoneDataservice.GetOne(_dbContext, id);
            if (record == null)
            {
                throw new CustomException(ERROR_CODE.DATA_NOT_FOUND, System.Net.HttpStatusCode.NotFound);
            }
            return record;
        }

        [HttpPatch]
        [Route("{id}")]
        public ActionResult<dynamic> update([FromRoute] int id, [FromBody] DTOs.Zone dto, dynamic extraPayload)
        {
            long editedBy = extraPayload.Id;
            ZoneDataservice.Update(_dbContext, id, editedBy, dto);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [HttpDelete]
        [Route("{id}")]
        public ActionResult<dynamic> delete([FromRoute] long id, dynamic extraPayload)
        {
            long editedBy = extraPayload.Id;
            ZoneDataservice.Delete(_dbContext, id, editedBy);
            return new { status = CUSTOM_RESPONSE.OK };
        }

    }
}
