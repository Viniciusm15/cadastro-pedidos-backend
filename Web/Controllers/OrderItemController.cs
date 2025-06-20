using Application.Interfaces;
using Common.Exceptions;
using Domain.Models.ResponseModels;
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
        /// Retorna a lista de itens associados a um pedido específico.
        /// </summary>
        /// <param name="id">Identificador único do pedido cujos itens serão recuperados.</param>
        /// <returns>Uma lista de itens do pedido correspondente ao ID fornecido.</returns>
        /// <response code="200">Pedido foi encontrado e retornado com sucesso.</response>
        /// <response code="404">Pedido com o ID fornecido não foi encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<OrderItemResponseModel>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<OrderItemResponseModel>>> GetOrderItemsByOrderId(int id)
        {
            try
            {
                var orderItems = await _orderItemService.GetOrderItemsByOrderId(id);
                return Ok(orderItems);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
