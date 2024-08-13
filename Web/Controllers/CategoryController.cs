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
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<Category> _categoryValidator;

        public CategoryController(ApplicationDbContext context, IValidator<Category> categoryValidator)
        {
            _context = context;
            _categoryValidator = categoryValidator;
        }

        /// <summary>
        /// Retorna todas as categorias
        /// </summary>
        /// <returns>Uma lista de categorias.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .WhereActive()
                .OrderBy(category => category.Name)
                .Include(category => category.Products)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna uma categoria com o ID especificado.
        /// </summary>
        /// <param name="id">ID da categoria a ser retornada.</param>
        /// <returns>Categoria correspondente ao ID fornecido.</returns>
        /// <response code="200">Categoria foi encontrada e retornada com sucesso.</response>
        /// <response code="404">Categoria com o ID fornecido não foi encontrada.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            var category = await _context.Categories
                .WhereActive()
                .Include(category => category.Products)
                .FirstOrDefaultAsync(category => category.Id == id);

            if (category == null)
                return NotFound("Category not found by ID: " + id + ". Please try again.");

            return category;
        }

        /// <summary>
        /// Adiciona uma nova categoria
        /// </summary>
        /// <param name="categoryRequestModel">Nova categoria a ser adicionada.</param>
        /// <returns>Nova categoria adicionada.</returns>
        /// <response code="201">Categoria adicionada com sucesso.</response>
        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Category>> PostCategory(CategoryRequestModel categoryRequestModel)
        {
            var category = new Category()
            {
                Name = categoryRequestModel.Name,
                Description = categoryRequestModel.Description
            };

            var validationResult = _categoryValidator.Validate(category);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(errorMessages);
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategoryById", new { id = category.Id }, category);
        }

        /// <summary>
        /// Atualiza uma categoria existente.
        /// </summary>
        /// <param name="id">ID da categoria a ser atualizada.</param>
        /// <param name="categoryRequestModel">Dados atualizados da categoria.</param>
        /// <returns>Retorna NoContent se a atualização for bem-sucedida.</returns>
        /// <response code="204">Categoria atualizada com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="404">Categoria não encontrada.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutCategory(int id, CategoryRequestModel categoryRequestModel)
        {
            var category = await _context.Categories
                .Where(c => c.IsActive)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound("Category not found by ID: " + id);

            category.Name = categoryRequestModel.Name;
            category.Description = categoryRequestModel.Description;

            var validationResult = _categoryValidator.Validate(category);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage);
                return BadRequest(errorMessages);
            }

            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deleta uma categoria com o ID especificado.
        /// </summary>
        /// <param name="id">ID da categoria a ser deletada.</param>
        /// <returns>Nenhum conteúdo.</returns>
        /// <response code="204">Categoria foi deletada com sucesso.</response>
        /// <response code="404">Categoria com o ID fornecido não foi encontrada.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
                return NotFound("Category not found by ID: " + id + ". Please try again.");

            category.IsActive = false;
            category.DeletedAt = DateTime.UtcNow;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
