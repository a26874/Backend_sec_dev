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

namespace Backend_sec_dev.Application.Services
{
    public class IdentityService : IIdentityService
    {
        protected readonly AppDbContext db;

        public IdentityService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<ApiResponse<LoginResult>> Login(UserCredentialsDto userCredentialsDto)
        {
            if (userCredentialsDto == null || NullChecks.StringNullOrEmpty(userCredentialsDto.email) || NullChecks.StringNullOrEmpty(userCredentialsDto.password))
                return ApiResponse<LoginResult>.Fail("Email or password not provided.");
            try
            {
                User? u = await this.db.Users.FirstOrDefaultAsync(t => t.Email == userCredentialsDto.email);
                if (u != null)
                {
                    if (u.IsLocked)
                    {
                        return ApiResponse<LoginResult>.Fail("Your account is locked. To Unlock please recover your password");
                    }
                    return await ComparePassword(userCredentialsDto, u);

                }
                return ApiResponse<LoginResult>.Fail("Login failed");

            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResult>.Fail(SqlHelpers.SqlExceptionError(ex));
            }

        }

        #region Login logic
        private async Task<ApiResponse<LoginResult>> ComparePassword(UserCredentialsDto userCredentialsDto, User u)
        {
            PasswordVerificationResult result = PasswordHashing.ComparePassword(userCredentialsDto, u.PasswordHash);
            switch (result)
            {
                case PasswordVerificationResult.Failed:
                    return await FailedLogin(u);

                case PasswordVerificationResult.Success:
                case PasswordVerificationResult.SuccessRehashNeeded:
                    return await SuccessfullLogin(u, result);

                default:
                    return ApiResponse<LoginResult>.Fail("Login failed");
            }
        }

        private async Task<ApiResponse<LoginResult>> SuccessfullLogin(User u, PasswordVerificationResult result)
        {
            ApiResponse<LoginResult> res = new ApiResponse<LoginResult>();
            u.LastLoginTime = DateTime.UtcNow;
            try
            {
                this.db.Update(u);
                await this.db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResult>.Fail(SqlHelpers.SqlExceptionError(ex));
            }
            switch (result)
            {
                case PasswordVerificationResult.Success:
                    res = ApiResponse<LoginResult>.Ok(new LoginResult { message = "Login with success" });
                    break;
                case PasswordVerificationResult.SuccessRehashNeeded:
                    res = ApiResponse<LoginResult>.Ok(new LoginResult { message = "Login with success, but you should redefine your password" });
                    break;
            }
            return res;

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
                return ApiResponse<LoginResult>.Fail(string.Format("Login failed, remaining tries before account blocking: {0}", User.MAX_ATTEMPTS_BEFORE_LOCK - u.FailedLoginAttempts));
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResult>.Fail(SqlHelpers.SqlExceptionError(ex));
            }
        }
        #endregion
    }
}