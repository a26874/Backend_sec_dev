/*
*	<copyright file="IdentityController">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 12:45:59 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s.Login;
using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Backend_sec_dev.Application.Interfaces;

namespace Backend_sec_dev.API.Controllers
{
    public class IdentityController : ApiControllerBase
    {
        private IIdentityService identityService;
        protected readonly IHttpContextAccessor httpContext;

        public IdentityController(IIdentityService identityService, IHttpContextAccessor httpContext)
        {
            this.identityService = identityService;
            this.httpContext = httpContext;
        }

        [Route("login")]
        [HttpPost]
        public async Task<ApiResponse<LoginResult>> Login(UserCredentialsDto userCredentialsDto)
        {
            ApiResponse<LoginResult> res = await this.identityService.Login(userCredentialsDto, this.httpContext.HttpContext!.Connection.RemoteIpAddress!.ToString());
            return res;
        }

        [Route("refresh_token")]
        [HttpPost]
        public async Task<ApiResponse<RefreshTokenResult>> RefreshToken(RefreshTokenRequest r)
        {
            ApiResponse<RefreshTokenResult> res = await this.identityService.RefreshToken(r.refreshToken);
            return res;
        }
    }
}