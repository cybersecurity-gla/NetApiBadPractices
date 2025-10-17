using Xunit;
using FluentAssertions;

namespace BadApiExample.Tests
{
    public class PersonTests
    {
        [Fact]
        public void Person_DefaultProperties_ShouldHaveExpectedValues()
        {
            // Arrange & Act
            var person = new Person();

            // Assert
            person.Id.Should().Be(0);
            person.Name.Should().BeNull();
            person.Email.Should().BeNull();
            person.Age.Should().Be(0);
            person.Phone.Should().BeNull();
            person.Address.Should().BeNull();
            person.CreatedDate.Should().Be(default(DateTime));
            person.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Person_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var person = new Person();
            var testDate = DateTime.Now;

            // Act
            person.Id = 1;
            person.Name = "John Doe";
            person.Email = "john@example.com";
            person.Age = 30;
            person.Phone = "123-456-7890";
            person.Address = "123 Main St";
            person.CreatedDate = testDate;
            person.IsActive = true;

            // Assert
            person.Id.Should().Be(1);
            person.Name.Should().Be("John Doe");
            person.Email.Should().Be("john@example.com");
            person.Age.Should().Be(30);
            person.Phone.Should().Be("123-456-7890");
            person.Address.Should().Be("123 Main St");
            person.CreatedDate.Should().Be(testDate);
            person.IsActive.Should().BeTrue();
        }
    }
}