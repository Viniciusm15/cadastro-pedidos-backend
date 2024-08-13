using Application.Interfaces;
using Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        /// <summary>
        /// Retorna todos os itens de pedido.
        /// </summary>
        /// <returns>Uma lista de itens de pedido.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
        {
            try
            {
                var orderItems = await _orderItemService.GetAllOrderItems();
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
