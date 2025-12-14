/*
*	<copyright file="Transaction">
*	</copyright>
* 	<author>Marco Macedo</author>
*   <date>2025 12/13/2025 9:09:10 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Domain.Entities
{
    public class Transaction
    {
        #region ATRIBUTOS
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Merchant { get; set; } = null!;
        public string Category { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        #endregion

        #region COMPORTAMENTO

        #region CONSTRUTORES
        public Transaction()
        {

        }
        #endregion

        #region PROPRIEDADES

        #endregion

        #region OPERADORES

        #endregion

        #region OVERRIDES

        #endregion

        #region OUTROS METODOS

        #endregion

        #endregion
    }
}