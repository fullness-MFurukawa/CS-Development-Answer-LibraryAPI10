using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.UseCases.Categories;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.Controllers;
using LibraryApi.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LibraryApi.Presentations.Tests.Controllers;
/// <summary>
/// CategoriesController の単体テスト
///
/// UseCase をモックにし、Controller が UseCase を呼び出して結果を
/// 正しいレスポンス(200 OK + 変換済み ViewModel)として返すことを検証する。
/// Adapter は実物を用いる(純粋な変換のため)。
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class CategoriesControllerTests
{
    [TestMethod(DisplayName = "分類一覧を取得し200 OKで返す")]
    public async Task GetCategories_TestCase01()
    {
        // Arrange:UseCase が 2 件の分類 DTO を返すようモックする
        var dtos = new List<CategoryDto>
        {
            new CategoryDto { CategoryUuid = "uuid-1", Name = "技術書" },
            new CategoryDto { CategoryUuid = "uuid-2", Name = "小説" },
        };

        var useCaseMock = new Mock<IFindCategoriesUseCase>();
        useCaseMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(dtos);

        // Adapter は実物を用いる
        IAdapter<CategoryDto, CategoryResponse> adapter = new CategoryResponseAdapter();

        var controller = new CategoriesController(useCaseMock.Object, adapter);

        // Act
        var actionResult = await controller.GetCategories();

        // Assert:200 OK であること
        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        // Assert:変換された ViewModel が 2 件返ること
        var responses = okResult.Value as IEnumerable<CategoryResponse>;
        Assert.IsNotNull(responses);
        var list = responses.ToList();
        Assert.HasCount(2, list);
        Assert.AreEqual("uuid-1", list[0].CategoryId);
        Assert.AreEqual("技術書", list[0].Name);

        // UseCase が呼ばれたこと
        useCaseMock.Verify(u => u.ExecuteAsync(), Times.Once);
    }
}