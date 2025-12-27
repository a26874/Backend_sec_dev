/*
*	<copyright file="IOwnershipService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:00:23 PM</date>
*	<description></description>
**/

using System.Security.Claims;

namespace Backend_sec_dev.Application.Interfaces
{
    public interface IOwnershipService
    {
        Task<bool> UserOwnsAsync(ClaimsPrincipal user, string resourceName, Guid resourceId);

    }
}