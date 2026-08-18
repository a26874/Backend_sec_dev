/*
*	<copyright file="IdentityService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 12:47:20 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s.Login;
using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Backend_sec_dev.API.Filters;
using System.Security.Claims;
using Backend_sec_dev.Domain.Enums;
using Microsoft.AspNetCore.Antiforgery;

namespace Backend_sec_dev.Application.Services
{
    public class IdentityService : IIdentityService
    {
        protected readonly IJwtService jwtService;
        protected readonly IHttpContextAccessor http;
        protected readonly IDatabaseRepository databaseRepository;
        public IdentityService(IJwtService jwtService, IHttpContextAccessor httpContext, IDatabaseRepository databaseRepository)
        {
            this.jwtService = jwtService;
            this.http = httpContext;
            this.databaseRepository = databaseRepository;
        }

        #region public

        /// <summary>
        /// Logins
        /// </summary>
        /// <param name="userCredentialsDto"></param>
        /// <returns></returns>
        public async Task<ApiResponse<LoginResult>> Login(UserCredentialsDto userCredentialsDto)
        {

            if (userCredentialsDto == null || NullChecks.StringNullOrEmpty(userCredentialsDto.email) || NullChecks.StringNullOrEmpty(userCredentialsDto.password))
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.Conflict, "Email or password not provided.");

            try
            {
                User? u = await this.databaseRepository.FirstOrDefaultAsync<User>(t => t.Email == userCredentialsDto.email);
                if (u != null)
                {
                    if (u.IsLocked)
                    {
                        return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, "Your account is locked. To Unlock please recover your password");
                    }
                    return await ComparePassword(userCredentialsDto, u);

                }
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, "Login failed");

            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.InternalServerError, SqlHelpers.SqlExceptionError(ex));
            }

        }

        /// <summary>
        /// Refresh the token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public async Task<ApiResponse<RefreshTokenResult>> RefreshToken(string refreshToken)
        {
            if (NullChecks.StringNullOrEmpty(refreshToken))
                return ApiResponse<RefreshTokenResult>.Fail(HttpStatusCode.Conflict, "Refresh token not provided");


            byte[] hashedRefreshToken = Hasher.HashToken(refreshToken);

            AuthSession? auth = await GetAuthSession(hashedRefreshToken);
            string jwt = string.Empty;
            string newRefreshToken = Hasher.GenerateToken(Domain.Enums.TokenType.JwtToken);

            try
            {
                if (auth != null)
                {
                    User? u = await this.databaseRepository.FirstOrDefaultAsync<User>(t => t.Id == auth.UserId);
                    if (u != null)
                    {
                        if (IsAuthSessionValid(auth))
                        {
                            return await InvalidateAuthSession(u);
                        }
                        auth.IsRevoked = true;

                        this.databaseRepository.Update(auth);
                        jwt = jwtService.GenerateJwtToken(u);

                        AuthSession newAuth = this.CreateAuthSession(u.Id, newRefreshToken);
                        this.databaseRepository.Add(newAuth);

                        this.CreateCookiesAux(jwt, refreshToken, newAuth.SessionId);

                    }
                }
                await this.databaseRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                return ApiResponse<RefreshTokenResult>.Fail(HttpStatusCode.InternalServerError, SqlHelpers.SqlExceptionError(ex));

            }

            return ApiResponse<RefreshTokenResult>.Ok(new RefreshTokenResult { jwtToken = jwt, refreshToken = newRefreshToken }, HttpStatusCode.OK);
        }

        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="userCredentials"></param>
        /// <returns></returns>
        public async Task<ApiResponse<LoginResult>> Logout(UserCredentialsDto userCredentials)
        {
            ApiResponse<LoginResult> res = new ApiResponse<LoginResult>();
            User? u = await this.databaseRepository.FirstOrDefaultAsync<User>(t => t.Email == userCredentials.email);
            if (u != null)
            {
                List<AuthSession> authSessions = await this.databaseRepository.Where<AuthSession>(t => t.UserId == u.Id && !t.IsRevoked);

                if (!NullChecks.ListNullOrEmpty(authSessions))
                {
                    foreach (AuthSession auth in authSessions)
                    {
                        auth.IsRevoked = true;
                    }
                    this.databaseRepository.UpdateRange(authSessions);
                }
                try
                {
                    await this.databaseRepository.SaveChanges();
                    res = new ApiResponse<LoginResult> { Success = true, statusCode = HttpStatusCode.OK, Message = "Logout efetuado com sucesso" };
                }
                catch (Exception ex)
                {
                    return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, SqlHelpers.SqlExceptionError(ex));
                }
            }

            return res;
        }

        //fix later
        public async Task<AuthSession?> GetAuthSession(byte[] hashedRefreshToken)
        {
            return await this.databaseRepository.FirstOrDefaultAsync<AuthSession>(t => t.Hashed_Token.SequenceEqual(hashedRefreshToken));
        }
        /// <summary>
        /// Validates authsession
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public bool IsAuthSessionValid(AuthSession auth)
        {
            return !auth.IsRevoked && auth.ExpiresAt > DateTime.UtcNow;
        }

        /// <summary>
        /// Validates authsession
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public bool IsAuthSessionTokenValid(AuthSession auth, byte[] hashedToken)
        {
            return Hasher.CompareHashes(hashedToken, auth.Hashed_Token);
        }

        /// <summary>
        /// Validates a token
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public bool isJwtTokenValid(string jwtToken)
        {
            if (NullChecks.StringNullOrEmpty(jwtToken)) return false;
            return jwtService.validateToken(jwtToken);
        }

        public void DecodeJwtAndPopulateUser(string token)
        {
            JwtSecurityToken jwt = DecodeJwtToken(token);
            Claim userId = jwt.Claims.FirstOrDefault(t => t.Type == CustomClaimTypes.Id)!;
            Claim userRole = jwt.Claims.FirstOrDefault(t => t.Type == CustomClaimTypes.Role)!;
            if (userId == null)
            {
                return;
            }
            List<Claim> claims =
            [
                new Claim(ClaimTypes.NameIdentifier, userId.Value),
                new Claim(ClaimTypes.Role, userRole.Value),
            ];

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "jwt");
            ClaimsPrincipal principal = new ClaimsPrincipal(claimsIdentity);

            http.HttpContext!.User = principal;
        }




        public JwtSecurityToken DecodeJwtToken(string jwtToken)
        {
            return this.jwtService.DecodeJwt(jwtToken);
        }
        #endregion

        #region Login logic

        private void CreateCookiesAux(string jwt, string refreshToken, Guid sessionId)
        {
            string sessionIdGuid = sessionId.ToString();
            Dictionary<string, string> keyCookies = new Dictionary<string, string>();
            keyCookies.Add("jwtToken", jwt);
            keyCookies.Add("refreshToken", refreshToken);
            keyCookies.Add("Session-Id", sessionIdGuid);
            CookieOptions co = new CookieOptions();
            co.HttpOnly = true;
            co.SameSite = SameSiteMode.Strict;
            co.Expires = DateTime.UtcNow.AddMinutes(5);
            co.Secure = true;
            if (http?.HttpContext != null)
            {
                IAntiforgery antiForgery = http.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

                AntiforgeryTokenSet tokens = antiForgery.GetAndStoreTokens(http.HttpContext);
                if (http?.HttpContext.Response != null && http?.HttpContext.Response.Headers != null)
                {
                    http.HttpContext.Response.Headers.Append("X-CSRF-TOKEN", tokens.RequestToken);
                }
                foreach (KeyValuePair<string, string> cookie in keyCookies)
                {
                    CreateCookies(cookie.Key, cookie.Value, co);
                }
            }
        }


        /// <summary>
        /// Invalidate the authsessions
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        private async Task<ApiResponse<RefreshTokenResult>> InvalidateAuthSession(User u)
        {
            List<AuthSession> sessionsToRevoke = new List<AuthSession>();
            sessionsToRevoke = await this.databaseRepository.Where<AuthSession>(t => t.UserId == u.Id);
            foreach (AuthSession session in sessionsToRevoke)
            {
                session.IsRevoked = true;
            }
            this.databaseRepository.UpdateRange(sessionsToRevoke);

            await this.databaseRepository.SaveChanges();
            return ApiResponse<RefreshTokenResult>.Fail(HttpStatusCode.Forbidden, "Token has been already used");
        }
        /// <summary>
        /// Compares the password
        /// </summary>
        /// <param name="userCredentialsDto"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        private async Task<ApiResponse<LoginResult>> ComparePassword(UserCredentialsDto userCredentialsDto, User u)
        {
            PasswordVerificationResult result = Hasher.ComparePassword(userCredentialsDto, u.PasswordHash);
            switch (result)
            {
                case PasswordVerificationResult.Failed:
                    return await FailedLogin(u);

                case PasswordVerificationResult.Success:
                case PasswordVerificationResult.SuccessRehashNeeded:
                    return await SuccessfullLogin(u, result);

                default:
                    return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, "Login failed");
            }
        }

        /// <summary>
        /// The user has already logged in.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task<ApiResponse<LoginResult>> SuccessfullLogin(User u, PasswordVerificationResult result)
        {
            ApiResponse<LoginResult> res = new ApiResponse<LoginResult>();

            res = TreatResultPassword(result, res);
            try
            {
                string ipAddress = HttpContextHelper.GetClientIpAddress(http.HttpContext!);
                Guid sessionId = HttpContextHelper.GetClientSessionId(http.HttpContext!);
                u.LastLoginTime = DateTime.UtcNow;
                this.databaseRepository.Update(u);


                AuthSession? userAuthSession = await this.databaseRepository.FirstOrDefaultAsync<AuthSession>(t => t.UserId == u.Id && t.IpAddress == ipAddress && t.SessionId == sessionId && t.IsRevoked == false);


                string jwtToken = this.jwtService.GenerateJwtToken(u);
                res!.Data!.jwt = jwtToken;
                string refreshToken = this.GenerateJwtTokenAux(u, res);

                userAuthSession = this.AuthSessionLogic(userAuthSession, u, refreshToken, res);

                DecodeJwtAndPopulateUser(jwtToken);
                CreateCookiesAux(jwtToken, refreshToken, userAuthSession!.SessionId);

                await this.databaseRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.InternalServerError, SqlHelpers.SqlExceptionError(ex));
            }

            return res;

        }

        /// <summary>
        /// Writes de result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        private ApiResponse<LoginResult> TreatResultPassword(PasswordVerificationResult result, ApiResponse<LoginResult> res)
        {
            switch (result)
            {
                case PasswordVerificationResult.Success:
                    res = ApiResponse<LoginResult>.Ok(new LoginResult { message = "Login with success" }, HttpStatusCode.OK);
                    break;
                case PasswordVerificationResult.SuccessRehashNeeded:
                    res = ApiResponse<LoginResult>.Ok(new LoginResult { message = "Login with success, but you should redefine your password" }, HttpStatusCode.OK);
                    break;
                default:
                    return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, "Login failed");
            }
            return res;
        }

        /// <summary>
        /// Both create a new session object based on conditions
        /// </summary>
        /// <param name="userAuthSession"></param>
        /// <param name="u"></param>
        /// <param name="refreshToken"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        private AuthSession? AuthSessionLogic(AuthSession? userAuthSession, User u, string refreshToken, ApiResponse<LoginResult> res)
        {
            if (userAuthSession == null)
            {
                userAuthSession = this.CreateAuthSession(u.Id, refreshToken);
                this.databaseRepository.Add(userAuthSession);
                res!.Data!.refreshToken = refreshToken;
                return userAuthSession;
            }
            else if (userAuthSession != null && IsAuthSessionValid(userAuthSession))
            {
                userAuthSession.IsRevoked = true;
                this.databaseRepository.Update(userAuthSession);
                AuthSession newSession = CreateAuthSession(u.Id, refreshToken);
                this.databaseRepository.Add(newSession);
                res!.Data!.refreshToken = refreshToken;
                return newSession;
            }
            return null;
        }

        /// <summary>
        /// Generates jwt tokens.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="res"></param>
        /// <returns></returns>

        private string GenerateJwtTokenAux(User u, ApiResponse<LoginResult> res)
        {
            string refreshToken = Hasher.GenerateToken(TokenType.JwtToken);

            return refreshToken;
        }



        /// <summary>
        /// Creates cookies
        /// </summary>
        /// <param name="cookieString"></param>
        /// <param name="cookieValue"></param>
        /// <param name="co"></param>
        private void CreateCookies(string cookieString, string cookieValue, CookieOptions co)
        {
            if (http != null && http.HttpContext != null)
            {
                http.HttpContext.Response.Cookies.Append($"{cookieString}", cookieValue, co);
            }
        }

        /// <summary>
        /// Create a authsession object
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        private AuthSession CreateAuthSession(Guid userId, string refreshToken)
        {
            string ipAddress = HttpContextHelper.GetClientIpAddress(http.HttpContext!);
            string userAgent = HttpContextHelper.GetUserAgent(http.HttpContext!);
            AuthSession authSession = new AuthSession();
            authSession.UserId = userId;
            authSession.IpAddress = ipAddress;
            authSession.UserAgent = userAgent;
            byte[] data = Hasher.HashToken(refreshToken);

            authSession.Hashed_Token = data;
            return authSession;
        }

        /// <summary>
        /// The login failed.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        private async Task<ApiResponse<LoginResult>> FailedLogin(User u)
        {
            if (u.FailedLoginAttempts + 1 >= User.MAX_ATTEMPTS_BEFORE_LOCK)
            {
                u.IsLocked = true;
                u.FailedLoginAttempts = 0;
            }
            else
            {
                u.FailedLoginAttempts += 1;
            }
            try
            {
                this.databaseRepository.Update(u);
                await this.databaseRepository.SaveChanges();
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, string.Format("Login failed, remaining tries before account blocking: {0}", User.MAX_ATTEMPTS_BEFORE_LOCK - u.FailedLoginAttempts));
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, SqlHelpers.SqlExceptionError(ex));
            }
        }


        #endregion
    }
}