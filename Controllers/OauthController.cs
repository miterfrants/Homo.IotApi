using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Homo.Core.Helpers;
using Homo.Core.Constants;
using Homo.AuthApi;
using Microsoft.Extensions.Options;

namespace Homo.IotApi
{
    [Route("v1/oauth")]
    public class OauthController : ControllerBase
    {
        private readonly IotDbContext _iotDbContext;
        private readonly DBContext _dbContext;
        private readonly string _jwtKey;
        private readonly string _refreshJwtKey;
        public OauthController(IotDbContext iotDbContext, DBContext dbContext, IOptions<AppSettings> optionAppSettings)
        {
            _iotDbContext = iotDbContext;
            _dbContext = dbContext;
            AppSettings settings = optionAppSettings.Value;
            _jwtKey = settings.Secrets.JwtKey;
            _refreshJwtKey = settings.Secrets.RefreshJwtKey;
        }

        [HttpGet]
        public ActionResult<dynamic> getCode([FromQuery(Name = "redirect_url")] string redirectUrl, [FromQuery] string state)
        {
            string randomCode = CryptographicHelper.GetSpecificLengthRandomString(20, true, false);
            OauthCodeDataservice.Create(_iotDbContext, new DTOs.OauthCode() { Code = randomCode, ExpiredAt = System.DateTime.Now.AddSeconds(60) });
            Response.Redirect($"{redirectUrl}?code={randomCode}&state={state}");
            return new { code = randomCode };
        }

        [HttpPost]
        [Route("token")]
        public ActionResult<dynamic> auth([FromBody] DTOs.Oauth dto)
        {
            // make sure user authorized
            var oauthClient = OauthClientDataservice.GetOneByClientId(_iotDbContext, dto.client_id);
            if (oauthClient.HashClientSecrets != CryptographicHelper.GenerateSaltedHash(dto.client_secret, oauthClient.Salt))
            {
                throw new CustomException(ERROR_CODE.UNAUTH_ACCESS_API, System.Net.HttpStatusCode.Forbidden);
            }

            int expirationMinutes = 3 * 30 * 24 * 60;
            var expirationTime = DateTime.Now.ToUniversalTime().AddMinutes(expirationMinutes);
            Int32 unixTimestamp = (Int32)(expirationTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            User user = UserDataservice.GetOne(_dbContext, oauthClient.OwnerId);
            List<ViewRelationOfGroupAndUser> permissions = RelationOfGroupAndUserDataservice.GetRelationByUserId(_dbContext, user.Id);
            string[] roles = permissions.SelectMany(x => Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(x.Roles)).ToArray();
            var extraPayload = new Homo.AuthApi.DTOs.JwtExtraPayload()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                County = user.County,
                City = user.City,
                FacebookSub = user.FacebookSub,
                GoogleSub = user.GoogleSub,
                LineSub = user.LineSub,
                Profile = user.Profile,
                PseudonymousPhone = user.PseudonymousPhone,
                PseudonymousAddress = user.PseudonymousAddress
            };
            string accessToken = JWTHelper.GenerateToken(_jwtKey, expirationMinutes, extraPayload, roles);

            if (dto.grant_type == "authorization_code")
            {
                string refreshToken = JWTHelper.GenerateToken(_refreshJwtKey, 6 * 30 * 24 * 60, extraPayload, roles);
                return new
                {
                    token_type = "bearer",
                    access_token = accessToken,
                    refresh_token = refreshToken,
                    expires_in = unixTimestamp
                };
            }
            else if (dto.grant_type == "refresh_token")
            {
                string authorization = Request.Headers["Authorization"];
                string token = authorization == null ? "" : authorization.Substring("Bearer ".Length).Trim();
                Homo.AuthApi.DTOs.JwtExtraPayload refreshExtraPayload = JWTHelper.GetExtraPayload(_refreshJwtKey, token);
                if (refreshExtraPayload == null)
                {
                    throw new CustomException(ERROR_CODE.UNAUTH_ACCESS_API, System.Net.HttpStatusCode.Forbidden);
                }
                return new
                {
                    token_type = "bearer",
                    access_token = accessToken,
                    expires_in = unixTimestamp
                };
            }

            return null;
        }
    }
}
