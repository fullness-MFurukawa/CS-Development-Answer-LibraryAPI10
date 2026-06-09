using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Tests.Models;
/// <summary>
/// BookStockエンティティの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Models")]
public class BookStockTests
{
    // ───────────────────────────────────────────
    // Create(新規作成)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "有効な蔵書数を渡すとインスタンスを生成する")]
    public void Create_TestCase1()
    {
        // Arrange
        var stock = 5;

        // Act
        var bookStock = BookStock.Create(stock);

        // Assert
        Assert.AreEqual(stock, bookStock.Stock);
        // 新規作成時は UUID が自動採番される(空でないこと)
        Assert.IsFalse(string.IsNullOrEmpty(bookStock.StockUuid));
    }

    [TestMethod(DisplayName = "蔵書数が境界値の0でも生成できる")]
    public void Create_TestCase2()
    {
        // Arrange
        // 0冊(在庫切れ)は正常な状態であり、許可される
        var stock = 0;

        // Act
        var bookStock = BookStock.Create(stock);

        // Assert
        Assert.AreEqual(0, bookStock.Stock);
    }

    [TestMethod(DisplayName = "生成のたびに異なるUUIDが採番される")]
    public void Create_TestCase3()
    {
        // Arrange / Act
        var bookStock1 = BookStock.Create(5);
        var bookStock2 = BookStock.Create(5);

        // Assert
        // 同じ蔵書数でも、UUID は個別に採番されるため一致しない
        Assert.AreNotEqual(bookStock1.StockUuid, bookStock2.StockUuid);
    }

    [TestMethod]
    [DataRow(-1, DisplayName = "蔵書数が-1ならDomainExceptionをスローする")]
    [DataRow(-100, DisplayName = "蔵書数が大きなマイナス値ならDomainExceptionをスローする")]
    public void Create_TestCase4(int invalidStock)
    {
        // Act / Assert
        // 蔵書数は0以上でなければならない(マイナス値は許可しない)
        var ex = Assert.ThrowsExactly<DomainException>(
            () => BookStock.Create(invalidStock));
        Assert.AreEqual("stock", ex.ParamName);
    }

    // ───────────────────────────────────────────
    // Restore(復元)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "既存のUUIDと蔵書数から復元するとUUIDが引き継がれる")]
    public void Restore_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var stock = 10;

        // Act
        var bookStock = BookStock.Restore(uuid, stock);

        // Assert
        // 復元ではUUIDは新規採番されず、渡した値がそのまま引き継がれる
        Assert.AreEqual(uuid, bookStock.StockUuid);
        Assert.AreEqual(stock, bookStock.Stock);
    }

    [TestMethod]
    [DataRow("", DisplayName = "UUIDが空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "UUIDが空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "UUIDがnullならDomainExceptionをスローする")]
    public void Restore_TestCase2(string? invalidUuid)
    {
        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => BookStock.Restore(invalidUuid!, 10));
        Assert.AreEqual("stockUuid", ex.ParamName);
    }

    [TestMethod(DisplayName = "蔵書数が不正ならDomainExceptionをスローする")]
    public void Restore_TestCase3()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => BookStock.Restore(uuid, -1));
        Assert.AreEqual("stock", ex.ParamName);
    }

    // ───────────────────────────────────────────
    // ChangeStock(蔵書数の変更)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "有効な蔵書数を渡すと蔵書数が変更される")]
    public void ChangeStock_TestCase1()
    {
        // Arrange
        var bookStock = BookStock.Create(5);

        // Act
        bookStock.ChangeStock(20);

        // Assert
        Assert.AreEqual(20, bookStock.Stock);
    }

    [TestMethod(DisplayName = "蔵書数を境界値の0に変更できる")]
    public void ChangeStock_TestCase2()
    {
        // Arrange
        var bookStock = BookStock.Create(5);

        // Act
        bookStock.ChangeStock(0);

        // Assert
        Assert.AreEqual(0, bookStock.Stock);
    }

    [TestMethod(DisplayName = "不正な蔵書数を渡すとDomainExceptionをスローする")]
    public void ChangeStock_TestCase3()
    {
        // Arrange
        var bookStock = BookStock.Create(5);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => bookStock.ChangeStock(-1));
        Assert.AreEqual("stock", ex.ParamName);
    }

    [TestMethod(DisplayName = "変更が失敗しても元の蔵書数は保持される")]
    public void ChangeStock_TestCase4()
    {
        // Arrange
        var bookStock = BookStock.Create(5);

        // Act
        try
        {
            bookStock.ChangeStock(-1); // 失敗する
        }
        catch (DomainException)
        {
            // 握りつぶす(状態が変わっていないことを確認するため)
        }

        // Assert
        // バリデーションは値を代入する前に行われるため、元の蔵書数が保持される
        Assert.AreEqual(5, bookStock.Stock);
    }
}