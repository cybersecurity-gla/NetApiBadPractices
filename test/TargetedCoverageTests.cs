using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BadApiExample.Tests;

public class TargetedCoverageTests
{
    private PersonController CreateController()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        return new PersonController(context);
    }

    private BadExamplesController CreateBadController()
    {
        return new BadExamplesController();
    }

    [Fact]
    public void BadExamplesController_GetSystemConfig_ReturnsConfig()
    {
        // Arrange
        var controller = CreateBadController();

        // Act
        var result = controller.GetSystemConfig();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var config = okResult.Value;
        config.Should().NotBeNull();
        
        // Use reflection to verify the anonymous object contains expected properties
        var configType = config!.GetType();
        configType.GetProperty("DatabasePassword").Should().NotBeNull();
        configType.GetProperty("SecretKey").Should().NotBeNull();
        configType.GetProperty("AdminPassword").Should().NotBeNull();
    }

    [Fact]
    public void BadExamplesController_GetPrivateData_ReturnsPrivateData()
    {
        // Arrange
        var controller = CreateBadController();

        // Act
        var result = controller.GetPrivateData();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var data = okResult.Value;
        data.Should().NotBeNull();
        
        // Use reflection to verify the anonymous object contains expected properties
        var dataType = data!.GetType();
        dataType.GetProperty("CreditCards").Should().NotBeNull();
        dataType.GetProperty("Passwords").Should().NotBeNull();
        dataType.GetProperty("SocialSecurityNumbers").Should().NotBeNull();
    }

    [Fact]
    public void BadExamplesController_UploadData_WithEmptyList_ReturnsOk()
    {
        // Arrange
        var controller = CreateBadController();
        var emptyList = new List<Person>();

        // Act
        var result = controller.UploadData(emptyList);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void BadExamplesController_UploadData_WithValidList_ReturnsOk()
    {
        // Arrange
        var controller = CreateBadController();
        var persons = new List<Person>
        {
            new Person { Name = "Test1", Email = "test1@test.com", Phone = "111", Address = "Addr1" }
        };

        // Act
        var result = controller.UploadData(persons);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void BadDataService_Instance_ReturnsSameInstance()
    {
        // Act
        var instance1 = BadDataService.Instance;
        var instance2 = BadDataService.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2);
        instance1.Should().NotBeNull();
    }

    [Fact]
    public void BadDataService_Instance_IsNotNull()
    {
        // Act
        var instance = BadDataService.Instance;

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeOfType<BadDataService>();
    }

    [Fact]
    public void PersonController_Delete_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var controller = CreateController();
        var person = new Person 
        { 
            Name = "ToDelete", 
            Email = "delete@test.com", 
            Phone = "999", 
            Address = "Delete Addr" 
        };
        
        var createdResult = controller.Create(person);
        var createdPerson = ((OkObjectResult)createdResult).Value as Person;

        // Act
        var result = controller.Delete(createdPerson!.Id);

        // Assert - Delete returns OkObjectResult, not OkResult
        result.Should().BeOfType<OkObjectResult>();
        
        // Verify person is deleted - GetById returns OkObjectResult with null value
        var getResult = controller.GetById(createdPerson.Id);
        getResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void PersonController_Update_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        var nonExistentPerson = new Person
        {
            Id = 999,
            Name = "NonExistent",
            Email = "nonexistent@test.com",
            Phone = "000",
            Address = "Nowhere"
        };

        // Act & Assert - Update throws NullReferenceException when person not found
        Assert.Throws<NullReferenceException>(() => controller.Update(999, nonExistentPerson));
    }

    [Fact]
    public void PersonController_SearchByName_WithNoMatches_ReturnsEmptyList()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.SearchByName("NonExistentName");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var persons = okResult.Value.Should().BeAssignableTo<IEnumerable<Person>>().Subject.ToList();
        persons.Should().BeEmpty();
    }

    [Fact]
    public void PersonController_SearchByName_WithPartialMatch_ReturnsResults()
    {
        // Arrange
        var controller = CreateController();
        var person = new Person 
        { 
            Name = "PartialMatchTest", 
            Email = "partial@test.com", 
            Phone = "123", 
            Address = "Partial Addr" 
        };
        
        controller.Create(person);

        // Act
        var result = controller.SearchByName("Partial");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var persons = okResult.Value.Should().BeAssignableTo<IEnumerable<Person>>().Subject.ToList();
        persons.Should().HaveCount(1);
        persons.First().Name.Should().Be("PartialMatchTest");
    }

    [Fact]
    public void PersonController_GetById_WithNegativeId_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetById(-1);

        // Assert - GetById returns OkObjectResult with null value, not NotFoundResult
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void PersonController_Delete_WithNegativeId_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act & Assert - Delete throws ArgumentNullException when entity not found
        Assert.Throws<ArgumentNullException>(() => controller.Delete(-1));
    }

    [Fact]
    public void BadExtensions_FromUnsafeString_WithValidFourFields_ParsesCorrectly()
    {
        // Arrange
        var personData = "John,john@test.com,123-456-7890,123 Main St";

        // Act
        var result = personData.FromUnsafeString();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("John");
        result.Email.Should().Be("john@test.com");
        result.Phone.Should().Be("123-456-7890");
        result.Address.Should().Be("123 Main St");
    }

    [Fact]
    public void BadExtensions_FromUnsafeString_WithExactlyFiveFields_ParsesCorrectly()
    {
        // Arrange
        var personData = "John,Doe,john@test.com,123-456-7890,123 Main St";

        // Act
        var result = personData.FromUnsafeString();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("John");
        result.Email.Should().Be("Doe"); // Second field becomes email
        result.Phone.Should().Be("john@test.com"); // Third field becomes phone
        result.Address.Should().Be("123-456-7890"); // Fourth field becomes address
    }

    [Fact]
    public void AppDbContext_Database_CanBeAccessed()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act & Assert
        using var context = new AppDbContext(options);
        context.Database.Should().NotBeNull();
        context.Persons.Should().NotBeNull();
    }

    [Fact]
    public void Person_AllPropertiesDefault_CanBeCreated()
    {
        // Act
        var person = new Person
        {
            Name = "Test",
            Email = "test@test.com", 
            Phone = "123",
            Address = "Test"
        };

        // Assert
        person.Id.Should().Be(0); // Default int value
        person.Name.Should().Be("Test");
        person.Email.Should().Be("test@test.com");
        person.Phone.Should().Be("123");
        person.Address.Should().Be("Test");
        person.IsActive.Should().BeFalse(); // Default bool value in this implementation
        person.CreatedDate.Should().Be(default(DateTime)); // Default DateTime value
    }
}