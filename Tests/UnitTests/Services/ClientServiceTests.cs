using Application.Services;
using Common.Exceptions;
using Common.Models;
using Domain.Interfaces;
using Domain.Models.Entities;
using Domain.Models.RequestModels;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.UnitTests.Services
{
    public class ClientServiceTests
    {
        private readonly Mock<ILogger<ClientService>> _loggerMock;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IValidator<Client>> _clientValidatorMock;
        private readonly ClientService _clientService;

        public ClientServiceTests()
        {
            _loggerMock = new Mock<ILogger<ClientService>>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _clientValidatorMock = new Mock<IValidator<Client>>();

            _clientService = new ClientService(
                _loggerMock.Object,
                _clientRepositoryMock.Object,
                _clientValidatorMock.Object);
        }

        [Fact]
        public async Task GetAllClients_ShouldReturnPagedResult_WhenClientsExist()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 10;
            var clients = new List<Client>
            {
                new() {
                    Id = 1,
                    Name = "Client 1",
                    Email = "client1@test.com",
                    Telephone = "123456789",
                    BirthDate = new DateTime(1990, 1, 1),
                    Orders = []
                },
                new() {
                    Id = 2,
                    Name = "Client 2",
                    Email = "client2@test.com",
                    Telephone = "987654321",
                    BirthDate = new DateTime(1995, 1, 1),
                    Orders = []
                }
            };

            var pagedResult = new PagedResult<Client>(clients, 2);

            _clientRepositoryMock
                .Setup(x => x.GetAllClientsAsync(pageNumber, pageSize))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _clientService.GetAllClients(pageNumber, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items.First().Name.Should().Be("Client 1");
            result.Items.Last().Name.Should().Be("Client 2");
            result.Items.First().PurchaseHistory.Should().BeEmpty();

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Retrieving clients for page {pageNumber} with size {pageSize}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllClients_ShouldIncludePurchaseHistory_WhenClientHasOrders()
        {
            // Arrange
            var client = new Client
            {
                Id = 1,
                Name = "Client 1",
                Email = "client1@test.com",
                Telephone = "123456789",
                Orders =
                [
                    new Order { Id = 1, OrderDate = DateTime.Now, TotalValue = 100.50 }
                ]
            };

            var pagedResult = new PagedResult<Client>([client], 1);

            _clientRepositoryMock
                .Setup(x => x.GetAllClientsAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _clientService.GetAllClients(1, 10);

            // Assert
            result.Items.First().PurchaseHistory.Should().HaveCount(1);
            result.Items.First().PurchaseHistory.First().TotalValue.Should().Be(100.50);
        }

        [Fact]
        public async Task GetClientById_ShouldReturnClient_WhenClientExists()
        {
            // Arrange
            var clientId = 1;
            var client = new Client
            {
                Id = clientId,
                Name = "Test Client",
                Email = "test@client.com",
                Telephone = "123456789",
                BirthDate = new DateTime(1985, 5, 15),
                Orders =
                [
                    new Order { Id = 1, OrderDate = DateTime.Now, TotalValue = 100.50 }
                ]
            };

            _clientRepositoryMock
                .Setup(x => x.GetClientByIdAsync(clientId))
                .ReturnsAsync(client);

            // Act
            var result = await _clientService.GetClientById(clientId);

            // Assert
            result.Should().NotBeNull();
            result.ClientId.Should().Be(clientId);
            result.Name.Should().Be(client.Name);
            result.Email.Should().Be(client.Email);
            result.Telephone.Should().Be(client.Telephone);
            result.BirthDate.Should().Be(client.BirthDate);
            result.PurchaseHistory.Count.Should().Be(1);
            result.PurchaseHistory.First().TotalValue.Should().Be(100.50);
        }

        [Fact]
        public async Task GetClientById_ShouldThrowNotFoundException_WhenClientDoesNotExist()
        {
            // Arrange
            var clientId = 999;

            _clientRepositoryMock
                .Setup(x => x.GetClientByIdAsync(clientId))
                .ReturnsAsync((Client)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _clientService.GetClientById(clientId));

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Client not found by ID: {clientId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateClient_ShouldReturnCreatedClient_WhenValidationPasses()
        {
            // Arrange
            var requestModel = new ClientRequestModel
            {
                Name = "New Client",
                Email = "new@client.com",
                Telephone = "987654321",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var createdClient = new Client
            {
                Id = 1,
                Name = requestModel.Name,
                Email = requestModel.Email,
                Telephone = requestModel.Telephone,
                BirthDate = requestModel.BirthDate,
                Orders =
                [
                    new Order { Id = 1, OrderDate = DateTime.Now, TotalValue = 100.50 }
                ]
            };

            _clientValidatorMock
                .Setup(x => x.Validate(It.IsAny<Client>()))
                .Returns(new ValidationResult());

            _clientRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<Client>()))
                .Callback<Client>(c =>
                {
                    c.Id = createdClient.Id;
                    c.Orders = createdClient.Orders;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _clientService.CreateClient(requestModel);

            // Assert
            result.Should().NotBeNull();
            result.ClientId.Should().Be(createdClient.Id);
            result.Name.Should().Be(requestModel.Name);
            result.Email.Should().Be(requestModel.Email);
            result.Telephone.Should().Be(requestModel.Telephone);
            result.BirthDate.Should().Be(requestModel.BirthDate);
            result.PurchaseHistory.Count.Should().Be(1);
            result.PurchaseHistory.First().TotalValue.Should().Be(100.50);

            _clientRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Client>()), Times.Once);
        }

        [Fact]
        public async Task CreateClient_ShouldThrowValidationException_WhenEmailIsInvalid()
        {
            // Arrange
            var requestModel = new ClientRequestModel
            {
                Name = "New Client",
                Email = "invalid-email", // Invalid email
                Telephone = "987654321",
                BirthDate = new DateTime(1990, 1, 1)
            };

            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Email must be valid")
            };

            _clientValidatorMock
                .Setup(x => x.Validate(It.IsAny<Client>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _clientService.CreateClient(requestModel));

            exception.ValidationErrors.Should().Contain("Email must be valid");

            _clientRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Client>()), Times.Never);
        }

        [Fact]
        public async Task UpdateClient_ShouldUpdateClient_WhenValidationPasses()
        {
            // Arrange
            var clientId = 1;
            var requestModel = new ClientRequestModel
            {
                Name = "Updated Client",
                Email = "updated@client.com",
                Telephone = "123123123",
                BirthDate = new DateTime(1980, 1, 1)
            };

            var existingClient = new Client
            {
                Id = clientId,
                Name = "Original Client",
                Email = "original@client.com",
                Telephone = "987654321",
                BirthDate = new DateTime(1990, 1, 1),
                Orders = []
            };

            _clientRepositoryMock
                .Setup(x => x.GetClientByIdAsync(clientId))
                .ReturnsAsync(existingClient);

            _clientValidatorMock
                .Setup(x => x.Validate(It.IsAny<Client>()))
                .Returns(new ValidationResult());

            // Act
            await _clientService.UpdateClient(clientId, requestModel);

            // Assert
            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Client>(c =>
                c.Id == clientId &&
                c.Name == requestModel.Name &&
                c.Email == requestModel.Email &&
                c.Telephone == requestModel.Telephone &&
                c.BirthDate == requestModel.BirthDate)),
            Times.Once);
        }

        [Fact]
        public async Task UpdateClient_ShouldThrowNotFoundException_WhenClientDoesNotExist()
        {
            // Arrange
            var clientId = 999;
            var requestModel = new ClientRequestModel
            {
                Name = "Updated Client",
                Email = "updated@client.com",
                Telephone = "123123123",
                BirthDate = new DateTime(1980, 1, 1)
            };

            _clientRepositoryMock
                .Setup(x => x.GetClientByIdAsync(clientId))
                .ReturnsAsync((Client)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _clientService.UpdateClient(clientId, requestModel));

            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Client>()), Times.Never);
        }

        [Fact]
        public async Task UpdateClient_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var clientId = 1;
            var requestModel = new ClientRequestModel
            {
                Name = "", // Invalid name
                Email = "updated@client.com",
                Telephone = "123123123",
                BirthDate = new DateTime(1980, 1, 1)
            };

            var existingClient = new Client
            {
                Id = clientId,
                Name = "Original Client",
                Email = "original@client.com",
                Telephone = "987654321",
                BirthDate = new DateTime(1990, 1, 1),
                Orders = []
            };

            var validationErrors = new List<ValidationFailure>
            {
                new("Name", "Name is required")
            };

            _clientRepositoryMock
               .Setup(x => x.GetClientByIdAsync(clientId))
               .ReturnsAsync(existingClient);

            _clientValidatorMock
                .Setup(x => x.Validate(It.IsAny<Client>()))
                .Returns(new ValidationResult(validationErrors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Common.Exceptions.ValidationException>(() =>
                _clientService.UpdateClient(clientId, requestModel));

            exception.ValidationErrors.Should().Contain("Name is required");

            _clientRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Client>()), Times.Never);
        }

        [Fact]
        public async Task DeleteClient_ShouldDeleteClient_WhenClientExists()
        {
            // Arrange
            var clientId = 1;
            var existingClient = new Client
            {
                Id = clientId,
                Name = "Client to Delete",
                Email = "delete@client.com",
                Telephone = "123456789",
                BirthDate = new DateTime(1985, 5, 15),
                Orders = new List<Order>()
            };

            _clientRepositoryMock
                .Setup(x => x.GetClientByIdAsync(clientId))
                .ReturnsAsync(existingClient);

            // Act
            await _clientService.DeleteClient(clientId);

            // Assert
            _clientRepositoryMock.Verify(x => x.DeleteAsync(existingClient), Times.Once);

            // Verify logging
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Deleting client with ID: {clientId}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteClient_ShouldThrowNotFoundException_WhenClientDoesNotExist()
        {
            // Arrange
            var clientId = 999;

            _clientRepositoryMock
                .Setup(x => x.GetClientByIdAsync(clientId))
                .ReturnsAsync((Client)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _clientService.DeleteClient(clientId));

            _clientRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Client>()), Times.Never);
        }
    }
}
