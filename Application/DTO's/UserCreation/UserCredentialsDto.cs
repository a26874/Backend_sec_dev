/*
*	<copyright file="UserDto">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 10:41:27 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Application.DTO_s
{
    public class UserCredentialsDto
    {
        #region ATTRIBUTES
        public required string email { get; set; }
        public required string password { get; set; }
        #endregion

    }
}