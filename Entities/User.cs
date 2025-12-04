using System.ComponentModel.DataAnnotations;

namespace DripCube.Entities
{
    public class User
    {

        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;


        public List<CartItem> CartItems { get; set; } = new();
    }
}