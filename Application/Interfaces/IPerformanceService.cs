/*
*	<copyright file="IIdentityService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/14/2025 12:47:38 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.DTO_s;
using Backend_sec_dev.Domain.Entities;

namespace Backend_sec_dev.Application.Interfaces
{
    public interface IPerformanceService
    {
        Task<ApiResponse<bool>> UploadCSVFile(UploadCSVFileDto csvFileBase64);
        Task<ApiResponse<bool>> ProcessBulkInsert(GenericPostDto objectToProcess);
    }
}