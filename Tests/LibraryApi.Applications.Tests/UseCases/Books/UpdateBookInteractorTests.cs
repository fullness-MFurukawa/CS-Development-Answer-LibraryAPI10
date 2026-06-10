using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.UseCases.Books;
using LibraryApi.Applications.UseCases.UnitOfWorks; 
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using Moq;
namespace LibraryApi.Applications.Tests.UseCases;
/// <summary>
/// UpdateBookInteractor の単体テスト
///
/// 図書変更のロジックを検証する。
/// ・対象が存在しなければ NotFoundException(トランザクションは開始しない)
/// ・正常時は取得・変更・更新し、トランザクションをコミットする
/// ・更新失敗時はロールバックして例外を再スローする
/// </summary>
[TestClass]
[TestCategory("UseCases.Books")]
public class UpdateBookInteractorTests
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

    [TestMethod(DisplayName = "正常時:取得・変更・更新しコミットしてDTOを返す")]
    public async Task ExecuteAsync_TestCase01()
    {
        // Arrange:対象の図書が存在する
        var book = CreateTestBook("book-uuid-1", "旧タイトル");

        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock
            .Setup(s => s.FindByUuidAsync("book-uuid-1"))
            .ReturnsAsync(book);
        bookServiceMock
            .Setup(s => s.UpdateAsync(It.IsAny<Book>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var input = new UpdateBookDto
        {
            Title = "新タイトル",
            Author = "佐藤花子",
            Stock = 10,
        };

        var interactor = new UpdateBookInteractor(
            bookServiceMock.Object,
            unitOfWorkMock.Object,
            CreateBookDtoAdapter());

        // Act
        var result = await interactor.ExecuteAsync("book-uuid-1", input);

        // Assert:変更後の値が DTO に反映されていること
        Assert.AreEqual("book-uuid-1", result.BookId);
        Assert.AreEqual("新タイトル", result.Title);
        Assert.AreEqual("佐藤花子", result.Author);
        Assert.AreEqual(10, result.Stock);

        // トランザクション制御の検証
        bookServiceMock.Verify(s => s.UpdateAsync(It.IsAny<Book>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [TestMethod(DisplayName = "対象が存在しなければNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_TestCase02()
    {
        // Arrange:図書が見つからない
        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock
            .Setup(s => s.FindByUuidAsync(It.IsAny<string>()))
            .ReturnsAsync((Book?)null);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var input = new UpdateBookDto { Title = "新タイトル", Author = "佐藤花子", Stock = 10 };

        var interactor = new UpdateBookInteractor(
            bookServiceMock.Object,
            unitOfWorkMock.Object,
            CreateBookDtoAdapter());

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => interactor.ExecuteAsync("not-exist", input));

        // トランザクションは開始されず、更新も行われないこと
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        bookServiceMock.Verify(s => s.UpdateAsync(It.IsAny<Book>()), Times.Never);
    }

    [TestMethod(DisplayName = "更新失敗時:ロールバックして例外を再スローする")]
    public async Task ExecuteAsync_TestCase03()
    {
        // Arrange:対象はあるが、更新で例外が発生する
        var book = CreateTestBook("book-uuid-1", "旧タイトル");

        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock
            .Setup(s => s.FindByUuidAsync("book-uuid-1"))
            .ReturnsAsync(book);
        bookServiceMock
            .Setup(s => s.UpdateAsync(It.IsAny<Book>()))
            .ThrowsAsync(new Exception("DB エラー"));

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var input = new UpdateBookDto { Title = "新タイトル", Author = "佐藤花子", Stock = 10 };

        var interactor = new UpdateBookInteractor(
            bookServiceMock.Object,
            unitOfWorkMock.Object,
            CreateBookDtoAdapter());

        // Act & Assert
        await Assert.ThrowsExactlyAsync<Exception>(
            () => interactor.ExecuteAsync("book-uuid-1", input));

        // ロールバックが呼ばれ、コミットは呼ばれないこと
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}