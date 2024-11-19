using Application.Interfaces;
using Common.Exceptions;
using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Retorna uma lista paginada de pedidos.
        /// </summary>
        /// <param name="pageNumber">Número da página desejada (padrão é 1).</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão é 10).</param>
        /// <returns>Uma lista paginada de pedidos com o total de itens.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<PagedResult<OrderResponseModel>>> GetOrders(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var pagedOrders = await _orderService.GetAllOrders(pageNumber, pageSize);
                return Ok(pagedOrders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retorna um pedido com o ID especificado.
        /// </summary>
        /// <param name="id">ID do pedido a ser retornado.</param>
        /// <returns>Pedido correspondente ao ID fornecido.</returns>
        /// <response code="200">Pedido foi encontrado e retornado com sucesso.</response>
        /// <response code="404">Pedido com o ID fornecido não foi encontrado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<OrderResponseModel>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderById(id);
                return Ok(order);
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

        /// <summary>
        /// Adiciona um novo pedido.
        /// </summary>
        /// <param name="orderRequestModel">Novo pedido a ser adicionado.</param>
        /// <returns>Novo pedido adicionado.</returns>
        /// <response code="201">Pedido adicionado com sucesso.</response>
        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<ActionResult<OrderResponseModel>> PostOrder(OrderRequestModel orderRequestModel)
        {
            try
            {
                var order = await _orderService.CreateOrder(orderRequestModel);
                return CreatedAtAction("GetOrderById", new { id = order.OrderId }, order);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.ValidationErrors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Atualiza um pedido existente.
        /// </summary>
        /// <param name="id">ID do pedido a ser atualizado.</param>
        /// <param name="orderRequestModel">Dados atualizados do pedido.</param>
        /// <returns>Retorna NoContent se a atualização for bem-sucedida.</returns>
        /// <response code="204">Pedido atualizado com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="404">Pedido não encontrado.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutOrder(int id, OrderRequestModel orderRequestModel)
        {
            try
            {
                await _orderService.UpdateOrder(id, orderRequestModel);
                return NoContent();
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

        /// <summary>
        /// Deleta um pedido com o ID especificado.
        /// </summary>
        /// <param name="id">ID do pedido a ser deletado.</param>
        /// <returns>Nenhum conteúdo.</returns>
        /// <response code="204">Pedido foi deletado com sucesso.</response>
        /// <response code="404">Pedido com o ID fornecido não foi encontrado.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteOrderById(int id)
        {
            try
            {
                await _orderService.DeleteOrder(id);
                return NoContent();
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
