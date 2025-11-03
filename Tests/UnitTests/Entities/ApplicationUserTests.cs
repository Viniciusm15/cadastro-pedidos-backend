using Domain.Interfaces;
using Domain.Models.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;

namespace Tests.UnitTests.Entities
{
    public class ApplicationUserTests
    {
        [Fact]
        public void Constructor_ShouldInitializeDefaultValues()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "john.doe@example.com",
                Email = "john.doe@example.com"
            };

            // Assert
            user.IsActive.Should().BeTrue();
            user.DeletedAt.Should().BeNull();
            user.FirstName.Should().Be("John");
            user.LastName.Should().Be("Doe");
            user.UserName.Should().Be("john.doe@example.com");
            user.Email.Should().Be("john.doe@example.com");
        }

        [Fact]
        public void Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "Jane",
                LastName = "Smith",
                UserName = "jane.smith@example.com",
                Email = "jane.smith@example.com",
                PhoneNumber = "+1234567890",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true
            };

            // Act & Assert
            user.FirstName.Should().Be("Jane");
            user.LastName.Should().Be("Smith");
            user.UserName.Should().Be("jane.smith@example.com");
            user.Email.Should().Be("jane.smith@example.com");
            user.PhoneNumber.Should().Be("+1234567890");
            user.EmailConfirmed.Should().BeTrue();
            user.PhoneNumberConfirmed.Should().BeTrue();
            user.TwoFactorEnabled.Should().BeFalse();
            user.LockoutEnabled.Should().BeTrue();
        }

        [Fact]
        public void SoftDelete_ShouldSetDeletedAtAndIsActive()
        {
            // Arrange
            var deleteTime = DateTime.UtcNow;
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "john.doe@example.com",
                DeletedAt = deleteTime,
                IsActive = false
            };

            // Assert
            user.DeletedAt.Should().Be(deleteTime);
            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void FullName_ShouldReturnConcatenatedFirstNameAndLastName()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var fullName = $"{user.FirstName} {user.LastName}";

            // Assert
            fullName.Should().Be("John Doe");
        }

        [Fact]
        public void ClientNavigation_ShouldWorkCorrectly()
        {
            // Arrange
            var user = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "john.doe@example.com"
            };

            var client = new Client
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Telephone = "123456789",
                ApplicationUserId = user.Id
            };

            // Act
            user.Client = client;

            // Assert
            user.Client.Should().NotBeNull();
            user.Client.Name.Should().Be("John Doe");
            user.Client.Email.Should().Be("john.doe@example.com");
            user.Client.ApplicationUserId.Should().Be(user.Id);
        }

        [Fact]
        public void IdentityUserProperties_ShouldInheritCorrectly()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                PhoneNumber = "123456789",
                AccessFailedCount = 2,
                LockoutEnd = DateTimeOffset.UtcNow.AddHours(1)
            };

            // Assert
            user.UserName.Should().Be("testuser");
            user.Email.Should().Be("test@example.com");
            user.PhoneNumber.Should().Be("123456789");
            user.AccessFailedCount.Should().Be(2);
            user.LockoutEnd.Should().NotBeNull();
        }

        [Fact]
        public void IsActive_DefaultValueShouldBeTrue()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void DeletedAt_DefaultValueShouldBeNull()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            user.DeletedAt.Should().BeNull();
        }

        [Theory]
        [InlineData("John", "Doe", "john.doe@example.com")]
        [InlineData("Jane", "Smith", "jane.smith@company.com")]
        [InlineData("Bob", "Wilson", "bob.wilson@test.org")]
        public void User_ShouldHandleDifferentDataCombinations(string firstName, string lastName, string email)
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = email,
                Email = email
            };

            // Assert
            user.FirstName.Should().Be(firstName);
            user.LastName.Should().Be(lastName);
            user.UserName.Should().Be(email);
            user.Email.Should().Be(email);
        }

        [Fact]
        public void User_ShouldHandleNullAndEmptyValues()
        {
            // Arrange & Act
            var user = new ApplicationUser
            {
                FirstName = "",
                LastName = string.Empty,
                UserName = null,
                Email = null
            };

            // Assert
            user.FirstName.Should().Be("");
            user.LastName.Should().Be("");
            user.UserName.Should().BeNull();
            user.Email.Should().BeNull();
        }

        [Fact]
        public void User_ShouldImplementISoftDeletable()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            user.Should().BeAssignableTo<ISoftDeletable>();
            user.IsActive.Should().BeTrue();
            user.DeletedAt.Should().BeNull();
        }

        [Fact]
        public void User_ShouldBeIdentityUser()
        {
            // Arrange & Act
            var user = new ApplicationUser();

            // Assert
            user.Should().BeAssignableTo<IdentityUser>();
        }
    }
}
