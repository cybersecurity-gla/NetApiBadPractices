using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace BadApiExample.Tests
{
    public class AppDbContextTests : IDisposable
    {
        private readonly AppDbContext _context;

        public AppDbContextTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
        }

        [Fact]
        public void AppDbContext_ShouldHavePersonsDbSet()
        {
            // Assert
            _context.Persons.Should().NotBeNull();
        }

        [Fact]
        public void AppDbContext_CanAddPerson()
        {
            // Arrange
            var person = new Person
            {
                Name = "Test Person",
                Email = "test@example.com",
                Age = 25,
                Phone = "123-456-7890",
                Address = "123 Test St",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            // Act
            _context.Persons.Add(person);
            _context.SaveChanges();

            // Assert
            var savedPerson = _context.Persons.FirstOrDefault();
            savedPerson.Should().NotBeNull();
            savedPerson!.Name.Should().Be("Test Person");
        }

        [Fact]
        public void AppDbContext_CanQueryPersons()
        {
            // Arrange
            var person1 = new Person 
            { 
                Name = "Person 1", 
                Email = "p1@test.com", 
                Age = 25,
                Phone = "123-456-7890",
                Address = "123 Test St",
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            var person2 = new Person 
            { 
                Name = "Person 2", 
                Email = "p2@test.com", 
                Age = 30,
                Phone = "987-654-3210",
                Address = "456 Test Ave",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Persons.AddRange(person1, person2);
            _context.SaveChanges();

            // Act
            var persons = _context.Persons.ToList();

            // Assert
            persons.Should().HaveCount(2);
            persons.Should().Contain(p => p.Name == "Person 1");
            persons.Should().Contain(p => p.Name == "Person 2");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}