using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;

namespace BadApiExample.Tests
{
    public class ErrorHandlingTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly PersonController _controller;

        public ErrorHandlingTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _controller = new PersonController(_context);
        }

        [Fact]
        public void PersonController_Update_WithNullExistingPerson_ThrowsNullReference()
        {
            // Arrange
            var nonExistentId = 999;
            var updatePerson = new Person
            {
                Name = "Updated",
                Email = "updated@test.com",
                Age = 25,
                Phone = "123-456-7890",
                Address = "123 Updated St"
            };

            // Act & Assert
            var action = () => _controller.Update(nonExistentId, updatePerson);
            action.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void PersonController_Delete_WithNullPerson_ThrowsArgumentNull()
        {
            // Arrange
            var nonExistentId = 999;

            // Act & Assert
            var action = () => _controller.Delete(nonExistentId);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PersonController_SearchByName_WithNullName_ReturnsResult()
        {
            // Arrange
            string searchName = null;

            // Act
            var result = _controller.SearchByName(searchName);

            // Assert - The method should handle null gracefully, not throw
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var persons = okResult.Value.Should().BeOfType<List<Person>>().Subject;
            persons.Should().NotBeNull();
        }

        [Fact]
        public void BadExamplesController_ExecuteCommand_WithNullCommand_ReturnsError()
        {
            // Arrange
            var controller = new BadExamplesController();
            string command = null;

            // Act
            var result = controller.ExecuteCommand(command);

            // Assert - Should return OK with error information
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public void BadExamplesController_UploadData_WithNullPersonsList_ThrowsNullReference()
        {
            // Arrange
            var controller = new BadExamplesController();
            List<Person> persons = null;

            // Act & Assert
            var action = () => controller.UploadData(persons);
            action.Should().Throw<NullReferenceException>();
        }

        [Fact]
        public void BadDataService_Singleton_ConsistentInstance()
        {
            // Act
            var instance1 = BadDataService.Instance;
            var instance2 = BadDataService.Instance;

            // Assert
            instance1.Should().BeSameAs(instance2);
            instance1.Should().NotBeNull();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}