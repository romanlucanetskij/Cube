using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DripCube.Data;
using DripCube.Entities;
using DripCube.Dtos;

namespace DripCube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartsController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<CartViewDto>>> GetCart(Guid userId)
        {

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();


            var result = cartItems.Select(c => new CartViewDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                ImageUrl = c.Product.ImageUrl,
                Price = c.Product.Price,
                Quantity = c.Quantity
            }).ToList();

            return Ok(result);
        }


        [HttpPost("add")]
        public async Task<ActionResult> AddToCart(AddToCartDto dto)
        {

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == dto.UserId && c.ProductId == dto.ProductId);

            if (existingItem != null)
            {

                existingItem.Quantity++;
            }
            else
            {

                var newItem = new CartItem
                {
                    UserId = dto.UserId,
                    ProductId = dto.ProductId,
                    Quantity = 1
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Added to cart" });
        }


        [HttpDelete("{itemId}")]
        public async Task<ActionResult> RemoveItem(int itemId)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item == null) return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }


        [HttpDelete("clear/{userId}")]
        public async Task<ActionResult> ClearCart(Guid userId)
        {
            var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart cleared" });
        }
    }
}