using LibraryApi.Domains.Repositories;
using Microsoft.Extensions.DependencyInjection;
namespace LibraryApi.Infrastructures.Tests.Repositories;
/// <summary>
/// CategoryRepository の統合テストドライバ
/// 実際の library_db に接続し、取得結果を検証する
/// (RepositoryTestBase により、各テストはトランザクション内で実行されロールバックされる)
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class CategoryRepositoryTests : RepositoryTestBase
{
    // ───────────────────────────────────────────
    // FindAllAsync(全件取得)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "すべての分類を取得する")]
    public async Task FindAllAsync_TestCase1()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<ICategoryRepository>();

        // Act
        var categories = await repository.FindAllAsync();

        // Assert
        // 手順書で投入した分類は6件
        Assert.HasCount(6, categories);
    }

    [TestMethod(DisplayName = "取得した分類はUUIDと名前が復元されている")]
    public async Task FindAllAsync_TestCase2()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<ICategoryRepository>();

        // Act
        var categories = await repository.FindAllAsync();

        // Assert
        // 各分類が、UUID と名前を持つドメインエンティティとして正しく復元されていること
        Assert.IsTrue(categories.All(c => !string.IsNullOrEmpty(c.CategoryUuid)));
        Assert.IsTrue(categories.All(c => !string.IsNullOrEmpty(c.Name)));
    }

    // ───────────────────────────────────────────
    // FindByUuidAsync(1件取得)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "識別Idで分類を1件取得する")]
    public async Task FindByUuidAsync_TestCase1()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<ICategoryRepository>();
        // 全件から1件取り出し、その UUID を検索キーに使う(具体値をハードコードしない)
        var all = await repository.FindAllAsync();
        var target = all.First();

        // Act
        var category = await repository.FindByUuidAsync(target.CategoryUuid);

        // Assert
        Assert.IsNotNull(category);
        Assert.AreEqual(target.CategoryUuid, category.CategoryUuid);
        Assert.AreEqual(target.Name, category.Name);
    }

    [TestMethod(DisplayName = "存在しない識別Idではnullが返る")]
    public async Task FindByUuidAsync_TestCase2()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<ICategoryRepository>();

        // Act
        var category = await repository.FindByUuidAsync("non-existent-uuid");

        // Assert
        Assert.IsNull(category);
    }
}