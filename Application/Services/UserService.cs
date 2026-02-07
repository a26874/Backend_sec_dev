/*
*	<copyright file="UserService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 10:39:34 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.DTO_s.User;
using Backend_sec_dev.Application.DTO_s.UserCreation;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Domain.Validators;
using Backend_sec_dev.Infrastructure.Persistence;
using Backend_sec_dev.Shared.Constants;
using Backend_sec_dev.Shared.Helpers;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backend_sec_dev.Application.Services
{
    public class UserService : IUserService
    {
        protected readonly AppDbContext db;

        public UserService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<ApiResponse<UserResultDto>> CreateUser(UserCredentialsDto userCredentialsDto)
        {

            List<string> errors = new List<string>();
            errors = this.ValidateUserCredentials(userCredentialsDto);
            if (errors.Any())
            {
                return ApiResponse<UserResultDto>.FailListErrors(HttpStatusCode.UnprocessableEntity, errors);
            }

            User u = new User();
            GetUserDatabaseResponse databaseUser = await VerifyIfUserExists(userCredentialsDto.email);

            if (databaseUser.exists)
                return ApiResponse<UserResultDto>.Fail(HttpStatusCode.UnprocessableEntity, "User already exists");

            bool createUserResult = CreateUserObject(userCredentialsDto, u);
            if (!createUserResult)
                return ApiResponse<UserResultDto>.Fail(HttpStatusCode.UnprocessableEntity, "Failed creating user");

            try
            {
                return await SaveChangesUser(u);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResultDto>.Fail(HttpStatusCode.InternalServerError, SqlHelpers.SqlExceptionError(ex));
            }
        }

        public async Task<ApiResponse<UserResultDto>> UpdateUserRole(UserUpdateDto userUpdateDto)
        {
            if (userUpdateDto == null || NullChecks.StringNullOrEmpty(userUpdateDto.email) || NullChecks.StringNullOrEmpty(userUpdateDto.newRole))
                return ApiResponse<UserResultDto>.Fail(HttpStatusCode.Forbidden, "Email or role should have values");


            GetUserDatabaseResponse databaseUser = await VerifyIfUserExists(userUpdateDto.email);
            if (!databaseUser.exists && databaseUser == null)
                return ApiResponse<UserResultDto>.Fail(HttpStatusCode.UnprocessableEntity, "Theres no user with given email");

            User u = databaseUser.user!;

            try
            {
                u.Role = userUpdateDto.newRole;
                this.db.Users.Update(u);
                await this.db.SaveChangesAsync();
                return ApiResponse<UserResultDto>.Ok(new UserResultDto { email = userUpdateDto.email, success = true }, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResultDto>.Fail(HttpStatusCode.InternalServerError, SqlHelpers.SqlExceptionError(ex));
            }
        }

        #region User Logic

        private async Task<GetUserDatabaseResponse> VerifyIfUserExists(string userEmail)
        {
            GetUserDatabaseResponse res = new GetUserDatabaseResponse();
            User? existingUser = await this.db.Users.FirstOrDefaultAsync(t => t.Email == userEmail);

            if (existingUser != null)
            {
                res.user = existingUser;
                res.exists = true;
                return res;
            }
            res.user = null;
            res.exists = false;
            return res;
        }

        private bool CreateUserObject(UserCredentialsDto userCreationDto, User u)
        {
            u.Email = userCreationDto.email;
            string hashedPassword = HashingPassword(userCreationDto, u);

            if (NullChecks.StringNullOrEmpty(hashedPassword))
                return false;

            u.PasswordHash = hashedPassword;
            u.Role = RolesConstants.User;
            return true;
        }

        private async Task<ApiResponse<UserResultDto>> SaveChangesUser(User u)
        {
            this.db.Users.Add(u);
            var result = await this.db.SaveChangesAsync();

            return ApiResponse<UserResultDto>.Ok(new UserResultDto { email = u.Email }, HttpStatusCode.Created, "Account created with success! ");
        }
        private string HashingPassword(UserCredentialsDto userCreationDto, User u)
        {
            return Hasher.HashPassword(userCreationDto);
        }

        /// <summary>
        /// Uses the custom validator and returns any errors.
        /// </summary>
        /// <param name="userCredentialsDto"></param>
        /// <returns></returns>
        private List<string> ValidateUserCredentials(UserCredentialsDto userCredentialsDto)
        {
            List<string> errors = new List<string>();
            UserCreationValidator validator = new UserCreationValidator();

            try
            {
                ValidationResult validation = validator.Validate(userCredentialsDto);
                if (!validation.IsValid)
                {
                    foreach (ValidationFailure error in validation.Errors)
                    {
                        errors.Add(string.Format("{0}: {1}", error.PropertyName, error.ErrorMessage));
                    }
                }
                return errors;
            }
            catch
            {
                throw;
            }

        }

        #endregion



    }
}