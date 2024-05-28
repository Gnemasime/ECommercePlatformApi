// Controllers/OrdersController.cs
using ECommercePlatform.Data;
using ECommercePlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var userId = User.Claims.First(c => c.Type == "UserID").Value;
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder()
        {
            var userId = User.Claims.First(c => c.Type == "UserID").Value;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any()) return BadRequest("Cart is empty.");

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                PaymentStatus = "Pending"
            };

            foreach (var item in cart.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });
            }

            order.TotalAmount = order.OrderItems.Sum(oi => oi.Quantity * oi.Price);

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);  // Clear the cart
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}
