using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.Services.Categories;
using LibraryApi.Applications.UseCases.Books;
using LibraryApi.Applications.UseCases.UnitOfWorks;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using Moq;
namespace LibraryApi.Applications.Tests.UseCases.Books;
/// <summary>
/// RegisterBookInteractor の単体テスト
///
/// 図書登録のロジックを検証する。
/// ・分類が存在しなければ InvalidInputException(トランザクションは開始しない)
/// ・正常時は構築・保存し、トランザクションをコミットする
/// ・保存失敗時はロールバックして例外を再スローする
/// </summary>
[TestClass]
[TestCategory("UseCases.Books")]
public class RegisterBookInteractorTests
{
    private static IAdapter<Book, BookDto> CreateBookDtoAdapter()
    {
        IAdapter<Category, CategoryDto> categoryDtoAdapter = new CategoryDtoAdapter();
        return new BookDtoAdapter(categoryDtoAdapter);
    }

    [TestMethod(DisplayName = "正常時:構築・保存しコミットしてDTOを返す")]
    public async Task ExecuteAsync_TestCase01()
    {
        // Arrange
        var category = Category.Restore("category-uuid-1", "技術書");

        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock
            .Setup(s => s.FindByUuidAsync("category-uuid-1"))
            .ReturnsAsync(category);

        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock
            .Setup(s => s.AddAsync(It.IsAny<Book>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var input = new RegisterBookDto
        {
            Title = "ドメイン駆動設計入門",
            Author = "山田太郎",
            CategoryId = "category-uuid-1",
            Stock = 5,
        };

        var interactor = new RegisterBookInteractor(
            categoryServiceMock.Object,
            bookServiceMock.Object,
            unitOfWorkMock.Object,
            CreateBookDtoAdapter());

        // Act
        var result = await interactor.ExecuteAsync(input);

        // Assert:入力どおりの DTO が返ること
        Assert.AreEqual("ドメイン駆動設計入門", result.Title);
        Assert.AreEqual("山田太郎", result.Author);
        Assert.AreEqual(5, result.Stock);
        Assert.AreEqual("技術書", result.Category.Name);

        // 保存とトランザクション制御の検証
        bookServiceMock.Verify(s => s.AddAsync(It.IsAny<Book>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never); // 正常時はロールバックしない
    }

    [TestMethod(DisplayName = "分類が存在しなければInvalidInputExceptionをスローする")]
    public async Task ExecuteAsync_TestCase02()
    {
        // Arrange:分類が見つからない
        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock
            .Setup(s => s.FindByUuidAsync(It.IsAny<string>()))
            .ReturnsAsync((Category?)null);

        var bookServiceMock = new Mock<IBookService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var input = new RegisterBookDto
        {
            Title = "ドメイン駆動設計入門",
            Author = "山田太郎",
            CategoryId = "not-exist",
            Stock = 5,
        };

        var interactor = new RegisterBookInteractor(
            categoryServiceMock.Object,
            bookServiceMock.Object,
            unitOfWorkMock.Object,
            CreateBookDtoAdapter());

        // Act & Assert:InvalidInputException がスローされること
        await Assert.ThrowsExactlyAsync<InvalidInputException>(
            () => interactor.ExecuteAsync(input));

        // トランザクションは開始されず、保存も行われないこと
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        bookServiceMock.Verify(s => s.AddAsync(It.IsAny<Book>()), Times.Never);
    }

    [TestMethod(DisplayName = "保存失敗時:ロールバックして例外を再スローする")]
    public async Task ExecuteAsync_TestCase03()
    {
        // Arrange:分類はあるが、保存で例外が発生する
        var category = Category.Restore("category-uuid-1", "技術書");

        var categoryServiceMock = new Mock<ICategoryService>();
        categoryServiceMock
            .Setup(s => s.FindByUuidAsync("category-uuid-1"))
            .ReturnsAsync(category);

        var bookServiceMock = new Mock<IBookService>();
        bookServiceMock
            .Setup(s => s.AddAsync(It.IsAny<Book>()))
            .ThrowsAsync(new Exception("DB エラー"));

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var input = new RegisterBookDto
        {
            Title = "ドメイン駆動設計入門",
            Author = "山田太郎",
            CategoryId = "category-uuid-1",
            Stock = 5,
        };

        var interactor = new RegisterBookInteractor(
            categoryServiceMock.Object,
            bookServiceMock.Object,
            unitOfWorkMock.Object,
            CreateBookDtoAdapter());

        // Act & Assert:例外が再スローされること
        await Assert.ThrowsExactlyAsync<Exception>(
            () => interactor.ExecuteAsync(input));

        // ロールバックが呼ばれ、コミットは呼ばれないこと
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}