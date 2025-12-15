/*
*	<copyright file="LoginResult">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 12:42:44 AM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Application.DTO_s.Login
{
    public sealed record class LoginResult
    {
        #region ATTRIBUTES
        public string message { get; set; } = default!;
        public string jwt { get; set; } = default!;
        public string refreshToken { get; set; } = default!;
        #endregion
    }

}