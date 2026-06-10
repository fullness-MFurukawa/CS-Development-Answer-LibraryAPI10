using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Models;

namespace LibraryApi.Applications.Tests.Adapters;

/// <summary>
/// CategoryDtoAdapter の単体テスト
///
/// ドメインエンティティ Category と DTO CategoryDto の変換を検証する。
/// 変換ロジックそのものが対象のため、モックは用いない。
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class CategoryDtoAdapterTests
{
    [TestMethod(DisplayName = "Convert:ドメインをDTOに変換する")]
    public void Convert_TestCase01()
    {
        // Arrange
        var category = Category.Restore("category-uuid-1", "技術書");
        var adapter = new CategoryDtoAdapter();

        // Act
        var dto = adapter.Convert(category);

        // Assert:各プロパティが正しく移ること
        Assert.AreEqual("category-uuid-1", dto.CategoryUuid);
        Assert.AreEqual("技術書", dto.Name);
    }

    [TestMethod(DisplayName = "Restore:現状サポートしておらず例外をスローする")]
    public void Restore_TestCase01()
    {
        // Arrange
        var dto = new CategoryDto { CategoryUuid = "category-uuid-1", Name = "技術書" };
        var adapter = new CategoryDtoAdapter();

        // Act & Assert:NotSupportedException がスローされること
        Assert.ThrowsExactly<NotSupportedException>(() => adapter.Restore(dto));
    }
}