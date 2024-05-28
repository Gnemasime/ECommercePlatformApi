// Controllers/CartController.cs
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
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Cart>> GetCart()
        {
            var userId = User.Claims.First(c => c.Type == "UserID").Value;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        [HttpPost("AddItem")]
        public async Task<ActionResult> AddItem(int productId, int quantity)
        {
            var userId = User.Claims.First(c => c.Type == "UserID").Value;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null)
            {
                cartItem = new CartItem { ProductId = productId, Quantity = quantity };
                cart.Items.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("RemoveItem")]
        public async Task<ActionResult> RemoveItem(int productId)
        {
            var userId = User.Claims.First(c => c.Type == "UserID").Value;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return NotFound();

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null) return NotFound();

            cart.Items.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
