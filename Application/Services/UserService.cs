/*
*	<copyright file="UserService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 10:39:34 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.DTO_s.Login;
using Backend_sec_dev.Application.DTO_s.UserCreation;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Infrastructure.Persistence;
using Backend_sec_dev.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend_sec_dev.Application.Services
{
    public class UserService : IUserService
    {
        protected readonly AppDbContext db;

        public UserService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<ApiResponse<UserCreationResultDto>> CreateUser(UserCredentialsDto userCredentialsDto)
        {

            if (userCredentialsDto == null || NullChecks.StringNullOrEmpty(userCredentialsDto.email) || NullChecks.StringNullOrEmpty(userCredentialsDto.password))
                return ApiResponse<UserCreationResultDto>.Fail("Email and password should have values");

            User u = new User();
            bool userExists = await VerifyIfUserExists(userCredentialsDto);
            if (userExists)
                return ApiResponse<UserCreationResultDto>.Fail("User already exists");


            bool createUserResult = CreateUserObject(userCredentialsDto, u);

            if (!createUserResult)
                return ApiResponse<UserCreationResultDto>.Fail("Failed creating user");

            try
            {
                return await SaveChangesUser(u);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserCreationResultDto>.Fail(SqlHelpers.SqlExceptionError(ex));
            }
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


        #region User Logic

        private async Task<bool> VerifyIfUserExists(UserCredentialsDto userCredentialsDto)
        {
            User? existingUser = await this.db.Users.FirstOrDefaultAsync(t => t.Email == userCredentialsDto.email);

            if (existingUser != null)
                return false;
            return true;
        }
        private bool CreateUserObject(UserCredentialsDto userCreationDto, User u)
        {
            u.Email = userCreationDto.email;
            string hashedPassword = HashingPassword(userCreationDto, u);

            if (NullChecks.StringNullOrEmpty(hashedPassword))
                return false;

            u.PasswordHash = hashedPassword;
            return true;
        }

        private async Task<ApiResponse<UserCreationResultDto>> SaveChangesUser(User u)
        {
            this.db.Users.Add(u);
            var result = await this.db.SaveChangesAsync();

            return ApiResponse<UserCreationResultDto>.Ok(new UserCreationResultDto { email = u.Email }, "Account created with success! ");
        }

        #endregion

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
        private string HashingPassword(UserCredentialsDto userCreationDto, User u)
        {
            return PasswordHashing.HashPassword(userCreationDto);
        }

        #endregion
    }
}