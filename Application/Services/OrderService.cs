using Application.Interfaces;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentValidation;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IValidator<Order> _orderValidator;

        public OrderService(IOrderRepository orderRepository, IValidator<Order> orderValidator)
        {
            _orderRepository = orderRepository;
            _orderValidator = orderValidator;
        }

        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<Order> GetOrderById(int id)
        {
            return await _orderRepository.GetOrderByIdAsync(id) ?? throw new NotFoundException($"Order not found by ID: {id}");
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

            await _orderRepository.AddOrderAsync(order);
            return order;
        }

        public async Task UpdateOrder(int id, OrderRequestModel orderRequestModel)
        {
            var order = await GetOrderById(id);

            order.OrderDate = orderRequestModel.OrderDate;
            order.TotalValue = orderRequestModel.TotalValue;
            order.ClientId = orderRequestModel.ClientId;

            var validationResult = _orderValidator.Validate(order);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task DeleteOrder(int id)
        {
            var order = await GetOrderById(id);
            await _orderRepository.DeleteOrderAsync(order);
        }
    }
}
