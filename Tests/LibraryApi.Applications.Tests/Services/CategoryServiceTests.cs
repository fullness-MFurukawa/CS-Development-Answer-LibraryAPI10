using LibraryApi.Applications.Services.Categories;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Moq;
namespace LibraryApi.Applications.Tests.Services;
/// <summary>
/// CategoryService の単体テスト
///
/// 依存する ICategoryRepository をモックに差し替え、Service がリポジトリへ
/// 正しく委譲し、その結果をそのまま返すことを検証する。
/// (CategoryService はリポジトリへの委譲に徹し、固有のロジックを持たない)
/// </summary>
[TestClass]
[TestCategory("Services")]
public class CategoryServiceTests
{
    [TestMethod(DisplayName = "リポジトリの結果をそのまま返す")]
    public async Task FindAllAsync_TestCase01()
    {
        // Arrange:リポジトリが2件の分類を返すよう設定する
        var categories = new List<Category>
        {
            Category.Restore("uuid-1", "技術書"),
            Category.Restore("uuid-2", "小説"),
        };

        var repositoryMock = new Mock<ICategoryRepository>();
        repositoryMock
            .Setup(r => r.FindAllAsync())
            .ReturnsAsync(categories);

        var service = new CategoryService(repositoryMock.Object);

        // Act
        var result = await service.FindAllAsync();

        // Assert:リポジトリの結果がそのまま返ること
        Assert.HasCount(2, result);
        Assert.AreEqual("uuid-1", result[0].CategoryUuid);
        Assert.AreEqual("小説", result[1].Name);

        // リポジトリの FindAllAsync が1回だけ呼ばれたことを確認する
        repositoryMock.Verify(r => r.FindAllAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "リポジトリへ識別Idを渡し結果をそのまま返す")]
    public async Task FindByUuidAsync_TestCase01()
    {
        // Arrange:特定の UUID で分類が返るよう設定する
        var category = Category.Restore("uuid-1", "技術書");

        var repositoryMock = new Mock<ICategoryRepository>();
        repositoryMock
            .Setup(r => r.FindByUuidAsync("uuid-1"))
            .ReturnsAsync(category);

        var service = new CategoryService(repositoryMock.Object);

        // Act
        var result = await service.FindByUuidAsync("uuid-1");

        // Assert:リポジトリの結果がそのまま返ること
        Assert.IsNotNull(result);
        Assert.AreEqual("uuid-1", result.CategoryUuid);
        Assert.AreEqual("技術書", result.Name);

        // 正しい引数でリポジトリが呼ばれたことを確認する
        repositoryMock.Verify(r => r.FindByUuidAsync("uuid-1"), Times.Once);
    }

    [TestMethod(DisplayName = "該当なしの場合はnullを返す")]
    public async Task FindByUuidAsync_TestCase02()
    {
        // Arrange:リポジトリが null を返すよう設定する
        var repositoryMock = new Mock<ICategoryRepository>();
        repositoryMock
            .Setup(r => r.FindByUuidAsync(It.IsAny<string>()))
            .ReturnsAsync((Category?)null);

        var service = new CategoryService(repositoryMock.Object);

        // Act
        var result = await service.FindByUuidAsync("not-exist");

        // Assert:null がそのまま返ること
        Assert.IsNull(result);
    }
}