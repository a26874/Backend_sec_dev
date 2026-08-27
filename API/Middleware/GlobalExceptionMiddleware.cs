/*
*	<copyright file="GlobalExceptionMiddleware">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2026 2/7/2026 12:49:09 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Domain.Entities;

namespace Backend_sec_dev.API.Middleware
{
    internal sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception occurred");

                switch (ex)
                {
                    case ApplicationException:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        break;
                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        break;
                }

                string detailDescription = env.IsDevelopment() ? ex.Message : "Unhandled error occured";
                await context.Response.WriteAsJsonAsync(
                    new GlobalExceptionMiddlewareError
                    {
                        Type = ex.GetType().Name,
                        Title = "Unhandled error occured",
                        Detail = detailDescription,
                        //rework this and throw the errors in a more structured way
                        StackTrace = ex.StackTrace
                    }
                );
            }

        }
    }
}