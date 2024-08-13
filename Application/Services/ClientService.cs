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
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IValidator<Client> _clientValidator;

        public ClientService(ApplicationDbContext context, IValidator<Client> clientValidator)
        {
            _context = context;
            _clientValidator = clientValidator;
        }

        public async Task<IEnumerable<Client>> GetAllClients()
        {
            return await _context.Clients
                .WhereActive()
                .OrderBy(client => client.Name)
                .Include(client => client.Orders)
                .ToListAsync();
        }

        public async Task<Client> GetClientById(int id)
        {
            var client = await _context.Clients
                .WhereActive()
                .Include(client => client.Orders)
                .FirstOrDefaultAsync(client => client.Id == id);

            return client ?? throw new NotFoundException($"Client not found by ID: {id}");
        }

        public async Task<Client> CreateClient(ClientRequestModel clientRequestModel)
        {
            var client = new Client
            {
                Name = clientRequestModel.Name,
                Email = clientRequestModel.Email,
                Telephone = clientRequestModel.Telephone,
                BirthDate = clientRequestModel.BirthDate
            };

            var validationResult = _clientValidator.Validate(client);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return client;
        }

        public async Task UpdateClient(int id, ClientRequestModel clientRequestModel)
        {
            var client = await GetClientById(id) ?? throw new NotFoundException($"Client not found by ID: {id}");

            client.Name = clientRequestModel.Name;
            client.Email = clientRequestModel.Email;
            client.Telephone = clientRequestModel.Telephone;
            client.BirthDate = clientRequestModel.BirthDate;

            var validationResult = _clientValidator.Validate(client);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            _context.Entry(client).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClient(int id)
        {
            var client = await GetClientById(id) ?? throw new NotFoundException($"Client not found by ID: {id}");

            client.IsActive = false;
            client.DeletedAt = DateTime.UtcNow;

            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }
    }
}
