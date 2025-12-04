using System.ComponentModel.DataAnnotations.Schema;

namespace DripCube.Entities
{
    public class CartItem
    {
        public int Id { get; set; }


        public Guid UserId { get; set; }
        public User? User { get; set; }


        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; } = 1;
    }
}