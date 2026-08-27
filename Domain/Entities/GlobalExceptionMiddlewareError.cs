/*
*	<copyright file="GlobalExceptionMiddlewareError">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2026 8/25/2026 10:11:19 PM</date>
*	<description></description>
**/

using Microsoft.AspNetCore.Mvc;

namespace Backend_sec_dev.Domain.Entities
{
    public sealed class GlobalExceptionMiddlewareError : ProblemDetails
    {
        public string? StackTrace { get; set; }
    }
}