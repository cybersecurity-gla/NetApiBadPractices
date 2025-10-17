using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;

namespace BadApiExample.Tests
{
    public class PersonControllerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly PersonController _controller;

        public PersonControllerTests()
        {
            // Configurar base de datos en memoria para tests
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _controller = new PersonController(_context);

            // Seed data para tests
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
                    IsActive = true
                }
            };

            _context.Persons.AddRange(testPersons);
            _context.SaveChanges();
        }

        [Fact]
        public void GetAll_ReturnsOkResult_WithListOfPersons()
        {
            // Act
            var result = _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var persons = okResult.Value.Should().BeOfType<List<Person>>().Subject;
            persons.Should().HaveCount(2);
        }

        [Fact]
        public void GetById_ExistingId_ReturnsOkResult_WithPerson()
        {
            // Arrange
            var existingId = 1;

            // Act
            var result = _controller.GetById(existingId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var person = okResult.Value.Should().BeOfType<Person>().Subject;
            person.Id.Should().Be(existingId);
            person.Name.Should().Be("John Doe");
        }

        [Fact]
        public void GetById_NonExistingId_ReturnsOkResult_WithNull()
        {
            // Arrange
            var nonExistingId = 999;

            // Act
            var result = _controller.GetById(nonExistingId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeNull();
        }

        [Fact]
        public void Create_ValidPerson_ReturnsOkResult_WithCreatedPerson()
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

            // Act
            var result = _controller.Create(newPerson);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var createdPerson = okResult.Value.Should().BeOfType<Person>().Subject;
            createdPerson.Name.Should().Be("Test Person");
            createdPerson.IsActive.Should().BeTrue();
            createdPerson.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void SearchByName_ExistingName_ReturnsMatchingPersons()
        {
            // Arrange
            var searchName = "John";

            // Act
            var result = _controller.SearchByName(searchName);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var persons = okResult.Value.Should().BeOfType<List<Person>>().Subject;
            persons.Should().HaveCount(1);
            persons.First().Name.Should().Contain(searchName);
        }

        [Fact]
        public void SearchByName_NonExistingName_ReturnsEmptyList()
        {
            // Arrange
            var searchName = "NonExistingName";

            // Act
            var result = _controller.SearchByName(searchName);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var persons = okResult.Value.Should().BeOfType<List<Person>>().Subject;
            persons.Should().BeEmpty();
        }

        [Fact]
        public void Update_ExistingId_ReturnsOkResult_WithUpdatedPerson()
        {
            // Arrange
            var existingId = 1;
            var updatedPerson = new Person
            {
                Name = "Updated Name",
                Email = "updated@test.com",
                Age = 40,
                Phone = "999-888-7777",
                Address = "999 Updated St"
            };

            // Act
            var result = _controller.Update(existingId, updatedPerson);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var person = okResult.Value.Should().BeOfType<Person>().Subject;
            person.Name.Should().Be("Updated Name");
            person.Email.Should().Be("updated@test.com");
            person.Age.Should().Be(40);
        }

        [Fact]
        public void Delete_ExistingId_ReturnsOkResult_WithSuccessMessage()
        {
            // Arrange
            var existingId = 1;

            // Act
            var result = _controller.Delete(existingId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("Eliminado correctamente");
            
            // Verify person was deleted
            var deletedPerson = _context.Persons.Find(existingId);
            deletedPerson.Should().BeNull();
        }

        [Fact]
        public void GetDatabaseInfo_ReturnsOkResult_WithDatabaseInfo()
        {
            // Act
            var result = _controller.GetDatabaseInfo();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var info = okResult.Value;
            info.Should().NotBeNull();
        }

        [Fact]
        public void DeleteAll_ReturnsOkResult_WithDeletedCount()
        {
            // Act
            var result = _controller.DeleteAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
            
            // Verify all persons were deleted
            var remainingPersons = _context.Persons.ToList();
            remainingPersons.Should().BeEmpty();
        }

        [Fact]
        public void GetAll_WithException_ReturnsBadRequest()
        {
            // Arrange - Dispose context to cause exception
            _context.Dispose();

            // Act
            var result = _controller.GetAll();

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public void Update_NonExistingId_ThrowsException()
        {
            // Arrange
            var nonExistingId = 999;
            var updatedPerson = new Person
            {
                Name = "Updated Name",
                Email = "updated@test.com",
                Age = 40,
                Phone = "999-888-7777",
                Address = "999 Updated St"
            };

            // Act & Assert
            var action = () => _controller.Update(nonExistingId, updatedPerson);
            action.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void Delete_NonExistingId_ThrowsException()
        {
            // Arrange
            var nonExistingId = 999;

            // Act & Assert
            var action = () => _controller.Delete(nonExistingId);
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Create_WithNullPerson_HandlesGracefully()
        {
            // Arrange
            Person person = null;

            // Act
            var result = _controller.Create(person);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var createdPerson = okResult.Value.Should().BeOfType<Person>().Subject;
            createdPerson.Should().NotBeNull();
            createdPerson.IsActive.Should().BeTrue();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}