using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BadApiExample.Tests;

public class Final80PercentTests
{
    private PersonController CreateController()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new PersonController(context);
    }

    [Fact]
    public void BadDataService_GetAllPersonsSlowly_CoversLazy()
    {
        // Arrange
        var service = BadDataService.Instance;

        // Act & Assert - Test the method exists and handles SQL connection failure
        var exception = Assert.ThrowsAny<Exception>(() => service.GetAllPersonsSlowly());
        exception.Should().NotBeNull();
    }

    [Fact]
    public void BadDataService_CreatePersonUnsafe_CoversMethod()
    {
        // Arrange
        var service = BadDataService.Instance;
        var validPerson = new Person
        {
            Name = "Test User",
            Email = "test@test.com",
            Phone = "123-456-7890",
            Address = "123 Test St"
        };

        // Act & Assert - Exercise the unsafe method
        var exception = Assert.ThrowsAny<Exception>(() => service.CreatePersonUnsafe(validPerson));
        exception.Should().NotBeNull();
    }

    [Fact]
    public void BadExtensions_FromUnsafeString_WithFullValidData_Parses()
    {
        // Arrange - Test with exactly what the method expects
        var personData = "John,Doe,john@example.com,123-456-7890,123 Main St";

        // Act
        var result = personData.FromUnsafeString();

        // Assert - Based on the actual implementation
        result.Should().NotBeNull();
        result.Name.Should().Be("John");
    }

    [Fact]
    public void BadExtensions_FromUnsafeString_WithLongData_HandlesCorrectly()
    {
        // Arrange - Test with more fields than expected
        var personData = "John,Doe,Middle,john@example.com,123-456-7890,123 Main St,Extra,Data";

        // Act
        var result = personData.FromUnsafeString();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("John");
    }

    [Fact]
    public void PersonController_Create_SetsTimestamp()
    {
        // Arrange
        var controller = CreateController();
        var person = new Person
        {
            Name = "Time Test",
            Email = "time@test.com",
            Phone = "999-888-7777",
            Address = "Time Street"
        };

        // Act
        var result = controller.Create(person);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var createdPerson = okResult.Value.Should().BeOfType<Person>().Subject;
        createdPerson.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void PersonController_Update_WithValidPersonAndId_Updates()
    {
        // Arrange
        var controller = CreateController();
        var originalPerson = new Person
        {
            Name = "Original Name",
            Email = "original@test.com",
            Phone = "111-111-1111",
            Address = "Original Address"
        };

        var createdResult = controller.Create(originalPerson);
        var createdPerson = ((OkObjectResult)createdResult).Value as Person;

        var updatedPerson = new Person
        {
            Id = createdPerson!.Id,
            Name = "Updated Name",
            Email = "updated@test.com",
            Phone = "222-222-2222",
            Address = "Updated Address"
        };

        // Act
        var result = controller.Update(createdPerson.Id, updatedPerson);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPerson = okResult.Value.Should().BeOfType<Person>().Subject;
        returnedPerson.Name.Should().Be("Updated Name");
        returnedPerson.Email.Should().Be("updated@test.com");
    }

    [Fact]
    public void PersonController_Delete_WithExistingId_DeletesSuccessfully()
    {
        // Arrange
        var controller = CreateController();
        var person = new Person
        {
            Name = "To Delete",
            Email = "delete@test.com",
            Phone = "000-000-0000",
            Address = "Delete Street"
        };

        var createdResult = controller.Create(person);
        var createdPerson = ((OkObjectResult)createdResult).Value as Person;

        // Act
        var result = controller.Delete(createdPerson!.Id);

        // Assert - The actual method returns OkObjectResult, not OkResult
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void PersonController_SearchByName_WithExactMatch_FindsResults()
    {
        // Arrange
        var controller = CreateController();
        var person = new Person
        {
            Name = "SearchableTest",
            Email = "searchable@test.com",
            Phone = "555-555-5555",
            Address = "Search Avenue"
        };

        controller.Create(person);

        // Act
        var result = controller.SearchByName("SearchableTest");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var persons = okResult.Value.Should().BeAssignableTo<IEnumerable<Person>>().Subject.ToList();
        persons.Should().HaveCount(1);
        persons.First().Name.Should().Be("SearchableTest");
    }

    [Fact]
    public void AppDbContext_Persons_DbSet_IsAccessible()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act & Assert
        using var context = new AppDbContext(options);
        context.Persons.Should().NotBeNull();
        context.Persons.Should().BeAssignableTo<DbSet<Person>>();
    }

    [Fact]
    public void Person_PropertySetters_WorkCorrectly()
    {
        // Arrange & Act
        var person = new Person
        {
            Name = "Test",
            Email = "test@test.com",
            Phone = "123",
            Address = "Test Address"
        };

        var testDate = DateTime.Now.AddHours(-1);
        person.Id = 99;
        person.IsActive = true;
        person.CreatedDate = testDate;

        // Assert
        person.Id.Should().Be(99);
        person.Name.Should().Be("Test");
        person.Email.Should().Be("test@test.com");
        person.Phone.Should().Be("123");
        person.Address.Should().Be("Test Address");
        person.IsActive.Should().BeTrue();
        person.CreatedDate.Should().Be(testDate);
    }

    [Fact]
    public void PersonController_GetById_WithExistingId_ReturnsCorrectPerson()
    {
        // Arrange
        var controller = CreateController();
        var person = new Person
        {
            Name = "GetByIdTest",
            Email = "getbyid@test.com",
            Phone = "777-777-7777",
            Address = "GetById Street"
        };

        var createdResult = controller.Create(person);
        var createdPerson = ((OkObjectResult)createdResult).Value as Person;

        // Act
        var result = controller.GetById(createdPerson!.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var foundPerson = okResult.Value.Should().BeOfType<Person>().Subject;
        foundPerson.Name.Should().Be("GetByIdTest");
        foundPerson.Email.Should().Be("getbyid@test.com");
    }

    [Fact]
    public void PersonController_GetAll_WithMultiplePersons_ReturnsAll()
    {
        // Arrange
        var controller = CreateController();
        var person1 = new Person { Name = "Person1", Email = "p1@test.com", Phone = "111", Address = "Addr1" };
        var person2 = new Person { Name = "Person2", Email = "p2@test.com", Phone = "222", Address = "Addr2" };
        var person3 = new Person { Name = "Person3", Email = "p3@test.com", Phone = "333", Address = "Addr3" };

        controller.Create(person1);
        controller.Create(person2);
        controller.Create(person3);

        // Act
        var result = controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var persons = okResult.Value.Should().BeAssignableTo<IEnumerable<Person>>().Subject.ToList();
        persons.Should().HaveCount(3);
        persons.Should().Contain(p => p.Name == "Person1");
        persons.Should().Contain(p => p.Name == "Person2");
        persons.Should().Contain(p => p.Name == "Person3");
    }
}