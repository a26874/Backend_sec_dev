/*
*	<copyright file="OwnershipService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:01:07 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.Interfaces;
using System.Security.Claims;

namespace Backend_sec_dev.Application.Services
{
    public class OwnershipService : IOwnershipService
    {
        public Task<bool> UserOwnsAsync(ClaimsPrincipal user, string resourceName, Guid resourceId)
        {
            return Task.FromResult(true);
        }
    }
}