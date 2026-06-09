using LibraryApi.Infrastructure.Adapters;
using LibraryApi.Infrastructure.Aggregators;
using LibraryApi.Infrastructure.Entities;
namespace LibraryApi.Infrastructure.Tests.Aggregators;
/// <summary>
/// BookAggregator の単体テストドライバ
/// BookEntity(分類・蔵書を内包)から、ドメインの集約 Book を構築することを検証する
/// 子の変換は実物の Adapter に委譲する(Adapter は別途単体テスト済み)
/// </summary>
[TestClass]
[TestCategory("Aggregators")]
public class BookAggregatorTests
{
    // 実物の Adapter を注入して Aggregator を生成する
    private readonly BookAggregator _aggregator =
        new(new CategoryAdapter(), new BookStockAdapter());

    /// <summary>
    /// テスト用に、分類・蔵書を内包した BookEntity を生成するヘルパー
    /// </summary>
    private static BookEntity CreateBookEntity()
    {
        return new BookEntity
        {
            Id = 1,
            BookUuid = "book-uuid-001",
            Title = "リーダブルコード",
            Author = "Dustin Boswell",
            CategoryId = 10,
            Category = new CategoryEntity
            {
                Id = 10,
                CategoryUuid = "category-uuid-001",
                Name = "技術書",
            },
            BookStock = new BookStockEntity
            {
                Id = 100,
                StockUuid = "stock-uuid-001",
                Stock = 3,
                BookId = 1,
            },
        };
    }

    [TestMethod(DisplayName = "BookEntityからBook集約を構築する")]
    public void Aggregate_TestCase1()
    {
        // Arrange
        var entity = CreateBookEntity();

        // Act
        var book = _aggregator.Aggregate(entity);

        // Assert
        // Book 自身のプロパティが構築される
        Assert.AreEqual("book-uuid-001", book.BookUuid);
        Assert.AreEqual("リーダブルコード", book.Title);
        Assert.AreEqual("Dustin Boswell", book.Author);
    }

    [TestMethod(DisplayName = "参照する分類が正しく構築される")]
    public void Aggregate_TestCase2()
    {
        // Arrange
        var entity = CreateBookEntity();

        // Act
        var book = _aggregator.Aggregate(entity);

        // Assert
        // 内包する CategoryEntity が、CategoryAdapter を介してドメインの Category に構築される
        Assert.IsNotNull(book.Category);
        Assert.AreEqual("category-uuid-001", book.Category.CategoryUuid);
        Assert.AreEqual("技術書", book.Category.Name);
    }

    [TestMethod(DisplayName = "内包する蔵書が正しく構築される")]
    public void Aggregate_TestCase3()
    {
        // Arrange
        var entity = CreateBookEntity();

        // Act
        var book = _aggregator.Aggregate(entity);

        // Assert
        // 内包する BookStockEntity が、BookStockAdapter を介してドメインの BookStock に構築される
        Assert.IsNotNull(book.BookStock);
        Assert.AreEqual("stock-uuid-001", book.BookStock.StockUuid);
        Assert.AreEqual(3, book.BookStock.Stock);
    }
}