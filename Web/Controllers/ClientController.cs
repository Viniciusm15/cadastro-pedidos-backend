using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Infra.Data;
using Infra.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController(ApplicationDbContext _context) : ControllerBase
    {
        /// <summary>
        /// Retorna todos os clientes
        /// </summary>
        /// <returns>Uma lista de clientes.</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients
                .WhereActive()
                .OrderBy(client => client.Name)
                .Include(client => client.Orders)
                .ToListAsync();
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
        public async Task<ActionResult<Client>> GetClientById(int id)
        {
            var client = await _context.Clients
                .WhereActive()
                .Include(client => client.Orders)
                .FirstOrDefaultAsync(client => client.Id == id);

            if (client == null)
            {
                return NotFound("Client not found by ID: " + id + ". Please try again.");
            }

            return client;
        }

        /// <summary>
        /// Adiciona um novo cliente
        /// </summary>
        /// <param name="clientRequestModel">Novo cliente a ser adicionado.</param>
        /// <returns>Novo cliente adicionado.</returns>
        /// <response code="201">Cliente adicionado com sucesso.</response>
        [HttpPost]
        [ProducesResponseType(201)]
        public async Task<ActionResult<Client>> PostClient(ClientRequestModel clientRequestModel)
        {
            var client = new Client()
            {
                Name = clientRequestModel.Name,
                Email = clientRequestModel.Email,
                Telephone = clientRequestModel.Telephone,
                BirthDate = clientRequestModel.BirthDate
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClientById", new { id = client.Id }, client);
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
            var client = await _context.Clients
                .Where(c => c.IsActive)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound("Client not found by ID: " + id);
            }

            client.Name = clientRequestModel.Name;
            client.Email = clientRequestModel.Email;
            client.Telephone = clientRequestModel.Telephone;
            client.BirthDate = clientRequestModel.BirthDate;

            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
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
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound("Client not found by ID: " + id + ". Please try again.");
            }

            client.IsActive = false;
            client.DeletedAt = DateTime.UtcNow;

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
