using System.ComponentModel.DataAnnotations;

namespace Backend_sec_dev.Domain.Entities
{
    public class Customers
    {
        [Key]
        public Guid CustomerId { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Country { get; set; } = default!;
        public DateOnly BirthDate { get; set; } = default!;
    }
}
