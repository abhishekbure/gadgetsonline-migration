using WebInfrastructure.Models;
using Xunit;

namespace GadgetsOnline.Tests
{
    public class CategoryTests
    {
        [Fact]
        public void Category_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var category = new Category();

            // Assert
            Assert.Equal(0, category.CategoryId);
            Assert.Equal(string.Empty, category.Name);
            Assert.Equal(string.Empty, category.Description);
            Assert.NotNull(category.Products);
            Assert.Empty(category.Products);
        }

        [Fact]
        public void Category_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var category = new Category();
            var products = new List<Product> { new Product() };

            // Act
            category.CategoryId = 1;
            category.Name = "Electronics";
            category.Description = "Electronic devices";
            category.Products = products;

            // Assert
            Assert.Equal(1, category.CategoryId);
            Assert.Equal("Electronics", category.Name);
            Assert.Equal("Electronic devices", category.Description);
            Assert.Same(products, category.Products);
        }
    }
}