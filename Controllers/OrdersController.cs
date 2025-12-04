using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DripCube.Data;
using DripCube.Entities;

namespace DripCube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("checkout")]
        public async Task<ActionResult> Checkout([FromBody] Dtos.CheckoutDto dto)
        {

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == dto.UserId)
                .ToListAsync();

            if (!cartItems.Any()) return BadRequest("Cart is empty");


            foreach (var item in cartItems)
            {
                if (item.Product.StockQuantity < item.Quantity)
                    return BadRequest($"Товара {item.Product.Name} нет в наличии");
            }


            decimal originalTotal = cartItems.Sum(x => x.Product.Price * x.Quantity);
            decimal finalTotal = originalTotal;
            string message = "Order Placed";

            if (dto.PromoCode == "DRIP2077")
            {
                finalTotal = originalTotal * 0.5m;
                message = "PROMO APPLIED";
            }


            var order = new Order
            {
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = finalTotal,


                FullName = dto.FullName,
                Phone = dto.Phone,
                City = dto.City,
                Address = dto.Address,
                PostalCode = dto.PostalCode
            };

            foreach (var item in cartItems)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.Product.Price
                });

                item.Product.StockQuantity -= item.Quantity;
                if (item.Product.StockQuantity <= 0) item.Product.IsInStock = false;
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(new { message = message, orderId = order.Id, total = finalTotal });
        }



        [HttpGet("my-orders/{userId}")]
        public async Task<ActionResult> GetMyOrders(Guid userId)
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    Date = o.CreatedAt,
                    Status = o.Status.ToString(),
                    Total = o.TotalAmount,
                    Items = o.Items.Select(i => new { i.Product.Name, i.Quantity, i.PriceAtPurchase })
                })
                .ToListAsync();

            return Ok(orders);
        }


        [HttpGet("all-orders")]
        public async Task<ActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.UserId,
                    o.CreatedAt,
                    o.TotalAmount,
                    o.Status,

                    Items = o.Items.Select(i => new
                    {
                        Quantity = i.Quantity,

                        Product = i.Product == null ? null : new { Name = i.Product.Name }
                    })
                })
                .ToListAsync();

            return Ok(orders);
        }


        [HttpPost("update-status/{orderId}")]
        public async Task<ActionResult> UpdateStatus(int orderId, [FromBody] int statusId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = (OrderStatus)statusId;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}