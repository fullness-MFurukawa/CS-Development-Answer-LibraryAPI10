using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.UseCases.Books;
using LibraryApi.Applications.UseCases.UnitOfWorks;
using LibraryApi.Domains.Models;
using Moq;
namespace LibraryApi.Applications.Tests.UseCases.Books;
/// <summary>
/// DeleteBookInteractor の単体テスト
///
/// 図書削除のロジックを検証する。
/// ・対象が存在しなければ NotFoundException(トランザクションは開始しない)
/// ・正常時は削除し、トランザクションをコミットする
/// ・削除失敗時はロールバックして例外を再スローする
/// </summary>
[TestClass]
[TestCategory("UseCases.Books")]
public class DeleteBookInteractorTests
{
    private static Book CreateTestBook(string bookUuid, string title)
    {
        var category = Category.Restore("category-uuid-1", "技術書");
        var bookStock = BookStock.Restore("stock-uuid-1", 5);
        return Book.Restore(bookUuid, title, "山田太郎", category, bookStock);
    }

    [TestMethod(DisplayName = "正常時:削除しコミットする")]
    public async Task ExecuteAsync_TestCase01()
    {
        // Arrange:対象の図書が存在する
        var book = CreateTestBook("book-uuid-1", "ドメイン駆動設計入門");

        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock
            .Setup(s => s.FindByUuidAsync("book-uuid-1"))
            .ReturnsAsync(book);
        bookServiceMock
            .Setup(s => s.DeleteAsync(It.IsAny<Book>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var interactor = new DeleteBookInteractor(
            bookServiceMock.Object,
            unitOfWorkMock.Object);

        // Act
        await interactor.ExecuteAsync("book-uuid-1");

        // Assert:削除とトランザクション制御の検証
        bookServiceMock.Verify(s => s.DeleteAsync(book), Times.Once);
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

        var interactor = new DeleteBookInteractor(
            bookServiceMock.Object,
            unitOfWorkMock.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => interactor.ExecuteAsync("not-exist"));

        // トランザクションは開始されず、削除も行われないこと
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        bookServiceMock.Verify(s => s.DeleteAsync(It.IsAny<Book>()), Times.Never);
    }

    [TestMethod(DisplayName = "削除失敗時:ロールバックして例外を再スローする")]
    public async Task ExecuteAsync_TestCase03()
    {
        // Arrange:対象はあるが、削除で例外が発生する
        var book = CreateTestBook("book-uuid-1", "ドメイン駆動設計入門");

        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock
            .Setup(s => s.FindByUuidAsync("book-uuid-1"))
            .ReturnsAsync(book);
        bookServiceMock
            .Setup(s => s.DeleteAsync(It.IsAny<Book>()))
            .ThrowsAsync(new Exception("DB エラー"));

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var interactor = new DeleteBookInteractor(
            bookServiceMock.Object,
            unitOfWorkMock.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<Exception>(
            () => interactor.ExecuteAsync("book-uuid-1"));

        // ロールバックが呼ばれ、コミットは呼ばれないこと
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}