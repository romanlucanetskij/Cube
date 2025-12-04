using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using DripCube.Data;
using DripCube.Entities;

namespace DripCube.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-checkout-session/{orderId}")]
        public async Task<ActionResult> CreateCheckoutSession(int orderId)
        {

            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound("Order not found");


            var lineItems = new List<SessionLineItemOptions>();

            foreach (var item in order.Items)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {

                        UnitAmount = (long)(item.PriceAtPurchase * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Quantity,
                });
            }


            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",

                SuccessUrl = $"{domain}/order/payment-success.html?orderId={order.Id}",

                CancelUrl = $"{domain}/order/orders.html",
            };

            var service = new SessionService();
            Session session = service.Create(options);


            order.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();


            return Ok(new { url = session.Url });
        }


        [HttpPost("confirm/{orderId}")]
        public async Task<ActionResult> ConfirmPayment(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.IsPaid = true;
            order.Status = OrderStatus.Pending;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}