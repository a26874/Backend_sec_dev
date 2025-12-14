/*
*	<copyright file="UserResultDto">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 11:32:31 PM</date>
*	<description></description>
**/

using System.Text.Json.Serialization;

namespace Backend_sec_dev.Application.DTO_s.UserCreation
{
    public class UserResultDto
    {
        #region ATTRIBUTES
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public required string email { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? success { get; set; } 
        #endregion

    }
}