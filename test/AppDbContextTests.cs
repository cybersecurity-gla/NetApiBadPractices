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

        [Fact]
        public void AppDbContext_CanUpdatePerson()
        {
            // Arrange
            var person = new Person
            {
                Name = "Original Name",
                Email = "original@test.com",
                Age = 25,
                Phone = "123-456-7890",
                Address = "123 Test St",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Persons.Add(person);
            _context.SaveChanges();

            // Act
            person.Name = "Updated Name";
            person.Email = "updated@test.com";
            _context.SaveChanges();

            // Assert
            var updatedPerson = _context.Persons.FirstOrDefault();
            updatedPerson.Should().NotBeNull();
            updatedPerson!.Name.Should().Be("Updated Name");
            updatedPerson.Email.Should().Be("updated@test.com");
        }

        [Fact]
        public void AppDbContext_CanDeletePerson()
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

            _context.Persons.Add(person);
            _context.SaveChanges();

            // Act
            _context.Persons.Remove(person);
            _context.SaveChanges();

            // Assert
            var deletedPerson = _context.Persons.FirstOrDefault();
            deletedPerson.Should().BeNull();
        }

        [Fact]
        public void AppDbContext_CanFilterPersonsByAge()
        {
            // Arrange
            var person1 = new Person { Name = "Young", Age = 20, Email = "young@test.com", Phone = "123", Address = "123 St", CreatedDate = DateTime.Now, IsActive = true };
            var person2 = new Person { Name = "Old", Age = 50, Email = "old@test.com", Phone = "456", Address = "456 St", CreatedDate = DateTime.Now, IsActive = true };

            _context.Persons.AddRange(person1, person2);
            _context.SaveChanges();

            // Act
            var youngPersons = _context.Persons.Where(p => p.Age < 30).ToList();

            // Assert
            youngPersons.Should().HaveCount(1);
            youngPersons.First().Name.Should().Be("Young");
        }

        [Fact]
        public void AppDbContext_CanFilterPersonsByActiveStatus()
        {
            // Arrange
            var activePerson = new Person { Name = "Active", IsActive = true, Email = "active@test.com", Phone = "123", Address = "123 St", CreatedDate = DateTime.Now, Age = 25 };
            var inactivePerson = new Person { Name = "Inactive", IsActive = false, Email = "inactive@test.com", Phone = "456", Address = "456 St", CreatedDate = DateTime.Now, Age = 30 };

            _context.Persons.AddRange(activePerson, inactivePerson);
            _context.SaveChanges();

            // Act
            var activePersons = _context.Persons.Where(p => p.IsActive).ToList();

            // Assert
            activePersons.Should().HaveCount(1);
            activePersons.First().Name.Should().Be("Active");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}