/*
*	<copyright file="IdentityController">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 12:45:59 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Backend_sec_dev.API.Controllers
{
    public class PerformanceController : ApiControllerBase
    {
        private IPerformanceService performanceService;
        protected readonly IHttpContextAccessor httpContext;

        public PerformanceController(IPerformanceService performanceService, IHttpContextAccessor httpContext)
        {
            this.performanceService = performanceService;
            this.httpContext = httpContext;
        }

        [Route("UploadCSVFile")]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<ApiResponse<bool>> UploadCSVFile([FromBody] UploadCSVFileDto file)
        {
            ApiResponse<bool> res = await this.performanceService.UploadCSVFile(file);
            return res;
        }

        [Route("ProcessBulkInsert")]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<ApiResponse<bool>> ProcessBulkInsert([FromBody] GenericPostDto objectToProcess)
        {
            ApiResponse<bool> res = await this.performanceService.ProcessBulkInsert(objectToProcess);
            return res;
        }
    }
}