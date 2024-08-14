using Application.Interfaces;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using Domain.Models.ResponseModels;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ClientService : IClientService
    {
        private readonly ILogger<ClientService> _logger;
        private readonly IClientRepository _clientRepository;
        private readonly IValidator<Client> _clientValidator;

        public ClientService(ILogger<ClientService> logger, IClientRepository clientRepository, IValidator<Client> clientValidator)
        {
            _logger = logger;
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
        }

        public async Task<IEnumerable<ClientResponseModel>> GetAllClients()
        {
            _logger.LogInformation("Retrieving all clients");
            var clients = await _clientRepository.GetAllClientsAsync();

            _logger.LogInformation("Retrieved {ClientsCount} clients", clients.Count());
            return clients.Select(client => new ClientResponseModel
            {
                ClientId = client.Id,
                Name = client.Name,
                Email = client.Email,
                Telephone = client.Telephone,
                BirthDate = client.BirthDate
            });
        }

        public async Task<ClientResponseModel> GetClientById(int id)
        {
            _logger.LogInformation("Starting client search with ID {Id}", id);
            var client = await _clientRepository.GetClientByIdAsync(id);

            if (client == null)
            {
                _logger.LogError("Client not found by ID: {Id}", id);
                throw new NotFoundException($"Client not found by ID: {id}");
            }

            _logger.LogInformation("Client found by ID: {ClientId}", client.Id);
            return new ClientResponseModel
            {
                ClientId = client.Id,
                Name = client.Name,
                Email = client.Email,
                Telephone = client.Telephone,
                BirthDate = client.BirthDate
            };
        }

        public async Task<ClientResponseModel> CreateClient(ClientRequestModel clientRequestModel)
        {
            _logger.LogInformation("Starting client creation with request data: {ClientRequest}", clientRequestModel);
            var client = new Client
            {
                Name = clientRequestModel.Name,
                Email = clientRequestModel.Email,
                Telephone = clientRequestModel.Telephone,
                BirthDate = clientRequestModel.BirthDate
            };

            var validationResult = _clientValidator.Validate(client);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Client creation failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _clientRepository.CreateAsync(client);

            _logger.LogInformation("Client created with ID: {ClientId}", client.Id);
            return new ClientResponseModel
            {
                ClientId = client.Id,
                Name = client.Name,
                Email = client.Email,
                Telephone = client.Telephone,
                BirthDate = client.BirthDate
            };
        }

        public async Task UpdateClient(int id, ClientRequestModel clientRequestModel)
        {
            _logger.LogInformation("Starting client update with request data: {ClientRequest}", clientRequestModel);

            _logger.LogInformation("Starting client search with ID {Id}", id);
            var client = await _clientRepository.GetClientByIdAsync(id);

            if (client == null)
            {
                _logger.LogError("Client not found by ID: {Id}", id);
                throw new NotFoundException($"Client not found by ID: {id}");
            }

            _logger.LogInformation("Client found by ID: {ClientId}", client.Id);

            client.Name = clientRequestModel.Name;
            client.Email = clientRequestModel.Email;
            client.Telephone = clientRequestModel.Telephone;
            client.BirthDate = clientRequestModel.BirthDate;

            var validationResult = _clientValidator.Validate(client);

            if (!validationResult.IsValid)
            {
                _logger.LogError("Client update failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _clientRepository.UpdateAsync(client);
            _logger.LogInformation("Client updated with ID: {ClientId}", id);
        }

        public async Task DeleteClient(int id)
        {
            _logger.LogInformation("Deleting client with ID: {Id}", id);

            _logger.LogInformation("Starting client search with ID {Id}", id);
            var client = await _clientRepository.GetClientByIdAsync(id);

            if (client == null)
            {
                _logger.LogError("Client not found by ID: {Id}", id);
                throw new NotFoundException($"Client not found by ID: {id}");
            }

            _logger.LogInformation("Client found by ID: {ClientId}", client.Id);
            await _clientRepository.DeleteAsync(client);
        }
    }
}
