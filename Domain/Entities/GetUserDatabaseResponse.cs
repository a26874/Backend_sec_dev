/*
*	<copyright file="GetUserDatabaseResponse">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 5:09:00 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Domain.Entities
{
    public class GetUserDatabaseResponse
    {
        #region ATTRIBUTES
        public bool exists { get; set; } = default!;
        public User? user { get; set; }
        #endregion
    }
}