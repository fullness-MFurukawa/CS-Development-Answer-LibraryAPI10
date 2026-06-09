using LibraryApi.Infrastructure.Adapters;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
namespace LibraryApi.Infrastructure.Tests.Adapters;
/// <summary>
/// CategoryAdapter の単体テストドライバ
/// Category(ドメイン)と CategoryEntity(EF Core)の相互変換を検証する
/// 変換のみを対象とし、DB アクセスは伴わない
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class CategoryAdapterTests
{
    private readonly CategoryAdapter _adapter = new();

    // ───────────────────────────────────────────
    // Convert(ドメイン → EF Core エンティティ)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "ドメインのCategoryをCategoryEntityに変換する")]
    public void Convert_TestCase1()
    {
        // Arrange
        var category = Category.Create("技術書");

        // Act
        var entity = _adapter.Convert(category);

        // Assert
        Assert.AreEqual(category.CategoryUuid, entity.CategoryUuid);
        Assert.AreEqual("技術書", entity.Name);
    }

    [TestMethod(DisplayName = "変換時にIdと日時は設定されない")]
    public void Convert_TestCase2()
    {
        // Arrange
        var category = Category.Create("技術書");

        // Act
        var entity = _adapter.Convert(category);

        // Assert
        // Id は DB採番のため、変換時点では既定値(0)
        Assert.AreEqual(0, entity.Id);
        // 日時は DbContext が保存時に設定するため、変換時点では既定値
        Assert.AreEqual(default, entity.CreatedAt);
        Assert.AreEqual(default, entity.UpdatedAt);
    }

    // ───────────────────────────────────────────
    // Restore(EF Core エンティティ → ドメイン)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "CategoryEntityをドメインのCategoryに復元する")]
    public void Restore_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var entity = new CategoryEntity
        {
            Id = 1,
            CategoryUuid = uuid,
            Name = "小説",
        };

        // Act
        var category = _adapter.Restore(entity);

        // Assert
        // UUID と名前が引き継がれる(Id は復元に使われない)
        Assert.AreEqual(uuid, category.CategoryUuid);
        Assert.AreEqual("小説", category.Name);
    }

    [TestMethod(DisplayName = "復元時にドメインの検証が働きUUIDが空ならDomainExceptionをスローする")]
    public void Restore_TestCase2()
    {
        // Arrange
        // UUID が空の不正な EF Core エンティティ
        var entity = new CategoryEntity
        {
            CategoryUuid = "",
            Name = "小説",
        };

        // Act / Assert
        // Restore は Category.Restore に委譲するため、ドメインの検証が働く
        Assert.ThrowsExactly<DomainException>(() => _adapter.Restore(entity));
    }
}