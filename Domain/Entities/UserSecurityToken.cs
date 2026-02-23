/*
*	<copyright file="UserSecurityToken">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2026 2/7/2026 9:38:45 PM</date>
*	<description></description>
**/

using Backend_sec_dev.Domain.Enums;

namespace Backend_sec_dev.Domain.Entities
{
    public class UserSecurityToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public byte[] HashedToken { get; set; } = default!;
        public UserSecurityEnum Type { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public string IpAddress { get; set; } = default!;
    }
}