/*
*	<copyright file="IJwtService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 4:41:10 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Domain.Entities;

namespace Backend_sec_dev.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateJwtToken(User u);
    }
}