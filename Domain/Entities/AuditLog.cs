/*
*	<copyright file="AuditLog">
*	</copyright>
* 	<author>Marco Macedo</author>
*   <date>2025 12/13/2025 9:09:18 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Domain.Entities
{
    public class AuditLog
    {
        #region ATRIBUTOS
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = null!;
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        #endregion

        #region COMPORTAMENTO

        #region CONSTRUTORES
        public AuditLog()
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