/*
*	<copyright file="UsersController">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 10:28:16 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.DTO_s.User;
using Backend_sec_dev.Application.DTO_s.UserCreation;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<ApiResponse<UserResultDto>> CreateUser(UserCredentialsDto userDto)
        {
            ApiResponse<UserResultDto> res = await this.userService.CreateUser(userDto);
            return res;
        }
        
        [Authorize(Roles = "Admin")]
        [Route("change_role")]
        [HttpPut]
        public async Task<ApiResponse<UserResultDto>> UpdateUserRole(UserUpdateDto userUpdateDto)
        {
            ApiResponse<UserResultDto> res = await this.userService.UpdateUserRole(userUpdateDto);
            return res;
        }

      
    }
}