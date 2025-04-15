using Application.Interfaces;
using Common.Exceptions;
using Common.Models;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Retorna uma lista paginada de categorias.
        /// </summary>
        /// <param name="pageNumber">Número da página desejada (padrão é 1).</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão é 10).</param>
        /// <returns>Uma lista paginada de categorias com o total de itens.</returns>
        /// <response code="200">Lista paginada de categorias retornada com sucesso.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<CategoryResponseModel>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<CategoryResponseModel>>> GetCategories(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var pagedCategories = await _categoryService.GetAllCategories(pageNumber, pageSize);
                return Ok(pagedCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retorna uma categoria com o ID especificado.
        /// </summary>
        /// <param name="id">ID da categoria a ser retornada.</param>
        /// <returns>Categoria correspondente ao ID fornecido.</returns>
        /// <response code="200">Categoria foi encontrada e retornada com sucesso.</response>
        /// <response code="404">Categoria com o ID fornecido não foi encontrada.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryResponseModel), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CategoryResponseModel>> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryById(id);
                return Ok(category);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Adiciona uma nova categoria.
        /// </summary>
        /// <param name="categoryRequestModel">Nova categoria a ser adicionada.</param>
        /// <returns>Nova categoria adicionada.</returns>
        /// <response code="201">Categoria adicionada com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponseModel), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CategoryResponseModel>> PostCategory([FromBody] CategoryRequestModel categoryRequestModel)
        {
            try
            {
                var category = await _categoryService.CreateCategory(categoryRequestModel);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.ValidationErrors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
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
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PutCategory(int id, [FromBody] CategoryRequestModel categoryRequestModel)
        {
            try
            {
                await _categoryService.UpdateCategory(id, categoryRequestModel);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.ValidationErrors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Deleta uma categoria com o ID especificado.
        /// </summary>
        /// <param name="id">ID da categoria a ser deletada.</param>
        /// <returns>Nenhum conteúdo.</returns>
        /// <response code="204">Categoria foi deletada com sucesso.</response>
        /// <response code="404">Categoria com o ID fornecido não foi encontrada.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            try
            {
                await _categoryService.DeleteCategory(id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
