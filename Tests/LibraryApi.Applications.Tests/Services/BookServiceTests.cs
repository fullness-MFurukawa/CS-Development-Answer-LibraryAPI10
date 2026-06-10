using LibraryApi.Applications.Services.Books;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Moq;
namespace LibraryApi.Applications.Tests.Services;
/// <summary>
/// BookService の単体テスト
///
/// 依存する IBookRepository をモックに差し替え、Service がリポジトリへ
/// 正しく委譲し、その結果をそのまま返すことを検証する。
/// </summary>
[TestClass]
[TestCategory("Services")]
public class BookServiceTests
{
    /// <summary>
    /// テスト用の Book(集約)を生成するヘルパー
    /// ※ Book.Restore / Book.Create の実際のシグネチャに合わせて調整すること
    /// </summary>
    private static Book CreateTestBook(string bookUuid, string title)
    {
        var category = Category.Restore("category-uuid-1", "技術書");
        // BookStock を先に復元してから、Book に内包させる
        var bookStock = BookStock.Restore("stock-uuid-1", 5); 
        return Book.Restore(bookUuid, title, "山田太郎", category, bookStock);
    }

    [TestMethod(DisplayName = "キーワード検索の結果をそのまま返す")]
    public async Task FindByTitleKeywordAsync_TestCase01()
    {
        // Arrange:2件ヒットするよう設定する
        var books = new List<Book>
        {
            CreateTestBook("book-uuid-1", "ドメイン駆動設計入門"),
            CreateTestBook("book-uuid-2", "ドメイン駆動設計実践"),
        };

        var repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(r => r.FindByTitleKeywordAsync("ドメイン"))
            .ReturnsAsync(books);

        var service = new BookService(repositoryMock.Object);

        // Act
        var result = await service.FindByTitleKeywordAsync("ドメイン");

        // Assert
        Assert.HasCount(2, result);
        Assert.AreEqual("book-uuid-1", result[0].BookUuid);
        repositoryMock.Verify(r => r.FindByTitleKeywordAsync("ドメイン"), Times.Once);
    }

    [TestMethod(DisplayName = "キーワード検索で0件なら空のリストを返す")]
    public async Task FindByTitleKeywordAsync_TestCase02()
    {
        var repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(r => r.FindByTitleKeywordAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Book>());

        var service = new BookService(repositoryMock.Object);

        var result = await service.FindByTitleKeywordAsync("該当なし");

        Assert.IsEmpty(result);
    }

    [TestMethod(DisplayName = "識別Idで図書を取得し結果をそのまま返す")]
    public async Task FindByUuidAsync_TestCase01()
    {
        var book = CreateTestBook("book-uuid-1", "ドメイン駆動設計入門");

        var repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(r => r.FindByUuidAsync("book-uuid-1"))
            .ReturnsAsync(book);

        var service = new BookService(repositoryMock.Object);

        var result = await service.FindByUuidAsync("book-uuid-1");

        Assert.IsNotNull(result);
        Assert.AreEqual("book-uuid-1", result.BookUuid);
        repositoryMock.Verify(r => r.FindByUuidAsync("book-uuid-1"), Times.Once);
    }

    [TestMethod(DisplayName =  "該当なしの場合はnullを返す")]
    public async Task FindByUuidAsync_TestCase02()
    {
        var repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(r => r.FindByUuidAsync(It.IsAny<string>()))
            .ReturnsAsync((Book?)null);

        var service = new BookService(repositoryMock.Object);

        var result = await service.FindByUuidAsync("not-exist");

        Assert.IsNull(result);
    }

    [TestMethod(DisplayName = "図書をリポジトリへ渡して追加する")]
    public async Task AddAsync_TestCase01()
    {
        var book = CreateTestBook("book-uuid-1", "ドメイン駆動設計入門");

        var repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(r => r.AddAsync(book))
            .Returns(Task.CompletedTask);

        var service = new BookService(repositoryMock.Object);

        await service.AddAsync(book);

        repositoryMock.Verify(r => r.AddAsync(book), Times.Once);
    }

    [TestMethod(DisplayName = "図書をリポジトリへ渡して更新する")]
    public async Task UpdateAsync_TestCase01()
    {
        var book = CreateTestBook("book-uuid-1", "ドメイン駆動設計入門");

        var repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(r => r.UpdateAsync(book))
            .Returns(Task.CompletedTask);

        var service = new BookService(repositoryMock.Object);

        await service.UpdateAsync(book);

        repositoryMock.Verify(r => r.UpdateAsync(book), Times.Once);
    }

    [TestMethod(DisplayName = "図書をリポジトリへ渡して削除する")]
    public async Task DeleteAsync_TestCase01()
    {
        var book = CreateTestBook("book-uuid-1", "ドメイン駆動設計入門");

        var repositoryMock = new Mock<IBookRepository>();
        repositoryMock
            .Setup(r => r.DeleteAsync(book))
            .Returns(Task.CompletedTask);

        var service = new BookService(repositoryMock.Object);

        await service.DeleteAsync(book);

        repositoryMock.Verify(r => r.DeleteAsync(book), Times.Once);
    }
}