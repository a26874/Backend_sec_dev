/*
*	<copyright file="RefreshTokenResult">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 5:43:33 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Application.DTO_s.Login
{
    public class RefreshTokenResult
    {
        #region ATTRIBUTES
        public string? refreshToken { get; set; } = default!;
        public string jwtToken { get; set; } = default!;
        #endregion

    }
}