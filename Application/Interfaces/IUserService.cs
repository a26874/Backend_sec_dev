/*
*	<copyright file="IUserService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 10:40:06 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.DTO_s.Login;
using Backend_sec_dev.Application.DTO_s.UserCreation;
using Backend_sec_dev.Domain.Entities;

namespace Backend_sec_dev.Application.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserCreationResultDto>> CreateUser(UserCredentialsDto user);
        Task<ApiResponse<LoginResult>> Login(UserCredentialsDto user);
    
    }
}