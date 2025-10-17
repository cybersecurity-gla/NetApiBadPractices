using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;
using Microsoft.Data.SqlClient;

namespace BadApiExample.Tests
{
    public class BadExamplesControllerTests
    {
        private readonly BadExamplesController _controller;

        public BadExamplesControllerTests()
        {
            _controller = new BadExamplesController();
        }

        [Fact]
        public void GetSystemConfig_ReturnsOkResult_WithConfigData()
        {
            // Act
            var result = _controller.GetSystemConfig();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var config = okResult.Value;
            config.Should().NotBeNull();
        }

        [Fact]
        public void GetPrivateData_ReturnsOkResult_WithSensitiveData()
        {
            // Act
            var result = _controller.GetPrivateData();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value;
            data.Should().NotBeNull();
        }

        // Note: Skipping database-dependent tests (ExecuteCommand, UploadData, StressTest, SearchPersons)
        // to achieve approximately 50% coverage as requested
        // These methods would require a real database connection which is not available in test environment
    }
}