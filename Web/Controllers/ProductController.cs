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
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<Product> _productValidator;

        public ProductController(ApplicationDbContext context, IValidator<Product> productValidator)
        {
            _context = context;
            _productValidator = productValidator;
        }

        /// <summary>
        /// Retorna todos os produtos
        /// </summary>
        /// <returns>Uma lista de produtos.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products
                .WhereActive()
                .OrderBy(product => product.Name)
                .Include(product => product.Category)
                .Include(product => product.OrderItens)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna um produto com o ID especificado.
        /// </summary>
        /// <param name="id">ID do produto a ser retornado.</param>
        /// <returns>Produto correspondente ao ID fornecido.</returns>
        /// <response code="200">Produto foi encontrado e retornado com sucesso.</response>
        /// <response code="404">Produto com o ID fornecido não foi encontrado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products
                .WhereActive()
                .Include(product => product.Category)
                .Include(product => product.OrderItens)
                .FirstOrDefaultAsync(product => product.Id == id);

            if (product == null)
                return NotFound("Product not found by ID: " + id + ". Please try again.");

            return product;
        }

        /// <summary>
        /// Adiciona um novo produto
        /// </summary>
        /// <param name="productRequestModel">Novo produto a ser adicionado.</param>
        /// <returns>Produto adicionado.</returns>
        /// <response code="201">Produto adicionado com sucesso.</response>
        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Product>> PostProduct(ProductRequestModel productRequestModel)
        {
            var product = new Product()
            {
                Name = productRequestModel.Name,
                Description = productRequestModel.Description,
                Price = productRequestModel.Price,
                StockQuantity = productRequestModel.StockQuantity,
                CategoryId = productRequestModel.CategoryId
            };

            var validationResult = _productValidator.Validate(product);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(errorMessages);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProductById", new { id = product.Id }, product);
        }

        /// <summary>
        /// Atualiza um produto existente.
        /// </summary>
        /// <param name="id">ID do produto a ser atualizado.</param>
        /// <param name="productRequestModel">Dados atualizados do produto.</param>
        /// <returns>Retorna NoContent se a atualização for bem-sucedida.</returns>
        /// <response code="204">Produto atualizado com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="404">Produto não encontrado.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutProduct(int id, ProductRequestModel productRequestModel)
        {
            var product = await _context.Products
                .Where(p => p.IsActive)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Product not found by ID: " + id);

            product.Name = productRequestModel.Name;
            product.Description = productRequestModel.Description;
            product.Price = productRequestModel.Price;
            product.StockQuantity = productRequestModel.StockQuantity;
            product.CategoryId = productRequestModel.CategoryId;

            var validationResult = _productValidator.Validate(product);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(errorMessages);
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deleta um produto com o ID especificado.
        /// </summary>
        /// <param name="id">ID do produto a ser deletado.</param>
        /// <returns>Nenhum conteúdo.</returns>
        /// <response code="204">Produto foi deletado com sucesso.</response>
        /// <response code="404">Produto com o ID fornecido não foi encontrado.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound("Product not found by ID: " + id + ". Please try again.");

            product.IsActive = false;
            product.DeletedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
