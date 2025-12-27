/*
*	<copyright file="AuthenticationMiddleware">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/21/2025 7:30:34 PM</date>
*	<description></description>
**/
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Shared.Helpers;


namespace Backend_sec_dev.API.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate next;
        public AuthenticationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        #region public
        public async Task InvokeAsync(HttpContext context)
        {
            bool isAuthRequest = IsHttpContextAuthentication(context);
            if (isAuthRequest)
            {
                await next(context);
                return;
            }

            IIdentityService identityService = GetIdentityScope(context);

            bool isJwtValid = IsJwtTokenValid(context, identityService);

            bool isRefresthTokenValid = await IsRefreshTokenValid(context, identityService);

            if (isJwtValid && isRefresthTokenValid)
            {
                identityService.DecodeJwtAndPopulateUser(GetJwtTokenFromCookies(context));
                await next(context);
            }
            return;
        }
        #endregion

        #region private

        /// <summary>
        /// Decodes the jwt, gets the claims that are needed (this case just userid and role) and attaches to user
        /// </summary>
        /// <param name="identityService"></param>
        /// <param name="token"></param>
        /// <param name="context"></param>

        private IIdentityService GetIdentityScope(HttpContext context)
        {
            IServiceScope scopes = context.RequestServices.CreateScope();
            return scopes.ServiceProvider.GetService<IIdentityService>()!;
        }

        private bool IsHttpContextAuthentication(HttpContext context)
        {
            string trimmedPath = context.Request.Path.Value!.Split("api/v1/")[1];

            bool isLogin = trimmedPath.StartsWith("identity/login");
            bool isLogout = trimmedPath.StartsWith("identity/logout");
            bool isRefreshToken = trimmedPath.StartsWith("identity/refreshToken");
            return isLogin || isLogout || isRefreshToken;
        }

        private bool IsJwtTokenValid(HttpContext context, IIdentityService identityService)
        {
            string jwtToken = GetJwtTokenFromCookies(context);
            return identityService.isJwtTokenValid(jwtToken);
        }

        private string GetJwtTokenFromCookies(HttpContext context)
        {
            return context.Request.Cookies["jwtToken"]!;
        }

        private async Task<bool> IsRefreshTokenValid(HttpContext context, IIdentityService identityService)
        {
            string refreshToken = context.Request.Cookies["refreshToken"]!;
            if (NullChecks.StringNullOrEmpty(refreshToken)) return false;

            
            byte[] Hashed = identityService.HashRefreshToken(refreshToken);
            AuthSession? auth = await identityService.GetAuthSession(Hashed);
            
            
            if (NullChecks.ObjectNullOrEmpty<AuthSession>(auth!)) return false;


            return identityService.IsAuthSessionValid(auth!) && identityService.IsAuthSessionTokenValid(auth!, refreshToken);
        }
        #endregion

    }
}