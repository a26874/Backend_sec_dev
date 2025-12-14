/*
*	<copyright file="AuthSession">
*	</copyright>
* 	<author>Marco Macedo</author>
*   <date>2025 12/13/2025 9:08:55 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Domain.Entities
{
    public class AuthSession
    {
        #region ATRIBUTOS
        public Guid Id { get; set; }  
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public string IpAddress { get; set; }
        // Navigation property
        public User User { get; set; } = null!;
        #endregion

        #region COMPORTAMENTO

        #region CONSTRUTORES
        public AuthSession()
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