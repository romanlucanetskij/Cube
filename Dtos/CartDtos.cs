using System.ComponentModel.DataAnnotations;

namespace DripCube.Dtos
{

    public class AddToCartDto
    {
        public Guid UserId { get; set; }
        public int ProductId { get; set; }
    }


    public class CartViewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}