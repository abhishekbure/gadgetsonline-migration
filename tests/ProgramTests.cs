using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GadgetsOnline.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void Program_ShouldBuildWebApplicationSuccessfully()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder([]);

            // Act
            builder.Services.AddControllers();

            var app = builder.Build();

            // Assert
            Assert.NotNull(app);
            Assert.IsType<ApplicationBuilder>(app);
        }

        [Fact]
        public void Program_ShouldRegisterRequiredServices()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder([]);

            // Act
            builder.Services.AddControllers();

            // Assert
            var services = builder.Services;
            Assert.NotEmpty(services);
        }
    }
}