using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;

namespace BadApiExample.Tests
{
    public class CompleteCoverageTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly PersonController _controller;

        public CompleteCoverageTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _controller = new PersonController(_context);
        }

        [Fact]
        public void PersonController_GetAll_WithEmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var persons = okResult.Value.Should().BeOfType<List<Person>>().Subject;
            persons.Should().BeEmpty();
        }

        [Fact]
        public void PersonController_GetById_WithZeroId_ReturnsNull()
        {
            // Act
            var result = _controller.GetById(0);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeNull();
        }

        [Fact]
        public void PersonController_GetById_WithNegativeId_ReturnsNull()
        {
            // Act
            var result = _controller.GetById(-1);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeNull();
        }

        [Fact]
        public void PersonController_SearchByName_WithWhitespace_ReturnsEmpty()
        {
            // Arrange
            var searchName = "   "; // Only whitespace

            // Act
            var result = _controller.SearchByName(searchName);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var persons = okResult.Value.Should().BeOfType<List<Person>>().Subject;
            persons.Should().BeEmpty();
        }

        [Fact]
        public void BadExtensions_ToUnsafeString_WithSpecialCharacters()
        {
            // Arrange
            var person = new Person
            {
                Name = "John O'Connor",
                Email = "john@test.com",
                Phone = "123-456-7890",
                Address = "123 Main St, Apt #5"
            };

            // Act
            var result = person.ToUnsafeString();

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("John O'Connor");
            result.Should().Contain("#5");
        }

        [Fact]
        public void BadExtensions_FromUnsafeString_WithExtraCommas()
        {
            // Arrange
            var personData = "John,Doe,john@test.com,123-456-7890,123 Main St,Extra,Data,Here";

            // Act
            var result = personData.FromUnsafeString();

            // Assert - Method uses only first 4 fields after splitting
            result.Should().NotBeNull();
            result.Name.Should().Be("John");
            result.Email.Should().Be("Doe"); // Second field becomes email
            result.Phone.Should().Be("john@test.com"); // Third field becomes phone
            result.Address.Should().Be("123-456-7890"); // Fourth field becomes address
        }

        [Fact]
        public void BadExtensions_FromUnsafeString_WithCommasInValues()
        {
            // Arrange - Method splits by comma, so commas in values cause issues
            var personData = "Smith, John,john.smith@test.com,123-456-7890,123 Main St, Suite 100";

            // Act
            var result = personData.FromUnsafeString();

            // Assert - The method splits by comma, so "Smith" is name, " John" is email
            result.Should().NotBeNull();
            result.Name.Should().Be("Smith");
            result.Email.Should().Be(" John"); // This is what actually happens
        }

        [Fact]
        public void Person_AllProperties_CanBeSetToNonDefaultValues()
        {
            // Arrange & Act
            var person = new Person
            {
                Id = 999,
                Name = "Test Name",
                Email = "test@email.com",
                Age = 50,
                Phone = "555-0123",
                Address = "789 Test Road",
                CreatedDate = new DateTime(2023, 1, 1),
                IsActive = false
            };

            // Assert
            person.Id.Should().Be(999);
            person.Name.Should().Be("Test Name");
            person.Email.Should().Be("test@email.com");
            person.Age.Should().Be(50);
            person.Phone.Should().Be("555-0123");
            person.Address.Should().Be("789 Test Road");
            person.CreatedDate.Should().Be(new DateTime(2023, 1, 1));
            person.IsActive.Should().BeFalse();
        }

        [Fact]
        public void BadExamplesController_CONNECTION_STRING_IsPublicField()
        {
            // Act
            var connectionString = BadExamplesController.CONNECTION_STRING;

            // Assert
            connectionString.Should().NotBeNullOrEmpty();
            connectionString.Should().Contain("localhost");
            connectionString.Should().Contain("BadDatabase");
        }

        [Fact]
        public void BadExamplesController_GetSystemConfig_ContainsAllExpectedFields()
        {
            // Arrange
            var controller = new BadExamplesController();

            // Act
            var result = controller.GetSystemConfig();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var config = okResult.Value;
            
            var configType = config.GetType();
            configType.GetProperty("DatabasePassword").Should().NotBeNull();
            configType.GetProperty("SecretKey").Should().NotBeNull();
            configType.GetProperty("AdminEmail").Should().NotBeNull();
            configType.GetProperty("AdminPassword").Should().NotBeNull();
            configType.GetProperty("ServerPath").Should().NotBeNull();
            configType.GetProperty("Environment").Should().NotBeNull();
            configType.GetProperty("ConnectionString").Should().NotBeNull();
        }

        [Fact]
        public void BadExamplesController_GetPrivateData_ContainsAllSensitiveData()
        {
            // Arrange
            var controller = new BadExamplesController();

            // Act
            var result = controller.GetPrivateData();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value;
            
            var dataType = data.GetType();
            dataType.GetProperty("CreditCards").Should().NotBeNull();
            dataType.GetProperty("SocialSecurityNumbers").Should().NotBeNull();
            dataType.GetProperty("Passwords").Should().NotBeNull();
            dataType.GetProperty("PersonalEmails").Should().NotBeNull();
        }

        [Fact]
        public void AppDbContext_CanAddAndQueryMultiplePersons()
        {
            // Arrange
            var persons = new[]
            {
                new Person { Name = "Person 1", Email = "p1@test.com", Age = 20, Phone = "111", Address = "Addr1", CreatedDate = DateTime.Now, IsActive = true },
                new Person { Name = "Person 2", Email = "p2@test.com", Age = 30, Phone = "222", Address = "Addr2", CreatedDate = DateTime.Now, IsActive = false },
                new Person { Name = "Person 3", Email = "p3@test.com", Age = 40, Phone = "333", Address = "Addr3", CreatedDate = DateTime.Now, IsActive = true }
            };

            // Act
            _context.Persons.AddRange(persons);
            _context.SaveChanges();

            // Assert
            var activePersons = _context.Persons.Where(p => p.IsActive).ToList();
            var inactivePersons = _context.Persons.Where(p => !p.IsActive).ToList();
            var allPersons = _context.Persons.ToList();

            activePersons.Should().HaveCount(2);
            inactivePersons.Should().HaveCount(1);
            allPersons.Should().HaveCount(3);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}