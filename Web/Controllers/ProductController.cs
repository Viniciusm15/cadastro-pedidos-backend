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
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Retorna uma lista paginada de produtos.
        /// </summary>
        /// <param name="pageNumber">Número da página desejada (padrão é 1).</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão é 10).</param>
        /// <returns>Uma lista paginada de produtos com o total de itens.</returns>
        /// <response code="200">Retorna a lista de produtos.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ProductResponseModel>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<ProductResponseModel>>> GetProducts(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var pagedProducts = await _productService.GetAllProducts(pageNumber, pageSize);
                return Ok(pagedProducts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retorna um produto com o ID especificado.
        /// </summary>
        /// <param name="id">ID do produto a ser retornado.</param>
        /// <returns>Produto correspondente ao ID fornecido.</returns>
        /// <response code="200">Produto encontrado com sucesso.</response>
        /// <response code="404">Produto não encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponseModel), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductResponseModel>> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductById(id);
                return Ok(product);
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
        /// Adiciona um novo produto.
        /// </summary>
        /// <param name="productRequestModel">Modelo do produto a ser adicionado.</param>
        /// <returns>Produto adicionado.</returns>
        /// <response code="201">Produto adicionado com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponseModel), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductResponseModel>> PostProduct([FromForm] ProductRequestModel productRequestModel)
        {
            try
            {
                var product = await _productService.CreateProduct(productRequestModel);
                return CreatedAtAction(nameof(GetProductById), new { id = product.ProductId }, product);
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
        /// Atualiza um produto existente.
        /// </summary>
        /// <param name="id">ID do produto a ser atualizado.</param>
        /// <param name="productRequestModel">Dados atualizados do produto.</param>
        /// <returns>Retorna NoContent se a atualização for bem-sucedida.</returns>
        /// <response code="204">Produto atualizado com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="404">Produto não encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutProduct(int id, [FromForm] ProductRequestModel productRequestModel)
        {
            try
            {
                await _productService.UpdateProduct(id, productRequestModel);
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

        /// <summary>
        /// Deleta um produto com o ID especificado.
        /// </summary>
        /// <param name="id">ID do produto a ser deletado.</param>
        /// <returns>Nenhum conteúdo.</returns>
        /// <response code="204">Produto deletado com sucesso.</response>
        /// <response code="404">Produto não encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteProductById(int id)
        {
            try
            {
                await _productService.DeleteProduct(id);
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
