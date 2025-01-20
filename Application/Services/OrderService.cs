using Application.Interfaces;
using Common.Exceptions;
using Common.Models;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IValidator<Order> _orderValidator;

        public OrderService(ILogger<OrderService> logger, IOrderRepository orderRepository, IValidator<Order> orderValidator)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _orderValidator = orderValidator;
        }

        public async Task<PagedResult<OrderResponseModel>> GetAllOrders(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Retrieving orders for page {PageNumber} with size {PageSize}", pageNumber, pageSize);
            var pagedOrders = await _orderRepository.GetAllOrdersAsync(pageNumber, pageSize);

            var orderModels = pagedOrders.Items.Select(order => new OrderResponseModel
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalValue = order.TotalValue,
                Status = order.Status,
                ClientId = order.ClientId
            }).ToList();

            _logger.LogInformation("Retrieved {OrderCount} orders on page {PageNumber}", orderModels.Count(), pageNumber);

            return new PagedResult<OrderResponseModel>(orderModels, pagedOrders.TotalCount);
        }

        public async Task<OrderResponseModel> GetOrderById(int id)
        {
            _logger.LogInformation("Starting order search with ID {Id}", id);
            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                _logger.LogError("Order not found by ID: {Id}", id);
                throw new NotFoundException($"Order not found by ID: {id}");
            }

            _logger.LogInformation("Order found by ID: {OrderId}", order.Id);
            return new OrderResponseModel()
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalValue = order.TotalValue,
                ClientId = order.ClientId,
                Status = order.Status
            };
        }

        public async Task<OrderResponseModel> CreateOrder(OrderRequestModel orderRequestModel)
        {
            _logger.LogInformation("Starting order creation with request data: {OrderRequest}", orderRequestModel);
            var order = new Order
            {
                OrderDate = orderRequestModel.OrderDate,
                TotalValue = orderRequestModel.TotalValue,
                Status = orderRequestModel.Status,
                ClientId = orderRequestModel.ClientId
            };

            var validationResult = _orderValidator.Validate(order);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Order creation failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _orderRepository.CreateAsync(order);

            _logger.LogInformation("Order created with ID: {OrderId}", order.Id);
            return new OrderResponseModel()
            {
                OrderId = order.Id,
                OrderDate = order.OrderDate,
                TotalValue = order.TotalValue,
                ClientId = order.ClientId,
                Status = order.Status
            };
        }

        public async Task UpdateOrder(int id, OrderRequestModel orderRequestModel)
        {
            _logger.LogInformation("Starting order update with request data: {OrderRequest}", orderRequestModel);

            _logger.LogInformation("Starting order search with ID {Id}", id);
            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                _logger.LogError("Order not found by ID: {Id}", id);
                throw new NotFoundException($"Order not found by ID: {id}");
            }

            _logger.LogInformation("Order found by ID: {OrderId}", order.Id);

            order.OrderDate = orderRequestModel.OrderDate;
            order.TotalValue = orderRequestModel.TotalValue;
            order.Status = orderRequestModel.Status;
            order.ClientId = orderRequestModel.ClientId;

            var validationResult = _orderValidator.Validate(order);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Order update failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _orderRepository.UpdateAsync(order);
            _logger.LogInformation("Order updated with ID: {OrderId}", id);
        }

        public async Task DeleteOrder(int id)
        {
            _logger.LogInformation("Deleting order with ID: {Id}", id);
            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                _logger.LogError("Order not found by ID: {Id}", id);
                throw new NotFoundException($"Order not found by ID: {id}");
            }

            order.Status = OrderStatus.Canceled;

            await _orderRepository.UpdateAsync(order);
            _logger.LogInformation("Order status updated to 'Canceled' with ID: {OrderId}", order.Id);

            await _orderRepository.DeleteAsync(order);
            _logger.LogInformation("Order deleted with ID: {ProductId}", id);
        }
    }
}
