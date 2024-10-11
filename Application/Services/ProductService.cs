using Application.Interfaces;
using Common.Exceptions;
using Common.Models;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IValidator<Product> _productValidator;

        public ProductService(ILogger<ProductService> logger, IProductRepository productRepository, IValidator<Product> productValidator)
        {
            _logger = logger;
            _productRepository = productRepository;
            _productValidator = productValidator;
        }

        public async Task<PagedResult<ProductResponseModel>> GetAllProducts(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Retrieving products for page {PageNumber} with size {PageSize}", pageNumber, pageSize);

            var pagedProducts = await _productRepository.GetAllProductsAsync(pageNumber, pageSize);

            var productModels = pagedProducts.Items.Select(product => new ProductResponseModel
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId
            }).ToList();

            _logger.LogInformation("Retrieved {ProductCount} products on page {PageNumber}", productModels.Count, pageNumber);

            return new PagedResult<ProductResponseModel>(productModels, pagedProducts.TotalCount);
        }

        public async Task<ProductResponseModel> GetProductById(int id)
        {
            _logger.LogInformation("Starting product search with ID {Id}", id);
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                _logger.LogError("Product not found by ID: {Id}", id);
                throw new NotFoundException($"Product not found by ID: {id}");
            }

            _logger.LogInformation("Product found by ID: {ProductId}", product.Id);
            return new ProductResponseModel
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId
            };
        }

        public async Task<ProductResponseModel> CreateProduct(ProductRequestModel productRequestModel)
        {
            _logger.LogInformation("Starting product creation with request data: {ProductRequest}", productRequestModel);

            var product = new Product
            {
                Name = productRequestModel.Name,
                Description = productRequestModel.Description,
                Price = productRequestModel.Price,
                StockQuantity = productRequestModel.StockQuantity,
                CategoryId = productRequestModel.CategoryId,
            };

            var validationResult = _productValidator.Validate(product);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Product creation failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _productRepository.CreateAsync(product);

            _logger.LogInformation("Product created with ID: {ProductId}", product.Id);
            return new ProductResponseModel
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId
            };
        }

        public async Task UpdateProduct(int id, ProductRequestModel productRequestModel)
        {
            _logger.LogInformation("Starting product update with request data: {ProductRequest}", productRequestModel);

            _logger.LogInformation("Starting product search with ID {Id}", id);
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                _logger.LogError("Product not found by ID: {Id}", id);
                throw new NotFoundException($"Product not found by ID: {id}");
            }

            _logger.LogInformation("Product found by ID: {ProductId}", product.Id);

            product.Name = productRequestModel.Name;
            product.Description = productRequestModel.Description;
            product.Price = productRequestModel.Price;
            product.StockQuantity = productRequestModel.StockQuantity;
            product.CategoryId = productRequestModel.CategoryId;

            var validationResult = _productValidator.Validate(product);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Product update failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _productRepository.UpdateAsync(product);
            _logger.LogInformation("Product updated with ID: {ProductId}", id);
        }

        public async Task DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with ID: {Id}", id);

            _logger.LogInformation("Starting product search with ID {Id}", id);
            var product = await _productRepository.GetProductByIdAsync(id);

            if (product == null)
            {
                _logger.LogError("Product not found by ID: {Id}", id);
                throw new NotFoundException($"Product not found by ID: {id}");
            }

            _logger.LogInformation("Product found by ID: {ProductId}", product.Id);
            await _productRepository.DeleteAsync(product);
        }
    }
}
