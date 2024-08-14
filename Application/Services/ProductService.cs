using Application.Interfaces;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentValidation;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IValidator<Product> _productValidator;

        public ProductService(IProductRepository productRepository, IValidator<Product> productValidator)
        {
            _productRepository = productRepository;
            _productValidator = productValidator;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _productRepository.GetProductsAsync();
        }

        public async Task<Product> GetProductById(int id)
        {
            return await _productRepository.GetProductByIdAsync(id) ?? throw new NotFoundException($"Product not found by ID: {id}");
        }

        public async Task<Product> CreateProduct(ProductRequestModel productRequestModel)
        {
            var product = new Product
            {
                Name = productRequestModel.Name,
                Description = productRequestModel.Description,
                Price = productRequestModel.Price,
                StockQuantity = productRequestModel.StockQuantity,
                CategoryId = productRequestModel.CategoryId
            };

            var validationResult = _productValidator.Validate(product);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            await _productRepository.CreateAsync(product);
            return product;
        }

        public async Task UpdateProduct(int id, ProductRequestModel productRequestModel)
        {
            var product = await GetProductById(id);

            product.Name = productRequestModel.Name;
            product.Description = productRequestModel.Description;
            product.Price = productRequestModel.Price;
            product.StockQuantity = productRequestModel.StockQuantity;
            product.CategoryId = productRequestModel.CategoryId;

            var validationResult = _productValidator.Validate(product);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProduct(int id)
        {
            var product = await GetProductById(id);
            await _productRepository.DeleteAsync(product);
        }
    }
}
