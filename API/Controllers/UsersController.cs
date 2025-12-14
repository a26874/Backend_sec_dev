/*
*	<copyright file="UsersController">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 10:28:16 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.DTO_s.Login;
using Backend_sec_dev.Application.DTO_s.UserCreation;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Backend_sec_dev.API.Controllers
{
    public class UsersController : ApiControllerBase
    {

        private IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }
        [Route("create_account")]
        [HttpPost]
        public async Task<ApiResponse<UserCreationResultDto>> CreateUser(UserCredentialsDto userDto)
        {
            ApiResponse<UserCreationResultDto> res = await this.userService.CreateUser(userDto);
            return res;
        }

        [Route("login")]
        [HttpPost]
        public async Task<ApiResponse<LoginResult>> Login(UserCredentialsDto userCredentialsDto)
        {
            ApiResponse<LoginResult> res = await this.userService.Login(userCredentialsDto);
            return res;
        }
    }
}