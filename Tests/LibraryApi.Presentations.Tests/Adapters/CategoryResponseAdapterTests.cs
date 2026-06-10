using LibraryApi.Applications.Dtos;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Tests.Adapters;
/// <summary>
/// CategoryResponseAdapter の単体テスト
///
/// ・Convert : CategoryDto → CategoryResponse(DTO → ViewModel)が正しく変換されること
/// ・Restore : サポートしないため NotSupportedException となること
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class CategoryResponseAdapterTests
{
    [TestMethod(DisplayName = "Convert:CategoryDtoをCategoryResponseに変換する")]
    public void Convert_TestCase01()
    {
        // Arrange
        var adapter = new CategoryResponseAdapter();
        var dto = new CategoryDto
        {
            CategoryUuid = "category-uuid-001",
            Name = "技術書",
        };

        // Act
        var response = adapter.Convert(dto);

        // Assert
        Assert.AreEqual("category-uuid-001", response.CategoryId);
        Assert.AreEqual("技術書", response.Name);
    }

    [TestMethod(DisplayName = "Restore:サポートしないためNotSupportedExceptionとなる")]
    public void Restore_TestCase01()
    {
        // Arrange
        var adapter = new CategoryResponseAdapter();
        var response = new CategoryResponse { CategoryId = "x", Name = "y" };

        // Act & Assert
        Assert.ThrowsExactly<NotSupportedException>(() => adapter.Restore(response));
    }
}