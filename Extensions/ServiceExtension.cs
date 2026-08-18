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
using Backend_sec_dev.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.RateLimiting;

namespace Backend_sec_dev.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Add dependencies
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
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
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(AutoValidateAntiforgeryTokenAttribute));
            });
            AddRateLimiter(services);
            return services;
        }

        /// <summary>
        /// Add ratelimit and its roles limitations.
        /// </summary>
        /// <param name="services"></param>
        private static void AddRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                ///Testing without global
                //options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                //    RateLimitPartition.GetFixedWindowLimiter(
                //        partitionKey: http.User.Identity?.Name ?? http.Request.Headers.Host.ToString(),
                //        factory: partition => new FixedWindowRateLimiterOptions
                //        {
                //            AutoReplenishment = true,
                //            PermitLimit = 10,
                //            QueueLimit = 5,
                //            Window = TimeSpan.FromMinutes(1)
                //        }));
                ///We have to specify the actual statuscode 429 otherwise the default is 503
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                ///For each type of role we add different limits
                ///For admin we will allow "unlimited" requests and the old ones are always first
                options.AddFixedWindowLimiter(RolesConstants.Admin,options =>
                {
                    options.PermitLimit = 9999;
                    options.Window = TimeSpan.FromMinutes(15);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 10;

                });
                ///For a user the limit is 10
                options.AddFixedWindowLimiter(RolesConstants.User, options =>
                {
                    options.PermitLimit = 10;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 0;
                });
                ///For a testing role the limit is 5
                options.AddFixedWindowLimiter(RolesConstants.Teste, options =>
                {
                    options.PermitLimit = 5;
                    options.Window = TimeSpan.FromSeconds(30);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                });

            });
            
        }
    }
}