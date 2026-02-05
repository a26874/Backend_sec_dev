/*
*	<copyright file="ApiResponse">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 10:31:30 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s.User;
using System.Net;
using System.Text.Json.Serialization;

namespace Backend_sec_dev.Domain.Entities
{
    public class ApiResponse<T>
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; init; }

        public bool Success { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; init; }
        public HttpStatusCode statusCode { get; init; } = default!;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? ListErrors { get; init; }


        public static ApiResponse<T> Ok(T data, HttpStatusCode statusCode, string? message = null) =>
            new ApiResponse<T> { Success = true, Data = data, statusCode = statusCode, Message = message, };

        public static ApiResponse<T> Fail(HttpStatusCode statusCode, string error) =>
            new ApiResponse<T> { Success = false, statusCode = statusCode, Error = error };

        public static ApiResponse<T> FailListErrors(HttpStatusCode statusCode, List<string> errors) =>
            new ApiResponse<T> { Success = false, statusCode = statusCode, ListErrors = errors };
    }
}