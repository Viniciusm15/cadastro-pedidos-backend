using Domain.Models.Entities;
using Infra.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController(ApplicationDbContext _context) : ControllerBase
    {
        /// <summary>
        /// Retorna todos os itens de pedido.
        /// </summary>
        /// <returns>Uma lista de itens de pedido.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
        {
            var orderItems = await _context.OrderItens
                .Include(orderItem => orderItem.Order)
                .Include(orderItem => orderItem.Product)
                .ToListAsync();

            return Ok(orderItems);
        }
    }
}
