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
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<Product> _productValidator;

        public ProductService(ApplicationDbContext context, IValidator<Product> productValidator)
        {
            _context = context;
            _productValidator = productValidator;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _context.Products
                .WhereActive()
                .OrderBy(product => product.Name)
                .Include(product => product.Category)
                .Include(product => product.OrderItens)
                .ToListAsync();
        }

        public async Task<Product> GetProductById(int id)
        {
            var products = await _context.Products
                .WhereActive()
                .Include(product => product.Category)
                .Include(product => product.OrderItens)
                .FirstOrDefaultAsync(product => product.Id == id);

            return products ?? throw new NotFoundException($"Product not found by ID: {id}");
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

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task UpdateProduct(int id, ProductRequestModel productRequestModel)
        {
            var product = await _context.Products.FindAsync(id) ?? throw new NotFoundException($"Product not found by ID: {id}");

            product.Name = productRequestModel.Name;
            product.Description = productRequestModel.Description;
            product.Price = productRequestModel.Price;
            product.StockQuantity = productRequestModel.StockQuantity;
            product.CategoryId = productRequestModel.CategoryId;

            var validationResult = _productValidator.Validate(product);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id) ?? throw new NotFoundException($"Product not found by ID: {id}");

            product.IsActive = false;
            product.DeletedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}
