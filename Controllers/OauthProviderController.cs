using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Homo.AuthApi;
using Homo.Core.Constants;

namespace Homo.IotApi
{
    [Route("v1/oauth")]
    public class OauthProviderController : ControllerBase
    {
        private readonly IotDbContext _dbContext;
        public OauthProviderController(IotDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
