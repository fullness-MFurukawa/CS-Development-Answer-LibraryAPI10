using LibraryApi.Infrastructure.Adapters;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
namespace LibraryApi.Infrastructure.Tests.Adapters;
/// <summary>
/// BookStockAdapter の単体テストドライバ
/// BookStock(ドメイン)と BookStockEntity(EF Core)の相互変換を検証する
/// 特に、ドメインの BookStock が図書への参照(BookId)を持たないことに伴う、
/// BookId の非対称な扱いを確認する
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class BookStockAdapterTests
{
    private readonly BookStockAdapter _adapter = new();

    // ───────────────────────────────────────────
    // Convert(ドメイン → EF Core エンティティ)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "ドメインのBookStockをBookStockEntityに変換する")]
    public void Convert_TestCase1()
    {
        // Arrange
        var bookStock = BookStock.Create(5);

        // Act
        var entity = _adapter.Convert(bookStock);

        // Assert
        Assert.AreEqual(bookStock.StockUuid, entity.StockUuid);
        Assert.AreEqual(5, entity.Stock);
    }

    [TestMethod(DisplayName = "変換時にBookIdは設定されない")]
    public void Convert_TestCase2()
    {
        // Arrange
        var bookStock = BookStock.Create(5);

        // Act
        var entity = _adapter.Convert(bookStock);

        // Assert
        // ドメインの BookStock は図書への参照を持たないため、BookId は設定されない(既定値0)
        // BookId の解決は、集約を扱うリポジトリ側が担う
        Assert.AreEqual(0, entity.BookId);
    }

    [TestMethod(DisplayName = "変換時にIdと日時は設定されない")]
    public void Convert_TestCase3()
    {
        // Arrange
        var bookStock = BookStock.Create(5);

        // Act
        var entity = _adapter.Convert(bookStock);

        // Assert
        Assert.AreEqual(0, entity.Id);
        Assert.AreEqual(default, entity.CreatedAt);
        Assert.AreEqual(default, entity.UpdatedAt);
    }

    // ───────────────────────────────────────────
    // Restore(EF Core エンティティ → ドメイン)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "BookStockEntityをドメインのBookStockに復元する")]
    public void Restore_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var entity = new BookStockEntity
        {
            Id = 1,
            StockUuid = uuid,
            Stock = 10,
            BookId = 99, // BookId が設定されていても、復元には使われない
        };

        // Act
        var bookStock = _adapter.Restore(entity);

        // Assert
        // UUID と蔵書数が引き継がれる(Id・BookId は復元に使われない)
        Assert.AreEqual(uuid, bookStock.StockUuid);
        Assert.AreEqual(10, bookStock.Stock);
    }

    [TestMethod(DisplayName = "復元時にドメインの検証が働きUUIDが空ならDomainExceptionをスローする")]
    public void Restore_TestCase2()
    {
        // Arrange
        var entity = new BookStockEntity
        {
            StockUuid = "",
            Stock = 10,
        };

        // Act / Assert
        Assert.ThrowsExactly<DomainException>(() => _adapter.Restore(entity));
    }
}