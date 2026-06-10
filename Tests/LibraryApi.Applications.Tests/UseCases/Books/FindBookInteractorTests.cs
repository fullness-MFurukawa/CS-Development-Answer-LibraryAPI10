using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.UseCases.Books;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using Moq;

namespace LibraryApi.Applications.Tests.UseCases.Books;

/// <summary>
/// FindBookInteractor の単体テスト
///
/// 依存する IBookService をモックに差し替え、図書詳細取得のロジックを検証する。
/// ・見つかれば DTO を返す
/// ・見つからなければ NotFoundException をスローする
/// </summary>
[TestClass]
[TestCategory("UseCases.Books")]
public class FindBookInteractorTests
{
    private static Book CreateTestBook(string bookUuid, string title)
    {
        var category = Category.Restore("category-uuid-1", "技術書");
        var bookStock = BookStock.Restore("stock-uuid-1", 5);
        return Book.Restore(bookUuid, title, "山田太郎", category, bookStock);
    }

    private static IAdapter<Book, BookDto> CreateBookDtoAdapter()
    {
        IAdapter<Category, CategoryDto> categoryDtoAdapter = new CategoryDtoAdapter();
        return new BookDtoAdapter(categoryDtoAdapter);
    }

    [TestMethod(DisplayName = "図書が見つかればDTOを返す")]
    public async Task ExecuteAsync_TestCase01()
    {
        // Arrange:指定 UUID で図書が見つかるよう設定する
        var book = CreateTestBook("book-uuid-1", "ドメイン駆動設計入門");

        var serviceMock = new Mock<IBookService>();
        serviceMock
            .Setup(s => s.FindByUuidAsync("book-uuid-1"))
            .ReturnsAsync(book);

        var interactor = new FindBookInteractor(serviceMock.Object, CreateBookDtoAdapter());

        // Act
        var result = await interactor.ExecuteAsync("book-uuid-1");

        // Assert:DTO に変換されて返ること
        Assert.AreEqual("book-uuid-1", result.BookId);
        Assert.AreEqual("ドメイン駆動設計入門", result.Title);
        Assert.AreEqual(5, result.Stock);
        Assert.AreEqual("技術書", result.Category.Name);

        serviceMock.Verify(s => s.FindByUuidAsync("book-uuid-1"), Times.Once);
    }

    [TestMethod(DisplayName = "図書が見つからなければNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_TestCase02()
    {
        // Arrange:Service が null を返すよう設定する
        var serviceMock = new Mock<IBookService>();
        serviceMock
            .Setup(s => s.FindByUuidAsync(It.IsAny<string>()))
            .ReturnsAsync((Book?)null);

        var interactor = new FindBookInteractor(serviceMock.Object, CreateBookDtoAdapter());

        // Act & Assert:NotFoundException がスローされること
        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => interactor.ExecuteAsync("not-exist"));
    }
}