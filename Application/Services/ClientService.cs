using Application.Interfaces;
using Common.Exceptions;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentValidation;

namespace Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IValidator<Client> _clientValidator;

        public ClientService(IClientRepository clientRepository, IValidator<Client> clientValidator)
        {
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
        }

        public async Task<IEnumerable<Client>> GetAllClients()
        {
            return await _clientRepository.GetAllClientsAsync();
        }

        public async Task<Client> GetClientById(int id)
        {
            return await _clientRepository.GetClientByIdAsync(id) ?? throw new NotFoundException($"Client not found by ID: {id}");
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

            await _clientRepository.CreateAsync(client);
            return client;
        }

        public async Task UpdateClient(int id, ClientRequestModel clientRequestModel)
        {
            var client = await GetClientById(id);

            client.Name = clientRequestModel.Name;
            client.Email = clientRequestModel.Email;
            client.Telephone = clientRequestModel.Telephone;
            client.BirthDate = clientRequestModel.BirthDate;

            var validationResult = _clientValidator.Validate(client);

            if (!validationResult.IsValid)
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            await _clientRepository.UpdateAsync(client);
        }

        public async Task DeleteClient(int id)
        {
            var client = await GetClientById(id);
            await _clientRepository.DeleteAsync(client);
        }
    }
}
