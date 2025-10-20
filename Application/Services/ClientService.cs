using Application.Interfaces;
using Common.Exceptions;
using Common.Models;
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

        public ClientService(ILogger<ClientService> logger,
            IClientRepository clientRepository,
            IValidator<Client> clientValidator)
        {
            _logger = logger;
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
        }

        public async Task<PagedResult<ClientResponseModel>> GetAllClients(int pageNumber, int pageSize)
        {
            _logger.LogInformation("Retrieving clients for page {PageNumber} with size {PageSize}", pageNumber, pageSize);
            var pagedClients = await _clientRepository.GetAllClientsAsync(pageNumber, pageSize);

            var clientModels = pagedClients.Items.Select(client => new ClientResponseModel
            {
                ClientId = client.Id,
                ApplicationUserId = client.ApplicationUserId,
                Name = client.Name,
                Email = client.Email,
                Telephone = client.Telephone,
                BirthDate = client.BirthDate,
                PurchaseHistory = client.Orders.Select(order => new OrderResponseModel
                {
                    OrderId = order.Id,
                    OrderDate = order.OrderDate,
                    TotalValue = order.TotalValue,
                    ClientId = client.Id,
                }).ToList()
            }).ToList();

            _logger.LogInformation("Retrieved {ClientsCount} clients on page {PageNumber}", clientModels.Count, pageNumber);

            return new PagedResult<ClientResponseModel>(clientModels, pagedClients.TotalCount);
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
                ApplicationUserId = client.ApplicationUserId,
                Name = client.Name,
                Email = client.Email,
                Telephone = client.Telephone,
                BirthDate = client.BirthDate,
                PurchaseHistory = client.Orders.Select(order => new OrderResponseModel
                {
                    OrderId = order.Id,
                    OrderDate = order.OrderDate,
                    TotalValue = order.TotalValue,
                    ClientId = client.Id,
                }).ToList()
            };
        }

        public async Task<ClientResponseModel> GetClientByApplicationUserIdAsync(string applicationUserId)
        {
            _logger.LogInformation("Searching client by ApplicationUserId: {ApplicationUserId}", applicationUserId);
            var client = await _clientRepository.GetClientByApplicationUserIdAsync(applicationUserId);

            if (client == null)
            {
                _logger.LogError("Client not found for ApplicationUserId: {ApplicationUserId}", applicationUserId);
                throw new NotFoundException($"Client not found for ApplicationUserId: {applicationUserId}");
            }

            _logger.LogInformation("Client found for ApplicationUserId: {ApplicationUserId}, ClientId: {ClientId}",
                applicationUserId, client.Id);

            return new ClientResponseModel
            {
                ClientId = client.Id,
                ApplicationUserId = client.ApplicationUserId,
                Name = client.Name,
                Email = client.Email,
                Telephone = client.Telephone,
                BirthDate = client.BirthDate,
                PurchaseHistory = []
            };
        }

        public async Task<ClientResponseModel> CreateClient(ClientRequestModel clientRequestModel, string userId)
        {
            _logger.LogInformation("Starting client creation with request data: {ClientRequest}", clientRequestModel);

            var client = new Client
            {
                Name = clientRequestModel.Name,
                Email = clientRequestModel.Email,
                Telephone = clientRequestModel.Telephone,
                BirthDate = clientRequestModel.BirthDate,
                ApplicationUserId = userId
            };

            var validationResult = _clientValidator.Validate(client);
            if (!validationResult.IsValid)
            {
                _logger.LogError("Client creation failed due to validation errors: {ValidationErrors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _clientRepository.CreateAsync(client);

            _logger.LogInformation("Client created with ID: {ClientId}, ApplicationUserId: {ApplicationUserId}",
                client.Id, client.ApplicationUserId);

            return new ClientResponseModel
            {
                ClientId = client.Id,
                ApplicationUserId = client.ApplicationUserId,
                Name = client.Name,
                Email = client.Email,
                Telephone = client.Telephone,
                BirthDate = client.BirthDate,
                PurchaseHistory = []
            };
        }

        public async Task UpdateClient(int id, ClientRequestModel clientRequestModel, bool reactivateIfInactive = false)
        {
            _logger.LogInformation("Starting client update with request data: {ClientRequest}", clientRequestModel);

            var client = await _clientRepository.GetClientByIdAsync(id, includeInactive: reactivateIfInactive);
            if (client == null)
            {
                _logger.LogError("Client not found by ID: {Id}", id);
                throw new NotFoundException($"Client not found by ID: {id}");
            }

            _logger.LogInformation("Client found by ID: {ClientId}", client.Id);

            if (reactivateIfInactive && !client.IsActive)
            {
                _logger.LogInformation("Reactivating inactive client. ClientId: {ClientId}", id);
                client.IsActive = true;
            }

            client.Name = clientRequestModel.Name;
            client.Email = clientRequestModel.Email;
            client.Telephone = clientRequestModel.Telephone;
            client.BirthDate = clientRequestModel.BirthDate;

            var validationResult = _clientValidator.Validate(client);
            if (!validationResult.IsValid)
            {
                _logger.LogError("Client update failed due to validation errors: {ValidationErrors}",
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                throw new Common.Exceptions.ValidationException(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            await _clientRepository.UpdateAsync(client);
            _logger.LogInformation("Client updated with ID: {ClientId}", id);
        }

        public async Task DeleteClient(int id)
        {
            _logger.LogInformation("Deleting client with ID: {Id}", id);
            var client = await _clientRepository.GetClientByIdAsync(id);

            if (client == null)
            {
                _logger.LogError("Client not found by ID: {Id}", id);
                throw new NotFoundException($"Client not found by ID: {id}");
            }

            await _clientRepository.DeleteAsync(client);
            _logger.LogInformation("Client deleted with ID: {Id}", id);
        }

        public async Task<int> GetActiveClientsCountAsync(int months = 6)
        {
            _logger.LogInformation("Retrieving active clients count for last {Months} months", months);
            var activeClientsCount = await _clientRepository.GetActiveClientsCountAsync(months);

            _logger.LogInformation("Retrieved {ActiveClientsCount} active clients for last {Months} months",
                activeClientsCount, months);

            return activeClientsCount;
        }

        public async Task<int> GetNewClientsThisMonthAsync()
        {
            var now = DateTime.Now;
            _logger.LogInformation("Retrieving new clients count for {Month}/{Year}", now.Month, now.Year);

            var newClientsCount = await _clientRepository.GetNewClientsCountAsync(now.Month, now.Year);

            _logger.LogInformation("Retrieved {NewClientsCount} new clients for {Month}/{Year}",
                newClientsCount, now.Month, now.Year);

            return newClientsCount;
        }

        public async Task<DashboardClientSummaryResponseModel> GetClientDataAsync()
        {
            _logger.LogInformation("Starting client data aggregation");

            var currentDate = DateTime.Now;
            var totalClients = await _clientRepository.GetTotalClientsCountAsync();
            var newClientsThisMonth = await _clientRepository.GetNewClientsCountAsync(currentDate.Month, currentDate.Year);
            var (monthlyData, monthlyLabels) = await GetMonthlyClientDataAsync(6);
            var retentionRate = await CalculateRetentionRateAsync();

            _logger.LogInformation("Successfully aggregated client data: " +
                "TotalClients={TotalClients}, NewThisMonth={NewClientsThisMonth}, Retention={RetentionRate}%",
                totalClients, newClientsThisMonth, retentionRate);

            return new DashboardClientSummaryResponseModel
            {
                TotalClients = totalClients,
                NewClientsThisMonth = newClientsThisMonth,
                RetentionRate = retentionRate,
                MonthlyData = monthlyData,
                MonthlyLabels = monthlyLabels
            };
        }

        private async Task<(List<int> Data, List<string> Labels)> GetMonthlyClientDataAsync(int months)
        {
            _logger.LogInformation("Retrieving monthly client data for last {Months} months", months);

            var monthlyData = new List<int>();
            var monthlyLabels = new List<string>();
            var currentDate = DateTime.Now;

            for (int i = months - 1; i >= 0; i--)
            {
                var date = currentDate.AddMonths(-i);
                var monthStart = new DateTime(date.Year, date.Month, 1);
                var monthEnd = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

                var count = await _clientRepository.GetClientsCountUntilDateAsync(monthStart, monthEnd);
                monthlyData.Add(count);
                monthlyLabels.Add(date.ToString("MMM"));
            }

            _logger.LogInformation("Completed monthly data retrieval. Retrieved {MonthCount} months of data", monthlyData.Count);
            return (monthlyData, monthlyLabels);
        }

        private async Task<int> CalculateRetentionRateAsync()
        {
            _logger.LogInformation("Calculating client retention rate");

            var activeClients = await _clientRepository.GetActiveClientsCountAsync(6);
            var totalClients = await _clientRepository.GetTotalClientsCountAsync();

            if (totalClients == 0)
            {
                _logger.LogWarning("No clients found - cannot calculate retention rate");
                return 0;
            }

            var retentionRate = (int)Math.Round((double)activeClients / totalClients * 100);

            _logger.LogInformation("Calculated retention rate: {RetentionRate}% (Active: {ActiveClients}, Total: {TotalClients})",
                retentionRate, activeClients, totalClients);

            return retentionRate;
        }
    }
}
