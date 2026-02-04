/*
*	<copyright file="TransactionsController">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:02:30 PM</date>
*	<description></description>
**/

using Backend_sec_dev.API.Filters;
using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend_sec_dev.API.Controllers
{
    [EnableRateLimiting(RolesConstants.User)]

    public class TransactionsController : ApiControllerBase
    {
        private readonly ITransactionService transactionService;
        
        public TransactionsController(ITransactionService transactionService)
        {
            this.transactionService = transactionService;
        }

        [Route("{id}")]
        //[ServiceFilter(typeof(OwnershipFilter))] -> step 5
        [HttpGet]
        public async Task<Transaction?> GetTransactionById(Guid id)
        {
            return await this.transactionService.GetById(id);
        }

        [Route("all")]
        [HttpGet]
        public async Task<List<Transaction>> GetTransactions()
        {
            return await this.transactionService.GetTransactions();
        }
    }
}