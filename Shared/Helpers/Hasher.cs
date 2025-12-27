/*
*	<copyright file="Hasher">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 11:03:33 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace Backend_sec_dev.Shared.Helpers
{
    public static class Hasher
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

        public static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public static bool CompareHashes(byte[] hashA, byte[] hashB)
        {
            ReadOnlySpan<byte> firstHash= hashA.AsSpan();
            ReadOnlySpan<byte> secondHash = hashB.AsSpan();
            return firstHash.SequenceEqual(secondHash);
        }
    }
}