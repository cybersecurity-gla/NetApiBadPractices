using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;

namespace BadApiExample.Tests
{
    public class EdgeCaseTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly PersonController _controller;

        public EdgeCaseTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _controller = new PersonController(_context);

            // Seed with some test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var testPersons = new List<Person>
            {
                new Person
                {
                    Id = 1,
                    Name = "John Doe",
                    Email = "john@test.com",
                    Age = 25,
                    Phone = "123-456-7890",
                    Address = "123 Test St",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    IsActive = true
                },
                new Person
                {
                    Id = 2,
                    Name = "Jane Smith",
                    Email = "jane@test.com",
                    Age = 30,
                    Phone = "987-654-3210",
                    Address = "456 Test Ave",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    IsActive = false
                }
            };

            _context.Persons.AddRange(testPersons);
            _context.SaveChanges();
        }

        [Fact]
        public void PersonController_SearchByName_EmptyResult()
        {
            // Arrange
            var searchName = "NonExistent";

            // Act
            var result = _controller.SearchByName(searchName);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var persons = okResult.Value.Should().BeOfType<List<Person>>().Subject;
            persons.Should().BeEmpty();
        }

        [Fact]
        public void PersonController_SearchByName_PartialMatch()
        {
            // Arrange
            var searchName = "Jo"; // Should match "John"

            // Act
            var result = _controller.SearchByName(searchName);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var persons = okResult.Value.Should().BeOfType<List<Person>>().Subject;
            persons.Should().HaveCount(1);
            persons.First().Name.Should().Contain("John");
        }

        [Fact]
        public void PersonController_Create_SetsCorrectDefaults()
        {
            // Arrange
            var newPerson = new Person
            {
                Name = "Test Person",
                Email = "test@example.com",
                Age = 35,
                Phone = "555-0123",
                Address = "789 Test Blvd"
            };

            var initialDateTime = DateTime.Now;

            // Act
            var result = _controller.Create(newPerson);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var createdPerson = okResult.Value.Should().BeOfType<Person>().Subject;
            
            createdPerson.IsActive.Should().BeTrue();
            createdPerson.CreatedDate.Should().BeCloseTo(initialDateTime, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void AppDbContext_ConnectionString_ThrowsForInMemory()
        {
            // Act & Assert - InMemory database doesn't support GetConnectionString
            var action = () => _context.Database.GetConnectionString();
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void BadExamplesController_GetSystemConfig_ExposesConfiguration()
        {
            // Arrange
            var controller = new BadExamplesController();

            // Act
            var result = controller.GetSystemConfig();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var config = okResult.Value;
            config.Should().NotBeNull();
            
            // Use reflection to check properties exist (since it's an anonymous object)
            var configType = config.GetType();
            configType.GetProperty("DatabasePassword").Should().NotBeNull();
            configType.GetProperty("SecretKey").Should().NotBeNull();
            configType.GetProperty("AdminEmail").Should().NotBeNull();
        }

        [Fact]
        public void BadExamplesController_GetPrivateData_ContainsSensitiveInfo()
        {
            // Arrange
            var controller = new BadExamplesController();

            // Act
            var result = controller.GetPrivateData();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value;
            data.Should().NotBeNull();
            
            // Use reflection to verify sensitive data structure
            var dataType = data.GetType();
            dataType.GetProperty("CreditCards").Should().NotBeNull();
            dataType.GetProperty("SocialSecurityNumbers").Should().NotBeNull();
            dataType.GetProperty("Passwords").Should().NotBeNull();
        }

        [Fact]
        public void Person_PropertiesCanBeSetAndRetrieved()
        {
            // Arrange
            var person = new Person();
            var testDate = DateTime.Now;

            // Act
            person.Id = 100;
            person.Name = "Test Name";
            person.Email = "test@email.com";
            person.Age = 25;
            person.Phone = "555-1234";
            person.Address = "123 Test Street";
            person.CreatedDate = testDate;
            person.IsActive = true;

            // Assert
            person.Id.Should().Be(100);
            person.Name.Should().Be("Test Name");
            person.Email.Should().Be("test@email.com");
            person.Age.Should().Be(25);
            person.Phone.Should().Be("555-1234");
            person.Address.Should().Be("123 Test Street");
            person.CreatedDate.Should().Be(testDate);
            person.IsActive.Should().BeTrue();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}