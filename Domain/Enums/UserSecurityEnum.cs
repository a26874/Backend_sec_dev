/*
*	<copyright file="UserSecurityEnum">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2026 2/7/2026 11:11:17 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Domain.Enums
{
    public enum UserSecurityEnum : byte
    {
        None = 0,
        ConfirmEmail = 1,
        ResetPassword = 2,
        ChangeEmail = 3,
    }
}