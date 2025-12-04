using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DripCube.Data;
using DripCube.Entities;
using DripCube.Services;

namespace DripCube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PhotoService _photoService;


        public ProductsController(AppDbContext context, PhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            [FromQuery] string? search,
            [FromQuery] string? category,
            [FromQuery] string? sort)
        {
            var query = _context.Products.AsQueryable();


            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }


            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                query = query.Where(p => p.Category == category);
            }


            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                default:
                    query = query.OrderByDescending(p => p.Id);
                    break;
            }

            return await query.ToListAsync();
        }


        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct([FromForm] Dtos.CreateProductDto dto)
        {
            string imageUrl = "https://via.placeholder.com/600";


            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {

                imageUrl = await _photoService.AddPhotoAsync(dto.ImageFile);
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                ImageUrl = imageUrl,
                IsInStock = true,
                StockQuantity = dto.StockQuantity
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound("Товар не найден");
            }

            return product;
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProduct(int id, [FromForm] Dtos.UpdateProductDto dto)
        {

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound("Товар не найден");


            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Category = dto.Category;
            product.StockQuantity = dto.StockQuantity;


            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {

                var newUrl = await _photoService.AddPhotoAsync(dto.ImageFile);
                product.ImageUrl = newUrl;
            }


            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product Deleted" });
        }
    }
}