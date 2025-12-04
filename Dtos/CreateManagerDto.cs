using System.ComponentModel.DataAnnotations;

namespace DripCube.Dtos
{
    public class CreateManagerDto
    {
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}