/*
*	<copyright file="UserUpdateDto">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 5:04:01 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Application.DTO_s.User
{
    public class UserUpdateDto
    {
        #region ATTRIBUTES
        public required string email {  get; set; }
        public required string newRole { get; set; }
        #endregion

    }
}