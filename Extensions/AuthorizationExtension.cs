/*
*	<copyright file="AuthorizationExtension">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:22:48 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Shared.Constants;

namespace Backend_sec_dev.Extensions
{
    public static class AuthorizationExtension
    {

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(RolesConstants.Admin, policy => policy.RequireRole(RolesConstants.Admin));
                options.AddPolicy(RolesConstants.User, policy => policy.RequireRole(RolesConstants.User));
            });
            return services;
        }
    }
}