using WebInfrastructure.Models;
using Xunit;

namespace GadgetsOnline.Tests
{
    public class ProductTests
    {
        [Fact]
        public void Product_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var product = new Product();

            // Assert
            Assert.Equal(0, product.ProductId);
            Assert.Equal(0, product.CategoryId);
            Assert.Equal(string.Empty, product.Name);
            Assert.Equal(0m, product.Price);
            Assert.Equal(string.Empty, product.ProductArtUrl);
            Assert.NotNull(product.Category);
        }

        [Fact]
        public void Product_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var product = new Product();
            var category = new Category { CategoryId = 1, Name = "Electronics" };

            // Act
            product.ProductId = 10;
            product.CategoryId = 1;
            product.Name = "Smartphone";
            product.Price = 599.99m;
            product.ProductArtUrl = "image.jpg";
            product.Category = category;

            // Assert
            Assert.Equal(10, product.ProductId);
            Assert.Equal(1, product.CategoryId);
            Assert.Equal("Smartphone", product.Name);
            Assert.Equal(599.99m, product.Price);
            Assert.Equal("image.jpg", product.ProductArtUrl);
            Assert.Same(category, product.Category);
        }
    }
}