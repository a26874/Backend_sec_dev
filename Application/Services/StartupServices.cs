/*
*	<copyright file="StartupServices">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 9:22:28 PM</date>
*	<description></description>
**/

using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Backend_sec_dev.Application.Services
{
    public static class StartupServices
    {
        public static void AddSerilogLogging(this WebApplicationBuilder builder)
        {
            var connString = builder.Configuration.GetConnectionString("DatabaseConnection");

            Log.Logger = new LoggerConfiguration()
                          .Enrich.FromLogContext()
                          .WriteTo.Console()
                         .CreateLogger();
        }
    }
}