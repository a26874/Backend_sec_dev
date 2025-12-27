/*
*	<copyright file="ServiceExtension">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:24:06 PM</date>
*	<description></description>
**/

using Backend_sec_dev.API.Filters;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Application.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backend_sec_dev.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IDatabaseRepository, DatabaseRepository>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IOwnershipService, OwnershipService>();
            services.AddScoped<OwnershipFilter>();
            return services;
        }
    }
}