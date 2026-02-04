/*
*	<copyright file="TransactionService">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/27/2025 7:07:22 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Application.Interfaces;
using Backend_sec_dev.Domain.Entities;
using Backend_sec_dev.Shared.Helpers;

namespace Backend_sec_dev.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IDatabaseRepository databaseRepository;

        public TransactionService(IDatabaseRepository databaseRepository)
        {
            this.databaseRepository = databaseRepository;
        }

        /// <summary>
        /// Finds a transaction by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Transaction?> GetById(Guid id)
        {
            if (NullChecks.GuidIsEmpty(id))
            {
                return null;
            }
            return await this.databaseRepository.GetById<Transaction>(id);
        }

        /// <summary>
        /// Gets all transactions
        /// </summary>
        /// <returns></returns>
        public async Task<List<Transaction>> GetTransactions()
        {
            return await this.databaseRepository.GetAll<Transaction>();
        }
    }
}