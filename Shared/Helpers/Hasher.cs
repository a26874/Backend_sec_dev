/*
*	<copyright file="Hasher">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 11:03:33 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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

        /// <summary>
        /// Generates token based on teh type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GenerateToken(TokenType type)
        {
            switch (type)
            {
                case TokenType.JwtToken:
                    return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
                case TokenType.Other:
                    return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
                default:
                    return string.Empty;
            }
        }

        public static bool CompareHashes(byte[] hashA, byte[] hashB)
        {
            ReadOnlySpan<byte> firstHash = hashA.AsSpan();
            ReadOnlySpan<byte> secondHash = hashB.AsSpan();
            return firstHash.SequenceEqual(secondHash);
        }

        /// <summary>
        /// hashes a base64 token
        /// </summary>
        /// <param name="hashToken"></param>
        /// <returns></returns>
        public static byte[] HashToken(string hashToken)
        {
            byte[] hashedToken;
            using (SHA256 cypher = SHA256.Create())
            {
                byte[] converted = WebEncoders.Base64UrlDecode(hashToken);
                hashedToken = cypher.ComputeHash(converted);
            }
            return hashedToken;
        }

    }
}