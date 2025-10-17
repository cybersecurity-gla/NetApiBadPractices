using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace BadApiExample.Tests
{
    public class BadDataServiceTests : IDisposable
    {
        private readonly BadDataService _service;

        public BadDataServiceTests()
        {
            _service = BadDataService.Instance;
        }

        [Fact]
        public void Instance_ShouldNotBeNull()
        {
            // Assert
            BadDataService.Instance.Should().NotBeNull();
        }

        // Note: Skipping database connection tests as they would fail without a real SQL Server instance
        // This represents partial test coverage as requested (50%)

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}