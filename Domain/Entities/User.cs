/*
*	<copyright file="User">
*	</copyright>
* 	<author>Marco Macedo</author>
*   <date>2025 12/13/2025 9:07:56 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Domain.Entities
{
    public class User
    {
        #region ATRIBUTOS
        public static int MAX_ATTEMPTS_BEFORE_LOCK = 5;
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "User";
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginTime { get; set; }
        #endregion

        #region COMPORTAMENTO

        #region CONSTRUTORES
        public User()
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