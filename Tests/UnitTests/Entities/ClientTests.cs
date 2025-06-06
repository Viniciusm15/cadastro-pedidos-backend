using Domain.Models.Entities;
using Domain.Validators;
using FluentAssertions;

namespace Tests.UnitTests.Entities
{
    public class ClientTests
    {
        [Fact]
        public void Constructor_ShouldInitializeDefaultValues()
        {
            // Arrange & Act
            var client = new Client
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Telephone = "123456789"
            };

            // Assert
            client.IsActive.Should().BeTrue();
            client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            client.DeletedAt.Should().BeNull();
        }

        [Fact]
        public void Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var client = new Client
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Telephone = "123456789"
            };

            // Act & Assert
            client.Name.Should().Be("John Doe");
            client.Email.Should().Be("john.doe@example.com");
            client.Telephone.Should().Be("123456789");
        }

        [Fact]
        public void SoftDelete_ShouldSetDeletedAtAndIsActive()
        {
            // Arrange
            var client = new Client
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Telephone = "123456789",
                DeletedAt = DateTime.UtcNow,
                IsActive = false
            };

            // Assert
            client.DeletedAt.Should().NotBeNull();
            client.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Validate_ShouldUseProvidedValidator()
        {
            // Arrange
            var validator = new ClientValidator();
            var client = new Client
            {
                Name = "Valid Name",
                Email = "valid.email@example.com",
                Telephone = "123456789",
                DeletedAt = DateTime.UtcNow,
                IsActive = false
            };

            // Act
            var result = client.Validate(validator);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validator_ShouldRejectInvalidEmailOrEmptyName()
        {
            // Arrange
            var validator = new ClientValidator();
            var client = new Client
            {
                Name = "",
                Email = "invalid-email",
                Telephone = "123"
            };

            // Act
            var result = validator.Validate(client);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Name");
            result.Errors.Should().Contain(e => e.PropertyName == "Email");
        }
    }
}
