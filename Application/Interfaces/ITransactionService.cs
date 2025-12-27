/*
*	<copyright file="ITransactionService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:07:14 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Domain.Entities;
namespace Backend_sec_dev.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction?> GetById(Guid id);
    }
}