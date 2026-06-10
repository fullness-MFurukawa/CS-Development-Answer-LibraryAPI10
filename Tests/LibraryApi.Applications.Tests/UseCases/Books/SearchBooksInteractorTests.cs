using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.UseCases.Books;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using Moq;
namespace LibraryApi.Applications.Tests.UseCases.Books;
/// <summary>
/// SearchBooksInteractor の単体テスト
///
/// 依存する IBookService をモックに差し替え、キーワード検索の結果(集約 Book)が
/// DTO に変換されて返ることを検証する。Adapter は実物を用いる。
/// </summary>
[TestClass]
[TestCategory("UseCases.Books")]
public class SearchBooksInteractorTests
{
    /// <summary>
    /// テスト用の Book(集約)を生成するヘルパー
    /// ※ Book.Restore / BookStock.Restore の実際のシグネチャに合わせること
    /// </summary>
    private static Book CreateTestBook(string bookUuid, string title)
    {
        var category = Category.Restore("category-uuid-1", "技術書");
        var bookStock = BookStock.Restore("stock-uuid-1", 5);
        return Book.Restore(bookUuid, title, "山田太郎", category, bookStock);
    }

    /// <summary>
    /// 実物の BookDtoAdapter を生成する(分類変換は実物の CategoryDtoAdapter に委譲)
    /// </summary>
    private static IAdapter<Book, BookDto> CreateBookDtoAdapter()
    {
        IAdapter<Category, CategoryDto> categoryDtoAdapter = new CategoryDtoAdapter();
        return new BookDtoAdapter(categoryDtoAdapter);
    }

    [TestMethod(DisplayName = "キーワード検索の結果をDTOに変換して返す")]
    public async Task ExecuteAsync_TestCase01()
    {
        // Arrange:2件ヒットするよう設定する
        var books = new List<Book>
        {
            CreateTestBook("book-uuid-1", "ドメイン駆動設計入門"),
            CreateTestBook("book-uuid-2", "ドメイン駆動設計実践"),
        };

        var serviceMock = new Mock<IBookService>();
        serviceMock
            .Setup(s => s.FindByTitleKeywordAsync("ドメイン"))
            .ReturnsAsync(books);

        var interactor = new SearchBooksInteractor(serviceMock.Object, CreateBookDtoAdapter());

        // Act
        var result = await interactor.ExecuteAsync("ドメイン");

        // Assert:2件が DTO に変換されて返ること
        Assert.HasCount(2, result);
        Assert.AreEqual("book-uuid-1", result[0].BookId);
        Assert.AreEqual("ドメイン駆動設計入門", result[0].Title);
        Assert.AreEqual("山田太郎", result[0].Author);
        Assert.AreEqual(5, result[0].Stock);
        Assert.AreEqual("技術書", result[0].Category.Name);

        serviceMock.Verify(s => s.FindByTitleKeywordAsync("ドメイン"), Times.Once);
    }

    [TestMethod(DisplayName = "検索結果が0件なら空のリストを返す")]
    public async Task ExecuteAsync_TestCase02()
    {
        // Arrange:0件を返すよう設定する
        var serviceMock = new Mock<IBookService>();
        serviceMock
            .Setup(s => s.FindByTitleKeywordAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Book>());

        var interactor = new SearchBooksInteractor(serviceMock.Object, CreateBookDtoAdapter());

        // Act
        var result = await interactor.ExecuteAsync("該当なし");

        // Assert
        Assert.IsEmpty(result);
    }
}