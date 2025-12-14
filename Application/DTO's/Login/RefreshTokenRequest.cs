/*
*	<copyright file="RefreshTokenRequest">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 7:29:52 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Application.DTO_s.Login
{
    public class RefreshTokenRequest
    {
        #region ATTRIBUTES
        public required string refreshToken { get; set; }
        #endregion

    }
}