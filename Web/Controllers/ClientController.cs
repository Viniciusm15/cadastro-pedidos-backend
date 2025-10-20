using Application.Interfaces;
using Common.Exceptions;
using Common.Models;
using Domain.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
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
    }
}
