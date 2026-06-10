using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Services.Categories;
using LibraryApi.Applications.UseCases.Categories;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using Moq;
namespace LibraryApi.Applications.Tests.UseCases.Categories;
/// <summary>
/// FindCategoriesInteractor の単体テスト
///
/// 依存する ICategoryService をモックに差し替え、UseCase のロジック
/// (取得した分類を DTO に変換して返す)を検証する。
/// 変換の Adapter は実物(CategoryDtoAdapter)を用いて、変換結果まで通しで確認する。
/// </summary>
[TestClass]
[TestCategory("UseCases/Categories")]
public class FindCategoriesInteractorTests
{
    [TestMethod(DisplayName = "分類を取得しDTOに変換して返す")]
    public async Task ExecuteAsync_TestCase01()
    {
        // Arrange:モックの ICategoryService が、2件の分類を返すよう設定する
        var categories = new List<Category>
        {
            Category.Restore("uuid-1", "技術書"),
            Category.Restore("uuid-2", "小説"),
        };

        var serviceMock = new Mock<ICategoryService>();
        serviceMock
            .Setup(s => s.FindAllAsync())
            .ReturnsAsync(categories);

        // Adapter は実物を使用する
        IAdapter<Category, CategoryDto> adapter = new CategoryDtoAdapter();

        var interactor = new FindCategoriesInteractor(serviceMock.Object, adapter);

        // Act
        var result = await interactor.ExecuteAsync();

        // Assert:2件が、正しく DTO に変換されて返ること
        Assert.HasCount(2, result);
        Assert.AreEqual("uuid-1", result[0].CategoryUuid);
        Assert.AreEqual("技術書", result[0].Name);
        Assert.AreEqual("uuid-2", result[1].CategoryUuid);
        Assert.AreEqual("小説", result[1].Name);

        serviceMock.Verify(s => s.FindAllAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "分類が0件なら空のリストを返す")]
    public async Task ExecuteAsync_TestCase02()
    {
        // Arrange:0件を返すよう設定する
        var serviceMock = new Mock<ICategoryService>();
        serviceMock
            .Setup(s => s.FindAllAsync())
            .ReturnsAsync(new List<Category>());

        IAdapter<Category, CategoryDto> adapter = new CategoryDtoAdapter();
        var interactor = new FindCategoriesInteractor(serviceMock.Object, adapter);

        // Act
        var result = await interactor.ExecuteAsync();

        // Assert
        Assert.IsEmpty(result);
    }
}