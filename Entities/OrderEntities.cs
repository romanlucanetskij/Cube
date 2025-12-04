using System.ComponentModel.DataAnnotations.Schema;

namespace DripCube.Entities
{
    public enum OrderStatus
    {
        Pending = 0,
        Shipped = 1,
        Delivered = 2,
        Canceled = 3
    }

    public class Order
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;


        public List<OrderItem> Items { get; set; } = new();

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public bool IsPaid { get; set; } = false;
        public string? StripeSessionId { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtPurchase { get; set; }
    }
}