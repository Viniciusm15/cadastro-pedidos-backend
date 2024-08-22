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
        /// Retorna todos os clientes.
        /// </summary>
        /// <returns>Uma lista de clientes.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<PagedResult<ClientResponseModel>>> GetClients(int pageNumber = 1, int pageSize = 10)
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
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
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
        /// <returns>Novo cliente adicionado.</returns>
        /// <response code="201">Cliente adicionado com sucesso.</response>
        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<ActionResult<ClientResponseModel>> PostClient(ClientRequestModel clientRequestModel)
        {
            try
            {
                var client = await _clientService.CreateClient(clientRequestModel);
                return CreatedAtAction("GetClientById", new { id = client.ClientId }, client);
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
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PutClient(int id, ClientRequestModel clientRequestModel)
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Deleta um cliente com o ID especificado.
        /// </summary>
        /// <param name="id">ID do cliente a ser deletado.</param>
        /// <returns>Nenhum conteúdo.</returns>
        /// <response code="204">Cliente foi deletado com sucesso.</response>
        /// <response code="404">Cliente com o ID fornecido não foi encontrado.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
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
