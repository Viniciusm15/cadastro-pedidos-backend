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
        /// <param name="pageNumber">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão: 10).</param>
        /// <returns>Lista paginada contendo os pedidos e o total de registros.</returns>
        /// <response code="200">Retorna a lista de pedidos paginada.</response>
        /// <response code="500">Erro interno ao buscar os pedidos.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<OrderResponseModel>), 200)]
        [ProducesResponseType(500)]
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
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponseModel), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
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
        /// Cria um novo pedido com os dados informados.
        /// </summary>
        /// <param name="orderRequestModel">Objeto com os dados do novo pedido.</param>
        /// <returns>O pedido criado com seu ID.</returns>
        /// <response code="201">Pedido criado com sucesso.</response>
        /// <response code="400">Dados inválidos fornecidos.</response>
        /// <response code="500">Erro interno ao criar o pedido.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseModel), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<OrderResponseModel>> PostOrder([FromBody] OrderRequestModel orderRequestModel)
        {
            try
            {
                var order = await _orderService.CreateOrder(orderRequestModel);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
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
        /// <response code="500">Erro interno ao criar o pedido.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PutOrder(int id, [FromBody] OrderRequestModel orderRequestModel)
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
        /// <response code="500">Erro interno ao criar o pedido.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
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

        /// <summary>
        /// Gera um relatório de pedidos em formato CSV.
        /// </summary>
        /// <returns>Arquivo CSV contendo o relatório de vendas.</returns>
        /// <response code="200">Relatório gerado e retornado com sucesso.</response>
        /// <response code="500">Erro interno ao gerar o relatório.</response>
        [HttpGet("generate-csv-report")]
        [Produces("text/csv")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GenerateOrdersCsvReport()
        {
            try
            {
                var reportFile = await _orderService.GenerateOrdersReportCsvAsync();
                return File(reportFile, "text/csv", "Order_Report.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while generating the CSV report: {ex.Message}");
            }
        }
    }
}
