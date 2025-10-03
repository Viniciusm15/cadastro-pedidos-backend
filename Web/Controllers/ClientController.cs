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
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        /// <summary>
        /// Retorna uma lista paginada de clientes.
        /// </summary>
        /// <param name="pageNumber">Número da página desejada (padrão é 1).</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão é 10).</param>
        /// <returns>Uma lista paginada de clientes com o total de itens.</returns>
        /// <response code="200">Lista paginada de clientes retornada com sucesso.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ClientResponseModel>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<ClientResponseModel>>> GetClients([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var pagedClients = await _clientService.GetAllClients(pageNumber, pageSize);
                return Ok(pagedClients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Retorna um cliente com o ID especificado.
        /// </summary>
        /// <param name="id">ID do cliente a ser retornado.</param>
        /// <returns>Cliente correspondente ao ID fornecido.</returns>
        /// <response code="200">Cliente foi encontrado e retornado com sucesso.</response>
        /// <response code="404">Cliente com o ID fornecido não foi encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClientResponseModel), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ClientResponseModel>> GetClientById(int id)
        {
            try
            {
                var client = await _clientService.GetClientById(id);
                return Ok(client);
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
        /// Adiciona um novo cliente.
        /// </summary>
        /// <param name="clientRequestModel">Novo cliente a ser adicionado.</param>
        /// <returns>O cliente recém-criado.</returns>
        /// <response code="201">Cliente adicionado com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ClientResponseModel), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ClientResponseModel>> PostClient([FromBody] ClientRequestModel clientRequestModel)
        {
            try
            {
                var client = await _clientService.CreateClient(clientRequestModel);
                return CreatedAtAction(nameof(GetClientById), new { id = client.ClientId }, client);
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
        /// Atualiza um cliente existente.
        /// </summary>
        /// <param name="id">ID do cliente a ser atualizado.</param>
        /// <param name="clientRequestModel">Dados atualizados do cliente.</param>
        /// <returns>Retorna NoContent se a atualização for bem-sucedida.</returns>
        /// <response code="204">Cliente atualizado com sucesso.</response>
        /// <response code="400">Requisição inválida.</response>
        /// <response code="404">Cliente não encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PutClient(int id, [FromBody] ClientRequestModel clientRequestModel)
        {
            try
            {
                await _clientService.UpdateClient(id, clientRequestModel);
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
        /// Deleta um cliente com o ID especificado.
        /// </summary>
        /// <param name="id">ID do cliente a ser deletado.</param>
        /// <returns>Nenhum conteúdo se a exclusão for bem-sucedida.</returns>
        /// <response code="204">Cliente foi deletado com sucesso.</response>
        /// <response code="404">Cliente com o ID fornecido não foi encontrado.</response>
        /// <response code="500">Erro interno no servidor.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteClientById(int id)
        {
            try
            {
                await _clientService.DeleteClient(id);
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
