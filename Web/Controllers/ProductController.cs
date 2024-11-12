using Application.Interfaces;
using Common.Exceptions;
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
        /// Retorna todos os produtos.
        /// </summary>
        /// <returns>Uma lista de produtos.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<ProductResponseModel>>> GetProducts(int pageNumber = 1, int pageSize = 10)
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
        /// <response code="200">Produto foi encontrado e retornado com sucesso.</response>
        /// <response code="404">Produto com o ID fornecido não foi encontrado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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
        /// <param name="productRequestModel">Novo produto a ser adicionado.</param>
        /// <returns>Produto adicionado.</returns>
        /// <response code="201">Produto adicionado com sucesso.</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductResponseModel>> PostProduct([FromForm] ProductRequestModel productRequestModel)
        {
            try
            {
                var product = await _productService.CreateProduct(productRequestModel);
                return CreatedAtAction("GetProductById", new { id = product.ProductId }, product);
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

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutProduct(int id, ProductRequestModel productRequestModel)
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
        /// <response code="204">Produto foi deletado com sucesso.</response>
        /// <response code="404">Produto com o ID fornecido não foi encontrado.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
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
