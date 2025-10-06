using Application.Interfaces;
using Common.Exceptions;
using Common.Models;
using Domain.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Obtém as métricas principais do dashboard
        /// </summary>
        /// <returns>Dados consolidados com as métricas do sistema</returns>
        /// <response code="200">Métricas retornadas com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(DashboardResponseModel), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                var metrics = await _dashboardService.GetDashboardMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Obtém dados de vendas semanais para exibição no gráfico
        /// </summary>
        /// <returns>Lista de vendas agrupadas por dia da semana</returns>
        /// <response code="200">Dados de vendas semanais retornados com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("weekly-sales")]
        [ProducesResponseType(typeof(List<DashboardWeeklySalesResponseModel>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetWeeklySalesData()
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-6).Date;
                var endDate = DateTime.Now.Date;

                var weeklySales = await _dashboardService.GetWeeklySalesDataAsync(startDate, endDate);
                return Ok(weeklySales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retorna uma lista paginada de produtos com estoque baixo
        /// </summary>
        /// <param name="pageNumber">Número da página desejada (padrão é 1)</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão é 10)</param>
        /// <returns>Lista paginada de produtos com estoque abaixo do limite</returns>
        /// <response code="200">Lista de produtos retornada com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("low-stock-products")]
        [ProducesResponseType(typeof(PagedResult<DashboardLowStockProductResponseModel>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<DashboardLowStockProductResponseModel>>> GetLowStockProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _dashboardService.GetLowStockProductsAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retorna uma lista paginada de pedidos pendentes
        /// </summary>
        /// <param name="pageNumber">Número da página desejada (padrão é 1)</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão é 10)</param>
        /// <returns>Lista paginada de pedidos com status 'Pending' ou 'Processing'</returns>
        /// <response code="200">Lista de pedidos retornada com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("pending-orders")]
        [ProducesResponseType(typeof(PagedResult<DashboardPendingOrderResponseModel>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<DashboardPendingOrderResponseModel>>> GetPendingOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _dashboardService.GetPendingOrdersAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Obtém dados consolidados sobre clientes
        /// </summary>
        /// <returns>Objeto com informações sobre total de clientes, novos clientes no mês,
        /// taxa de retenção e dados mensais dos últimos 6 meses</returns>
        /// <response code="200">Dados de clientes retornados com sucesso</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpGet("clients-data")]
        [ProducesResponseType(typeof(DashboardClientSummaryResponseModel), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetClientsData()
        {
            try
            {
                var result = await _dashboardService.GetClientDataAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Realiza a reposição de estoque de um produto específico
        /// </summary>
        /// <param name="productId">ID do produto a ter o estoque reposto</param>
        /// <param name="restockQuantity">Quantidade a ser adicionada ao estoque</param>
        /// <returns>Resultado da operação com mensagem e novo valor de estoque</returns>
        /// <response code="200">Estoque reposto com sucesso</response>
        /// <response code="400">Quantidade inválida (menor ou igual a zero)</response>
        /// <response code="404">Produto não encontrado</response>
        /// <response code="500">Erro interno ao processar a requisição</response>
        [HttpPost("restock-product/{productId}")]
        [ProducesResponseType(typeof(DashboardRestockResponseModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RestockProduct(int productId, [FromBody] int restockQuantity)
        {
            try
            {
                var result = await _dashboardService.RestockProductAsync(productId, restockQuantity);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.ValidationErrors);
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
