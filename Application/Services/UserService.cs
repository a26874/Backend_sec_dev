/*
*	<copyright file="UserService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 10:39:34 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.DTO_s.UserCreation;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Infrastructure.Persistence;
using Backend_sec_dev.Shared.Helpers;
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
        private string HashingPassword(UserCredentialsDto userCreationDto, User u)
        {
            return PasswordHashing.HashPassword(userCreationDto);
        }
        #endregion



    }
}