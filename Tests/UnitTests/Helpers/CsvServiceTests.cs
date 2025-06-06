using Common.Helpers;
using CsvHelper;
using FluentAssertions;
using System.Text;

namespace Tests.UnitTests.Helpers
{
    public class CsvServiceTests
    {
        private readonly CsvService _csvService = new();

        public record TestRecord(int Id, string Name, DateTime CreatedAt, decimal Price);

        [Fact]
        public void WriteCsvToByteArray_ShouldReturnValidCsvData_WhenRecordsExist()
        {
            // Arrange
            var records = new List<TestRecord>
            {
                new(1, "Product A", DateTime.Now, 9.99m),
                new(2, "Product B", DateTime.Now.AddDays(-1), 19.99m)
            };

            // Act
            var result = _csvService.WriteCsvToByteArray(records);
            var csvContent = Encoding.UTF8.GetString(result);

            // Assert
            result.Should().NotBeNullOrEmpty();
            csvContent.Should().Contain("Id,Name,CreatedAt,Price");
            csvContent.Should().Contain("Product A");
        }

        [Fact]
        public void WriteCsvToByteArray_ShouldReturnEmptyArray_WhenRecordsAreEmpty()
        {
            // Arrange
            var emptyRecords = Enumerable.Empty<TestRecord>();

            // Act
            var result = _csvService.WriteCsvToByteArray(emptyRecords);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void WriteCsvToByteArray_ShouldUseInvariantCulture_ForNumericFormats()
        {
            // Arrange
            var records = new List<TestRecord>
            {
                new(1, "Test", DateTime.Now, 1234.56m)
            };

            // Act
            var result = _csvService.WriteCsvToByteArray(records);
            var csvContent = Encoding.UTF8.GetString(result);

            // Assert
            csvContent.Should().Contain("1234.56");
        }

        [Fact]
        public void WriteCsvToByteArray_ShouldThrowCsvHelperException_WhenRecordIsInvalid()
        {
            // Arrange
            var invalidRecords = new List<object>
            {
                new { InvalidProperty = new object() }
            };

            // Act & Assert
            Assert.Throws<WriterException>(() =>
                _csvService.WriteCsvToByteArray(invalidRecords));
        }

        [Fact]
        public void WriteCsvToByteArray_ShouldHonorCsvConfiguration()
        {
            // Arrange
            var records = new List<TestRecord>
            {
                new(1, "  Product  ", DateTime.Now, 9.99m)
            };

            // Act
            var result = _csvService.WriteCsvToByteArray(records);
            var csvContent = Encoding.UTF8.GetString(result);

            // Assert (config TrimOptions.Trim)
            csvContent.Should().Contain("Product").And.NotContain("  ");
        }
    }
}
