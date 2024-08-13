using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentValidation;
using Infra.Data;
using Infra.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<Order> _orderValidator;

        public OrderController(ApplicationDbContext context, IValidator<Order> orderValidator)
        {
            _context = context;
            _orderValidator = orderValidator;
        }

        /// <summary>
        /// Retorna todos os pedidos
        /// </summary>
        /// <returns>Uma lista de pedidos.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .WhereActive()
                .OrderBy(order => order.OrderDate)
                .Include(order => order.Client)
                .Include(order => order.OrderItens)
                .ToListAsync();
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
        public async Task<ActionResult<Order>> GetOrderById(int id)
        {
            var order = await _context.Orders
                .WhereActive()
                .Include(order => order.Client)
                .Include(order => order.OrderItens)
                .FirstOrDefaultAsync(order => order.Id == id);

            if (order == null)
                return NotFound("Order not found by ID: " + id + ". Please try again.");

            return order;
        }

        /// <summary>
        /// Adiciona um novo pedido
        /// </summary>
        /// <param name="orderRequestModel">Novo pedido a ser adicionado.</param>
        /// <returns>Novo pedido adicionado.</returns>
        /// <response code="201">Pedido adicionado com sucesso.</response>
        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Order>> PostOrder(OrderRequestModel orderRequestModel)
        {
            var order = new Order()
            {
                OrderDate = orderRequestModel.OrderDate,
                TotalValue = orderRequestModel.TotalValue,
                ClientId = orderRequestModel.ClientId
            };

            var validationResult = _orderValidator.Validate(order);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(errorMessages);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrderById", new { id = order.Id }, order);
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
            var order = await _context.Orders
                .Where(o => o.IsActive)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound("Order not found by ID: " + id);

            order.OrderDate = orderRequestModel.OrderDate;
            order.TotalValue = orderRequestModel.TotalValue;
            order.ClientId = orderRequestModel.ClientId;

            var validationResult = _orderValidator.Validate(order);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(errorMessages);
            }

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
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
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound("Order not found by ID: " + id + ". Please try again.");

            order.IsActive = false;
            order.DeletedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
