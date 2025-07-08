using Application.Interfaces;
using Common.Exceptions;
using Common.Helpers;
using Common.Models;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.Reports;
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
        private readonly IOrderItemService _orderItemService;
        private readonly ICsvService _csvService;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(ILogger<OrderService> logger, IOrderRepository orderRepository, IValidator<Order> orderValidator, IOrderItemService orderItemService, ICsvService csvService, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _orderValidator = orderValidator;
            _orderItemService = orderItemService;
            _csvService = csvService;
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.BeginTransactionAsync();
            try
            {
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

                foreach (var itemRequest in orderRequestModel.OrderItems)
                {
                    itemRequest.OrderId = order.Id;
                    await _orderItemService.CreateOrderItem(itemRequest);
                }

                await _unitOfWork.CommitAsync();

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
            catch (Exception ex)
            {
                _logger.LogError("Order creation failed. Rolling back transaction. Error: {Error}", ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateOrder(int id, OrderRequestModel orderRequestModel)
        {
            _logger.LogInformation("Starting order update with request data: {OrderRequest}", orderRequestModel);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
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

                await _orderItemService.SyncOrderItems(order.Id, orderRequestModel.OrderItems);
                order = await _orderRepository.GetOrderByIdAsync(order.Id);
                order.TotalValue = order.OrderItems.Sum(i => i.Subtotal);

                var validationResult = _orderValidator.Validate(order);
                if (!validationResult.IsValid)
                {
                    _logger.LogError("Order update failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
                }

                await _unitOfWork.CommitAsync();

                await _orderRepository.UpdateAsync(order);
                _logger.LogInformation("Order updated with ID: {OrderId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError("Order update failed. Rolling back transaction. Error: {Error}", ex.Message);
                await _unitOfWork.RollbackAsync();
                throw;
            }
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

            if (order.OrderItems.Any())
            {
                foreach (var orderItem in order.OrderItems)
                {
                    await _orderItemService.DeleteOrderItem(orderItem.Id);
                    _logger.LogInformation("Order item with ID: {OrderItemId} deleted", orderItem.Id);
                }
            }

            order.Status = OrderStatus.Canceled;

            await _orderRepository.UpdateAsync(order);
            _logger.LogInformation("Order status updated to 'Canceled' with ID: {OrderId}", order.Id);

            await _orderRepository.DeleteAsync(order);
            _logger.LogInformation("Order deleted with ID: {ProductId}", id);
        }

        public async Task<byte[]> GenerateOrdersReportCsvAsync()
        {
            _logger.LogInformation("Starting orders CSV report generation");

            var orders = await _orderRepository.GetAllOrdersAsync();
            if (orders == null || !orders.Any())
            {
                _logger.LogWarning("No orders found to generate the report");
                return Array.Empty<byte>();
            }

            _logger.LogInformation("{OrderCount} orders fetched for the report", orders.Count());

            try
            {
                var reportData = orders.Select(order => new OrderReportModel
                {
                    OrderNumber = order.Id,
                    OrderDate = order.OrderDate.ToString("MM/dd/yyyy"),
                    ClientName = order.Client?.Name ?? "No Client",
                    Status = order.Status.ToString(),
                    TotalItems = order.OrderItems?.Sum(i => i.Quantity) ?? 0,
                    TotalValue = order.TotalValue
                }).ToList();

                _logger.LogInformation("CSV report data generated successfully. {ReportDataCount} items in the report", reportData.Count);
                return _csvService.WriteCsvToByteArray(reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while generating the CSV report: {Message}", ex.Message);
                return Array.Empty<byte>();
            }
        }
    }
}
