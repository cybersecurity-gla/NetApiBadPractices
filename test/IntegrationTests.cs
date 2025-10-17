using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace BadApiExample.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real database context
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add an in-memory database for testing
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForIntegrationTesting");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PersonController_GetAll_ReturnsSuccessResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/Person");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PersonController_GetById_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/Person/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PersonController_Post_AcceptsValidPerson()
        {
            // Arrange
            var person = new
            {
                Name = "Integration Test Person",
                Email = "integration@test.com",
                Age = 30,
                Phone = "555-0123",
                Address = "123 Integration St"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(person),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/Person", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PersonController_SearchByName_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/Person/search/John");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task PersonController_GetDatabaseInfo_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/Person/debug/database");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task BadExamplesController_GetSystemConfig_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/BadExamples/admin/config");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task BadExamplesController_GetPrivateData_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/BadExamples/private-data");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task BadExamplesController_SearchPersons_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/BadExamples/search?name=John&email=john@test.com");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task BadExamplesController_ExecuteCommand_ReturnsResponse()
        {
            // Arrange
            var command = new { command = "SELECT 1" };
            var content = new StringContent(
                JsonSerializer.Serialize(command),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/BadExamples/execute", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task BadExamplesController_StressTest_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/BadExamples/stress");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task BadExamplesController_DeletePersonWithGet_ReturnsResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/BadExamples/delete/1");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }
    }
}