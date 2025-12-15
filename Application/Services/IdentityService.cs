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
using Backend_sec_dev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Cryptography;

namespace Backend_sec_dev.Application.Services
{
    public class IdentityService : IIdentityService
    {
        protected readonly AppDbContext db;
        protected readonly IJwtService jwtService;
        protected readonly IHttpContextAccessor httpContext;
        public IdentityService(AppDbContext db, IJwtService jwtService, IHttpContextAccessor httpContext)
        {
            this.db = db;
            this.jwtService = jwtService;
            this.httpContext = httpContext;
        }

        #region public

        public async Task<ApiResponse<LoginResult>> Login(UserCredentialsDto userCredentialsDto)
        {
            if (userCredentialsDto == null || NullChecks.StringNullOrEmpty(userCredentialsDto.email) || NullChecks.StringNullOrEmpty(userCredentialsDto.password))
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.Conflict, "Email or password not provided.");

            try
            {
                User? u = await this.db.Users.FirstOrDefaultAsync(t => t.Email == userCredentialsDto.email);
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

        public async Task<ApiResponse<RefreshTokenResult>> RefreshToken(string refreshToken)
        {
            if (NullChecks.StringNullOrEmpty(refreshToken))
                return ApiResponse<RefreshTokenResult>.Fail(HttpStatusCode.Conflict, "Refresh token not provided");


            byte[] hashedRefreshToken = HashRefreshToken(refreshToken);

            AuthSession? auth = await this.db.AuthSessions.FirstOrDefaultAsync(t => t.Hashed_Token == hashedRefreshToken);
            string jwt = string.Empty;
            string newRefreshToken = Hasher.GenerateRefreshToken();

            try
            {
                if (auth != null)
                {
                    User? u = await this.db.Users.FirstOrDefaultAsync(t => t.Id == auth.UserId);
                    if (u != null)
                    {
                        if (auth.IsRevoked)
                        {
                            List<AuthSession> sessionsToRevoke = new List<AuthSession>();
                            sessionsToRevoke = await this.db.AuthSessions.Where(t => t.UserId == u.Id).ToListAsync();
                            foreach (AuthSession session in sessionsToRevoke)
                            {
                                session.IsRevoked = true;
                            }
                            this.db.AuthSessions.UpdateRange(sessionsToRevoke);

                            await this.db.SaveChangesAsync();
                            return ApiResponse<RefreshTokenResult>.Fail(HttpStatusCode.Forbidden, "Token has been already used");
                        }
                        auth.IsRevoked = true;

                        this.db.AuthSessions.Update(auth);
                        jwt = jwtService.GenerateJwtToken(u);

                        AuthSession newAuth = this.CreateAuthSession(u.Id, newRefreshToken);
                        this.db.AuthSessions.Add(newAuth);

                        CreateCookies(jwt, newRefreshToken);

                    }
                }
                await this.db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ApiResponse<RefreshTokenResult>.Fail(HttpStatusCode.InternalServerError, SqlHelpers.SqlExceptionError(ex));

            }

            return ApiResponse<RefreshTokenResult>.Ok(new RefreshTokenResult { jwtToken = jwt, refreshToken = newRefreshToken }, HttpStatusCode.OK);
        }

        public async Task<ApiResponse<LoginResult>> Logout(UserCredentialsDto userCredentials)
        {
            ApiResponse<LoginResult> res = new ApiResponse<LoginResult>();
            User? u = await this.db.Users.FirstOrDefaultAsync(t => t.Email == userCredentials.email);
            if (u != null)
            {
                List<AuthSession> authSessions = await this.db.AuthSessions.Where(t => t.UserId == u.Id && !t.IsRevoked ).ToListAsync();

                if (!NullChecks.ListNullOrEmpty(authSessions))
                {
                    foreach (AuthSession auth in authSessions)
                    {
                        auth.IsRevoked = true;
                    }
                    this.db.AuthSessions.UpdateRange(authSessions);
                }
                try
                {
                    await this.db.SaveChangesAsync();
                    res = new ApiResponse<LoginResult> { statusCode = HttpStatusCode.OK, Message = "Logout efetuado com sucesso" };
                }
                catch (Exception ex)
                {
                    return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, SqlHelpers.SqlExceptionError(ex));
                }
            }

            return res;
        }
        #endregion

        #region Login logic
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

        private async Task<ApiResponse<LoginResult>> SuccessfullLogin(User u, PasswordVerificationResult result)
        {
            ApiResponse<LoginResult> res = new ApiResponse<LoginResult>();

            res = TreatResultPassword(result, res);
            try
            {
                string ipAddress = this.GetIpAddress();
                u.LastLoginTime = DateTime.UtcNow;
                this.db.Update(u);
                AuthSession? userAuthSession = await this.db.AuthSessions.FirstOrDefaultAsync(t => t.UserId == u.Id && t.IpAddress == ipAddress && t.IsRevoked == false);

                string refreshToken = this.GenerateJwtTokenAux(u, res);

                this.AuthSessionLogic(userAuthSession, u, refreshToken, res);

                await this.db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.InternalServerError, SqlHelpers.SqlExceptionError(ex));
            }

            return res;

        }

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

        private void AuthSessionLogic(AuthSession? userAuthSession, User u, string refreshToken, ApiResponse<LoginResult> res)
        {
            if (userAuthSession == null)
            {
                userAuthSession = this.CreateAuthSession(u.Id, refreshToken);
                this.db.AuthSessions.Add(userAuthSession);
                res!.Data!.refreshToken = refreshToken;
            }
            else if (userAuthSession != null && userAuthSession.ExpiresAt > DateTime.UtcNow)
            {
                userAuthSession.IsRevoked = true;
                this.db.AuthSessions.Update(userAuthSession);
                AuthSession newSession = CreateAuthSession(u.Id, refreshToken);
                this.db.AuthSessions.Add(newSession);
                res!.Data!.refreshToken = refreshToken;
            }
        }

        private string GenerateJwtTokenAux(User u, ApiResponse<LoginResult> res)
        {
            string jwtToken = this.jwtService.GenerateJwtToken(u);

            res!.Data!.jwt = jwtToken;
            string refreshToken = Hasher.GenerateRefreshToken();

            CreateCookies(jwtToken, refreshToken);

            return refreshToken;
        }
        private void CreateCookies(string jwtToken, string refreshToken)
        {
            CookieOptions co = new CookieOptions();
            co.HttpOnly = true;
            co.SameSite = SameSiteMode.Strict;
            co.Expires = DateTime.UtcNow.AddMinutes(5);
            co.Secure = true;
            if (httpContext != null && httpContext.HttpContext != null)
            {
                httpContext.HttpContext.Response.Cookies.Append("jwtToken", jwtToken, co);
                httpContext.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, co);
            }
        }

        private AuthSession CreateAuthSession(Guid userId, string refreshToken)
        {
            string ipAddress = GetIpAddress();

            AuthSession authSession = new AuthSession();
            authSession.UserId = userId;
            authSession.IpAddress = ipAddress;

            byte[] data = HashRefreshToken(refreshToken);

            authSession.Hashed_Token = data;
            return authSession;
        }

        private byte[] HashRefreshToken(string refreshToken)
        {
            byte[] hashedToken;
            using (SHA256 cypher = SHA256.Create())
            {
                byte[] converted = Convert.FromBase64String(refreshToken);
                hashedToken = cypher.ComputeHash(converted);
            }
            return hashedToken;
        }

        private string GetIpAddress()
        {
            string toReturn = string.Empty;
            if (httpContext != null && httpContext.HttpContext != null && httpContext.HttpContext.Connection != null && httpContext.HttpContext.Connection.RemoteIpAddress != null)
            {
                toReturn = httpContext.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            return toReturn;
        }

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
                this.db.Update(u);
                await this.db.SaveChangesAsync();
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