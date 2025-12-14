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
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.X86;

namespace Backend_sec_dev.Application.Services
{
    public class IdentityService : IIdentityService
    {
        protected readonly AppDbContext db;
        protected readonly IJwtService jwtService;
        public IdentityService(AppDbContext db, IJwtService jwtService)
        {
            this.db = db;
            this.jwtService = jwtService;
        }

        public async Task<ApiResponse<LoginResult>> Login(UserCredentialsDto userCredentialsDto, string ipAddress)
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
                    return await ComparePassword(userCredentialsDto, u, ipAddress);

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

            AuthSession? auth = await this.db.AuthSessions.FirstOrDefaultAsync(t => t.Hashed_Token == hashedRefreshToken && t.IsRevoked == false);
            string jwt = string.Empty;
            if (auth != null)
            {
                User? u = await this.db.Users.FirstOrDefaultAsync(t => t.Id == auth.UserId);
                if (u != null)
                {
                    jwt = jwtService.GenerateJwtToken(u);
                }
            }
            return ApiResponse<RefreshTokenResult>.Ok(new RefreshTokenResult { jwtToken = jwt }, HttpStatusCode.OK);
        }

        #region Login logic
        private async Task<ApiResponse<LoginResult>> ComparePassword(UserCredentialsDto userCredentialsDto, User u, string ipAddress)
        {
            PasswordVerificationResult result = Hasher.ComparePassword(userCredentialsDto, u.PasswordHash);
            switch (result)
            {
                case PasswordVerificationResult.Failed:
                    return await FailedLogin(u);

                case PasswordVerificationResult.Success:
                case PasswordVerificationResult.SuccessRehashNeeded:
                    return await SuccessfullLogin(u, result, ipAddress);

                default:
                    return ApiResponse<LoginResult>.Fail(HttpStatusCode.Forbidden, "Login failed");
            }
        }

        private async Task<ApiResponse<LoginResult>> SuccessfullLogin(User u, PasswordVerificationResult result, string ipAddress)
        {
            ApiResponse<LoginResult> res = new ApiResponse<LoginResult>();
            switch (result)
            {
                case PasswordVerificationResult.Success:
                    res = ApiResponse<LoginResult>.Ok(new LoginResult { message = "Login with success" }, HttpStatusCode.OK);
                    break;
                case PasswordVerificationResult.SuccessRehashNeeded:
                    res = ApiResponse<LoginResult>.Ok(new LoginResult { message = "Login with success, but you should redefine your password" }, HttpStatusCode.OK);
                    break;
            }


            try
            {
                u.LastLoginTime = DateTime.UtcNow;
                this.db.Update(u);
                AuthSession? userAuthSession = await this.db.AuthSessions.FirstOrDefaultAsync(t => t.UserId == u.Id && t.IsRevoked == false);

                string jwtToken = this.jwtService.GenerateJwtToken(u);

                if (!NullChecks.StringNullOrEmpty(jwtToken) && res.Data != null && userAuthSession == null)
                {
                    res.Data.jwt = jwtToken;
                    res!.Data!.refreshToken = Hasher.GenerateRefreshToken();
                }
                if(userAuthSession != null && res.Data != null)
                {
                    res.Data.jwt = jwtToken;
                    res.Data.refreshToken = Convert.ToBase64String(userAuthSession.Hashed_Token);
                }

                if (userAuthSession == null)
                {
                    userAuthSession = this.CreateAuthSession(u.Id, ipAddress, res!.Data!.refreshToken);
                    this.db.Add(userAuthSession);
                }
                else if (userAuthSession != null && userAuthSession.ExpiresAt < DateTime.UtcNow)
                {
                    userAuthSession = this.CreateAuthSession(u.Id, ipAddress, res!.Data!.refreshToken);
                    this.db.Update(userAuthSession);
                }
                await this.db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResult>.Fail(HttpStatusCode.InternalServerError, SqlHelpers.SqlExceptionError(ex));
            }

            return res;

        }

        private AuthSession CreateAuthSession(Guid userId, string ipAddress, string refreshToken)
        {
            AuthSession authSession = new AuthSession();
            authSession.UserId = userId;
            authSession.IpAddress = ipAddress;

            byte[] data = HashRefreshToken(refreshToken);

            authSession.Hashed_Token = data;
            return authSession;
        }

        private byte[] HashRefreshToken(string refreshToken)
        {
            byte[] hashedToken = null;
            using (SHA256 cypher = SHA256.Create())
            {
                byte[] converted = Convert.FromBase64String(refreshToken);
                hashedToken = cypher.ComputeHash(converted);
            }
            return hashedToken;
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