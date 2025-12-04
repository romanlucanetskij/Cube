using System.ComponentModel.DataAnnotations;

namespace DripCube.Dtos
{
    public class ActivateManagerDto
    {
        public int ManagerId { get; set; }
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [Required] public string Email { get; set; } = string.Empty;
    }
}