using LibraryApi.Applications.Dtos;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Tests.Adapters;
/// <summary>
/// BookResponseAdapter の単体テスト
///
/// ・Convert : BookDto → BookResponse(入れ子の分類変換を CategoryResponseAdapter に委譲)
/// ・Restore : サポートしないため NotSupportedException となること
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class BookResponseAdapterTests
{
    private static BookResponseAdapter CreateAdapter()
    {
        // 入れ子の分類変換を担う、実物の CategoryResponseAdapter を注入する
        return new BookResponseAdapter(new CategoryResponseAdapter());
    }

    [TestMethod(DisplayName = "Convert:BookDtoをBookResponseに変換する(入れ子の分類も変換される)")]
    public void Convert_TestCase01()
    {
        // Arrange
        var adapter = CreateAdapter();
        var dto = new BookDto
        {
            BookId = "book-uuid-001",
            Title = "ドメイン駆動設計入門",
            Author = "Eric Evans",
            Category = new CategoryDto { CategoryUuid = "category-uuid-001", Name = "技術書" },
            Stock = 3,
        };

        // Act
        var response = adapter.Convert(dto);

        // Assert:図書のプロパティ
        Assert.AreEqual("book-uuid-001", response.BookId);
        Assert.AreEqual("ドメイン駆動設計入門", response.Title);
        Assert.AreEqual("Eric Evans", response.Author);
        Assert.AreEqual(3, response.Stock);

        // Assert:入れ子の分類が変換されていること
        Assert.IsNotNull(response.Category);
        Assert.AreEqual("category-uuid-001", response.Category.CategoryId);
        Assert.AreEqual("技術書", response.Category.Name);
    }

    [TestMethod(DisplayName = "Restore:サポートしないためNotSupportedExceptionとなる")]
    public void Restore_TestCase01()
    {
        var adapter = CreateAdapter();
        var response = new BookResponse();

        Assert.ThrowsExactly<NotSupportedException>(() => adapter.Restore(response));
    }
}