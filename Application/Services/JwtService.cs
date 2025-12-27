/*
*	<copyright file="JwtService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 4:40:11 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Backend_sec_dev.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly string _secret_token;

        public JwtService(IOptions<JwtSettings> settings)
        {
            this._secret_token = settings.Value.Token;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret_token!.Trim());

            var claims = new[]
            {
            new Claim("Email", user.Email),
            new Claim("Id", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "Backend_sec_dev",
                Audience = "api",
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);



            return tokenHandler.WriteToken(token);
        }

        public bool validateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = this.GetTokenValidationParameters();
            SecurityToken validatedToken;

            try
            {
                IPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return true;
            }
            catch
            {
                throw;
            }
        }

        public JwtSecurityToken DecodeJwt(string token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jsonToken = handler.ReadJwtToken(token);
            return jsonToken;
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "Backend_sec_dev",
                ValidAudience = "api",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret_token))
            };
        }

    }
}