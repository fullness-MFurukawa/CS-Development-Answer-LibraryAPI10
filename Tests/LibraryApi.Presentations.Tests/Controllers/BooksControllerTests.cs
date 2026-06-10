using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.UseCases.Books;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.Controllers;
using LibraryApi.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LibraryApi.Presentations.Tests.Controllers;
/// <summary>
/// BooksController の単体テスト
///
/// 各 UseCase をモックにし、Controller が正しいレスポンス(200/201/204)を返すこと、
/// および UseCase の例外を素通しすることを検証する。Adapter は実物を用いる。
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class BooksControllerTests
{
    // テスト対象の Controller を、必要なモックと実物 Adapter で組み立てるヘルパ
    private static BooksController CreateController(
        Mock<ISearchBooksUseCase>? searchMock = null,
        Mock<IFindBookUseCase>? findMock = null,
        Mock<IRegisterBookUseCase>? registerMock = null,
        Mock<IUpdateBookUseCase>? updateMock = null,
        Mock<IDeleteBookUseCase>? deleteMock = null)
    {
        var bookResponseAdapter = new BookResponseAdapter(new CategoryResponseAdapter());
        var registerAdapter = new RegisterBookRequestAdapter();
        var updateAdapter = new UpdateBookRequestAdapter();

        return new BooksController(
            (searchMock ?? new Mock<ISearchBooksUseCase>()).Object,
            (findMock ?? new Mock<IFindBookUseCase>()).Object,
            (registerMock ?? new Mock<IRegisterBookUseCase>()).Object,
            (updateMock ?? new Mock<IUpdateBookUseCase>()).Object,
            (deleteMock ?? new Mock<IDeleteBookUseCase>()).Object,
            bookResponseAdapter,
            registerAdapter,
            updateAdapter);
    }

    private static BookDto SampleBookDto(string id = "book-1") => new BookDto
    {
        BookId = id,
        Title = "ドメイン駆動設計入門",
        Author = "Eric Evans",
        Category = new CategoryDto { CategoryUuid = "cat-1", Name = "技術書" },
        Stock = 3,
    };

    [TestMethod(DisplayName = "詳細取得:存在する図書を200 OKで返す")]
    public async Task GetBook_TestCase01()
    {
        var findMock = new Mock<IFindBookUseCase>();
        findMock.Setup(u => u.ExecuteAsync("book-1")).ReturnsAsync(SampleBookDto());

        var controller = CreateController(findMock: findMock);

        var actionResult = await controller.GetBook("book-1");

        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        var response = okResult.Value as BookResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("book-1", response.BookId);
    }

    [TestMethod(DisplayName = "詳細取得:UseCaseのNotFoundExceptionを素通しする")]
    public async Task GetBook_TestCase02()
    {
        var findMock = new Mock<IFindBookUseCase>();
        findMock
            .Setup(u => u.ExecuteAsync(It.IsAny<string>()))
            .ThrowsAsync(new NotFoundException("BookNotFound", "指定された図書が存在しません。"));

        var controller = CreateController(findMock: findMock);

        // Controller は例外をキャッチせず素通しする(404 への変換はミドルウェアの責務)
        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => controller.GetBook("missing"));
    }

    [TestMethod(DisplayName = "削除:204 No Contentを返す")]
    public async Task DeleteBook_TestCase01()
    {
        var deleteMock = new Mock<IDeleteBookUseCase>();
        deleteMock.Setup(u => u.ExecuteAsync("book-1")).Returns(Task.CompletedTask);

        var controller = CreateController(deleteMock: deleteMock);

        var result = await controller.DeleteBook("book-1");

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        deleteMock.Verify(u => u.ExecuteAsync("book-1"), Times.Once);
    }
}