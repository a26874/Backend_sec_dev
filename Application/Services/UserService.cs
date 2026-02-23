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
using Backend_sec_dev.Domain.Enums;
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
        protected readonly IHttpContextAccessor http;
        protected readonly IDatabaseRepository databaseRepository;

        public UserService(AppDbContext db, IDatabaseRepository databaseRepository, IHttpContextAccessor httpContext)
        {
            this.db = db;
            this.databaseRepository = databaseRepository;
            this.http = httpContext;
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

        /// <summary>
        /// Generates a token and saves it to the database
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string>> SendResetPasswordEmail(string email)
        {
            User? u = await this.databaseRepository.FirstOrDefaultAsync<User>(t => t.Email == email) ?? null;

            if (u == null)
                return ApiResponse<string>.Ok(string.Empty, HttpStatusCode.Accepted);


            string generatedToken = Hasher.GenerateToken(TokenType.Other);
            byte[] hashedToken = Hasher.HashToken(generatedToken);

            try
            {
                UserSecurityToken? token = this.CreateUserSecurityToken(u, hashedToken);
                this.databaseRepository.Add<UserSecurityToken>(token);
                await this.databaseRepository.SaveChanges();

                return ApiResponse<string>.Ok(generatedToken, HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail(HttpStatusCode.BadRequest, SqlHelpers.SqlExceptionError(ex));
            }
        }

        /// <summary>
        /// Resets the password
        /// </summary>
        /// <param name="token"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> ResetPasswordEmail(string token, string newPassword)
        {
            byte[] hashedToken = Hasher.HashToken(token);
            UserSecurityToken? secToken = await this.GetUserSecurityToken(hashedToken);

            if (!NullChecks.ObjectNullOrEmpty<UserSecurityToken>(secToken!))
            {

                User? user = await this.databaseRepository.FirstOrDefaultAsync<User>(t => t.Id == secToken!.UserId);

                if (!NullChecks.ObjectNullOrEmpty<User>(user!))
                {
                    bool isTokenValid = this.IsTokenValid(secToken!, user!.Id);
                    if (isTokenValid)
                    {
                        UserCredentialsDto userCredentialsDto = new UserCredentialsDto { email = user.Email, password = newPassword };
                        string password = Hasher.HashPassword(userCredentialsDto);
                        user.PasswordHash = password;

                        List<UserSecurityToken> userTokens = await this.databaseRepository.Where<UserSecurityToken>(t => t.UserId == user.Id && t.IsRevoked == false);
                        secToken!.IsRevoked = true;
                        this.RevokeUserTokens(user.Id, userTokens);

                        try
                        {
                            this.databaseRepository.Update(user);
                            this.databaseRepository.Update(secToken);
                            this.databaseRepository.UpdateRange(userTokens);
                            await this.databaseRepository.SaveChanges();
                            return ApiResponse<bool>.Ok(true, HttpStatusCode.OK, "Palavra passe mudada com sucesso");
                        }
                        catch (Exception ex)
                        {
                            return ApiResponse<bool>.Fail(HttpStatusCode.BadRequest, SqlHelpers.SqlExceptionError(ex));
                        }
                    }
                }
            }
            return ApiResponse<bool>.Fail(HttpStatusCode.Unauthorized, "Ocorreu um erro a mudar a password");
        }

        #region User Logic

        private UserSecurityToken CreateUserSecurityToken(User u, byte[] hashedToken)
        {
            return new UserSecurityToken
            {
                UserId = u.Id,
                HashedToken = hashedToken,
                Type = UserSecurityEnum.ResetPassword,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsRevoked = false,
                IpAddress = HttpContextHelper.GetClientIpAddress(http.HttpContext!) ?? "Unknown"
            };
        }

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

        private async Task<UserSecurityToken?> GetUserSecurityToken(byte[] hashedToken)
        {
            return await this.databaseRepository.FirstOrDefaultAsync<UserSecurityToken>(t => t.HashedToken.SequenceEqual(hashedToken));
        }

        private bool IsTokenValid(UserSecurityToken token, Guid userId)
        {
            string ipAddress = HttpContextHelper.GetClientIpAddress(http.HttpContext!) ?? "Unknown";

            if (token.ExpiresAt < DateTime.UtcNow || token.IsRevoked || token.Type != UserSecurityEnum.ResetPassword || token.UserId != userId)
                return false;
            
            return true;
        }

        private void RevokeUserTokens(Guid userId, List<UserSecurityToken> userTokens)
        {
            foreach (UserSecurityToken tk in userTokens)
            {
                tk.IsRevoked = true;
            }
        }
        #endregion



    }
}