using Xunit;
using FluentAssertions;

namespace BadApiExample.Tests
{
    public class BadExtensionsTests
    {
        [Fact]
        public void ToUnsafeString_WithValidPerson_ReturnsFormattedString()
        {
            // Arrange
            var person = new Person
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "123-456-7890",
                Address = "123 Main St"
            };

            // Act
            var result = person.ToUnsafeString();

            // Assert
            result.Should().Contain("John Doe");
            result.Should().Contain("john@example.com");
            result.Should().Contain("123-456-7890");
            result.Should().Contain("123 Main St");
        }

        [Fact]
        public void FromUnsafeString_WithValidString_ReturnsPerson()
        {
            // Arrange
            var personData = "John Doe,john@example.com,123-456-7890,123 Main St";

            // Act
            var result = personData.FromUnsafeString();

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("John Doe");
            result.Email.Should().Be("john@example.com");
            result.Phone.Should().Be("123-456-7890");
            result.Address.Should().Be("123 Main St");
        }

        [Fact]
        public void FromUnsafeString_WithInvalidString_ThrowsException()
        {
            // Arrange
            var invalidData = "OnlyOnePart";

            // Act & Assert
            var action = () => invalidData.FromUnsafeString();
            action.Should().Throw<IndexOutOfRangeException>();
        }
    }
}