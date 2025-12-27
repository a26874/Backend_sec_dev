/*
*	<copyright file="DatabaseExtension">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:25:02 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend_sec_dev.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DatabaseConnection")));

            return services;
        }
    }
}