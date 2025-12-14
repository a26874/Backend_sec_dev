using Backend_sec_dev.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Backend_sec_dev.Shared.Helpers
{
    public class ApiResultHelpers
    {
        public static IActionResult Success<T>(T data)
        {
            return new OkObjectResult(new ApiResponse<T>
            {
                Success = true,
                Data = data
            });
        }

        public static IActionResult Failed<T>(T data, string error)
        {
            return new ConflictObjectResult(new ApiResponse<T>
            {
                Success = false,
                Data = data,
                Error = error
            });
        }
    }
}
