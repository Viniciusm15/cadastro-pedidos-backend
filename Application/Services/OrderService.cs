using Application.Interfaces;
using Common.Exceptions;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentValidation;
using Infra.Data;
using Infra.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<Order> _orderValidator;

        public OrderService(ApplicationDbContext context, IValidator<Order> orderValidator)
        {
            _context = context;
            _orderValidator = orderValidator;
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _context.Orders
                .WhereActive()
                .OrderBy(order => order.OrderDate)
                .Include(order => order.Client)
                .Include(order => order.OrderItens)
                .ToListAsync();
        }

        public async Task<Order> GetOrderById(int id)
        {
            var order = await _context.Orders
                .WhereActive()
                .Include(order => order.Client)
                .Include(order => order.OrderItens)
                .FirstOrDefaultAsync(order => order.Id == id);

            return order ?? throw new NotFoundException($"Order not found by ID: {id}");
        }

        public async Task<Order> CreateOrder(OrderRequestModel orderRequestModel)
        {
            var order = new Order
            {
                OrderDate = orderRequestModel.OrderDate,
                TotalValue = orderRequestModel.TotalValue,
                ClientId = orderRequestModel.ClientId
            };

            var validationResult = _orderValidator.Validate(order);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task UpdateOrder(int id, OrderRequestModel orderRequestModel)
        {
            var order = await GetOrderById(id) ?? throw new NotFoundException($"Order not found by ID: {id}");

            order.OrderDate = orderRequestModel.OrderDate;
            order.TotalValue = orderRequestModel.TotalValue;
            order.ClientId = orderRequestModel.ClientId;

            var validationResult = _orderValidator.Validate(order);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrder(int id)
        {
            var order = await GetOrderById(id) ?? throw new NotFoundException($"Order not found by ID: {id}");

            order.IsActive = false;
            order.DeletedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
