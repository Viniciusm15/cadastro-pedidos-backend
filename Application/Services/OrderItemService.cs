using Application.Interfaces;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly ILogger<OrderItemService> _logger;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductService _productService;
        private readonly IValidator<OrderItem> _orderItemValidator;

        public OrderItemService(ILogger<OrderItemService> logger, IOrderItemRepository orderItemRepository, IProductService productService, IValidator<OrderItem> orderItemValidator)
        {
            _logger = logger;
            _orderItemRepository = orderItemRepository;
            _orderItemValidator = orderItemValidator;
            _productService = productService;
        }

        public async Task<IEnumerable<OrderItemResponseModel>> GetOrderItemsByOrderId(int orderId)
        {
            _logger.LogInformation("Retrieving all order itens");
            var orderItems = await _orderItemRepository.GetByOrderIdAsync(orderId);

            _logger.LogInformation("Retrieved {OrderItens} order itens", orderItems.Count());
            return orderItems.Select(orderItem => new OrderItemResponseModel
            {
                OrderId = orderItem.OrderId,
                Quantity = orderItem.Quantity,
                UnitaryPrice = orderItem.UnitaryPrice,
                Subtotal = orderItem.Subtotal,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.Product.Name,
                OrderItemId = orderItem.Id
            });
        }

        public async Task<OrderItemResponseModel> CreateOrderItem(OrderItemRequestModel orderItemRequestModel)
        {
            _logger.LogInformation("Starting order item creation with request data: {OrderRequest}", orderItemRequestModel);
            var orderItem = new OrderItem
            {
                OrderId = orderItemRequestModel.OrderId,
                ProductId = orderItemRequestModel.ProductId,
                Quantity = orderItemRequestModel.Quantity,
                UnitaryPrice = orderItemRequestModel.UnitaryPrice,
            };

            var validationResult = _orderItemValidator.Validate(orderItem);
            if (!validationResult.IsValid)
            {
                _logger.LogError("Order item creation failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var product = await _productService.GetProductById(orderItemRequestModel.ProductId);
            await _orderItemRepository.CreateAsync(orderItem);

            _logger.LogInformation("Order item created with ID: {OrderItemId}", orderItem.Id);
            return new OrderItemResponseModel()
            {
                OrderId = orderItem.OrderId,
                Quantity = orderItem.Quantity,
                UnitaryPrice = orderItem.UnitaryPrice,
                Subtotal = orderItem.Subtotal,
                ProductId = orderItem.ProductId,
                ProductName = product.Name,
                OrderItemId = orderItem.OrderId
            };
        }

        public async Task UpdateOrderItem(int orderItemId, OrderItemRequestModel orderItemRequestModel)
        {
            _logger.LogInformation("Starting order item update with request data: {OrderItemRequest}", orderItemRequestModel);

            _logger.LogInformation("Starting order item search with ID {Id}", orderItemId);
            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId);

            if (orderItem == null)
            {
                _logger.LogError("Order item not found by ID: {Id}", orderItemId);
                throw new NotFoundException($"Order item not found by ID: {orderItemId}");
            }

            _logger.LogInformation("Order item found by ID: {OrderItemId}", orderItem.Id);

            orderItem.Quantity = orderItemRequestModel.Quantity;
            orderItem.UnitaryPrice = orderItemRequestModel.UnitaryPrice;

            var validationResult = _orderItemValidator.Validate(orderItem);
            if (!validationResult.IsValid)
            {
                _logger.LogError("Order item creation failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _orderItemRepository.UpdateAsync(orderItem);
            _logger.LogInformation("Order item updated with ID: {OrderItemId}", orderItem.Id);
        }

        public async Task SyncOrderItems(int orderId, IEnumerable<OrderItemRequestModel> itemRequests)
        {
            _logger.LogInformation("Starting sync of order items for Order ID: {OrderId}", orderId);

            var existingItems = await _orderItemRepository.GetByOrderIdAsync(orderId);
            foreach (var item in itemRequests)
            {
                var existingItem = existingItems.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem != null)
                    await UpdateOrderItem(existingItem.Id, item);
                else
                    await CreateOrderItem(item);
            }

            _logger.LogInformation("Order items synced for Order ID: {OrderId}", orderId);
        }

        public async Task DeleteOrderItem(int id)
        {
            _logger.LogInformation("Deleting order item with ID: {OrderItemId}", id);
            var orderItem = await _orderItemRepository.GetByIdAsync(id);

            if (orderItem == null)
            {
                _logger.LogError("Order item not found by ID: {Id}", id);
                throw new NotFoundException($"Order item not found by ID: {id}");
            }

            await _orderItemRepository.DeleteAsync(orderItem);
            _logger.LogInformation("Deleted order item ID: {OrderItemId}", id);
        }
    }
}
