/*
*	<copyright file="PasswordHashing">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 11:03:33 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Microsoft.AspNetCore.Identity;

namespace Backend_sec_dev.Shared.Helpers
{
    public static class PasswordHashing
    {
        public static string HashPassword(UserCredentialsDto userCreationDto)
        {
            PasswordHasher<UserCredentialsDto> pHasher = new PasswordHasher<UserCredentialsDto>();
            string returnPassword = pHasher.HashPassword(userCreationDto, userCreationDto.password);
            if (NullChecks.StringNullOrEmpty(returnPassword)) return string.Empty;
            return returnPassword;
        }


        public static PasswordVerificationResult ComparePassword(UserCredentialsDto userCredentials, string hashedPassword)
        {
            PasswordHasher<UserCredentialsDto> pHasher = new PasswordHasher<UserCredentialsDto>();
            return pHasher.VerifyHashedPassword(userCredentials, hashedPassword, userCredentials.password);
        }
    }
}