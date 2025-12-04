using System.ComponentModel.DataAnnotations;

namespace DripCube.Entities
{
    public class Employee
    {
        public int Id { get; set; }

        public EmployeeRole Role { get; set; }

        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;


        public string PersonalId { get; set; } = string.Empty;


        public string ChatId { get; set; } = string.Empty;


        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }


        public bool IsActive { get; set; } = false;
    }
}