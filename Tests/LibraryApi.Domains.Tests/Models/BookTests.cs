using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Tests.Models;
/// <summary>
/// Bookエンティティ(集約ルート)の単体テストドライバ
/// 集約ルートとして、内包する BookStock・参照する Category を正しく扱えること、
/// および図書情報の変更が内包する BookStock へ委譲されることを検証する
/// </summary>
[TestClass]
[TestCategory("Models")]
public class BookTests
{
    // テストで共通的に使う Category を生成するヘルパー
    private static Category CreateCategory() => Category.Create("技術書");

    // ───────────────────────────────────────────
    // Create(新規作成)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "有効な値を渡すとインスタンスを生成する")]
    public void Create_TestCase1()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var book = Book.Create("リーダブルコード", "Dustin Boswell", category, 3);

        // Assert
        Assert.AreEqual("リーダブルコード", book.Title);
        Assert.AreEqual("Dustin Boswell", book.Author);
        // 新規作成時は UUID が自動採番される(空でないこと)
        Assert.IsFalse(string.IsNullOrEmpty(book.BookUuid));
    }

    [TestMethod(DisplayName = "生成時に蔵書(BookStock)が内包され蔵書数が反映される")]
    public void Create_TestCase2()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var book = Book.Create("リーダブルコード", "Dustin Boswell", category, 3);

        // Assert
        // 集約ルートが蔵書を内包し、渡した蔵書数が反映されていること
        Assert.IsNotNull(book.BookStock);
        Assert.AreEqual(3, book.BookStock.Stock);
        // 内包する蔵書にも UUID が採番されていること
        Assert.IsFalse(string.IsNullOrEmpty(book.BookStock.StockUuid));
    }

    [TestMethod(DisplayName = "生成時に渡した分類(Category)が参照として保持される")]
    public void Create_TestCase3()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var book = Book.Create("リーダブルコード", "Dustin Boswell", category, 3);

        // Assert
        // 参照として渡した分類が、そのまま保持されていること
        Assert.AreSame(category, book.Category);
    }

    [TestMethod(DisplayName = "生成のたびに異なるUUIDが採番される")]
    public void Create_TestCase4()
    {
        // Arrange
        var category = CreateCategory();

        // Act
        var book1 = Book.Create("書籍A", "著者A", category, 1);
        var book2 = Book.Create("書籍B", "著者B", category, 1);

        // Assert
        Assert.AreNotEqual(book1.BookUuid, book2.BookUuid);
    }

    [TestMethod(DisplayName = "書名が境界値の50文字なら生成できる")]
    public void Create_TestCase5()
    {
        // Arrange
        var category = CreateCategory();
        var title = new string('あ', 50);

        // Act
        var book = Book.Create(title, "著者", category, 1);

        // Assert
        Assert.AreEqual(title, book.Title);
    }

    [TestMethod(DisplayName = "著者名が境界値の30文字なら生成できる")]
    public void Create_TestCase6()
    {
        // Arrange
        var category = CreateCategory();
        var author = new string('あ', 30);

        // Act
        var book = Book.Create("書名", author, category, 1);

        // Assert
        Assert.AreEqual(author, book.Author);
    }

    [TestMethod]
    [DataRow("", DisplayName = "書名が空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "書名が空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "書名がnullならDomainExceptionをスローする")]
    public void Create_TestCase7(string? invalidTitle)
    {
        // Arrange
        var category = CreateCategory();

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Create(invalidTitle!, "著者", category, 1));
        Assert.AreEqual("title", ex.ParamName);
    }

    [TestMethod(DisplayName = "書名が51文字以上ならDomainExceptionをスローする")]
    public void Create_TestCase8()
    {
        // Arrange
        var category = CreateCategory();
        var tooLongTitle = new string('あ', 51);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Create(tooLongTitle, "著者", category, 1));
        Assert.AreEqual("title", ex.ParamName);
    }

    [TestMethod]
    [DataRow("", DisplayName = "著者名が空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "著者名が空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "著者名がnullならDomainExceptionをスローする")]
    public void Create_TestCase9(string? invalidAuthor)
    {
        // Arrange
        var category = CreateCategory();

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Create("書名", invalidAuthor!, category, 1));
        Assert.AreEqual("author", ex.ParamName);
    }

    [TestMethod(DisplayName = "著者名が31文字以上ならDomainExceptionをスローする")]
    public void Create_TestCase10()
    {
        // Arrange
        var category = CreateCategory();
        var tooLongAuthor = new string('あ', 31);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Create("書名", tooLongAuthor, category, 1));
        Assert.AreEqual("author", ex.ParamName);
    }

    [TestMethod(DisplayName = "分類がnullならDomainExceptionをスローする")]
    public void Create_TestCase11()
    {
        // Act / Assert
        // 図書は必ずいずれかの分類に属する(UC-04 BR-03)
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Create("書名", "著者", null!, 1));
        Assert.AreEqual("category", ex.ParamName);
    }

    [TestMethod(DisplayName = "蔵書数が不正なら内包するBookStockがDomainExceptionをスローする")]
    public void Create_TestCase12()
    {
        // Arrange
        var category = CreateCategory();

        // Act / Assert
        // 蔵書数のバリデーションは内包する BookStock に委譲される
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Create("書名", "著者", category, -1));
        Assert.AreEqual("stock", ex.ParamName);
    }

    // ───────────────────────────────────────────
    // Restore(復元)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "既存の値から復元するとUUIDと内包する蔵書が引き継がれる")]
    public void Restore_TestCase1()
    {
        // Arrange
        var bookUuid = Guid.NewGuid().ToString();
        var category = CreateCategory();
        var stockUuid = Guid.NewGuid().ToString();
        var bookStock = BookStock.Restore(stockUuid, 7);

        // Act
        var book = Book.Restore(bookUuid, "Clean Code", "Robert C. Martin", category, bookStock);

        // Assert
        // 復元では UUID は新規採番されず、渡した値がそのまま引き継がれる
        Assert.AreEqual(bookUuid, book.BookUuid);
        Assert.AreEqual("Clean Code", book.Title);
        Assert.AreEqual("Robert C. Martin", book.Author);
        Assert.AreSame(category, book.Category);
        // 内包する蔵書も、渡したインスタンス(UUID・蔵書数)がそのまま引き継がれる
        Assert.AreSame(bookStock, book.BookStock);
        Assert.AreEqual(stockUuid, book.BookStock.StockUuid);
        Assert.AreEqual(7, book.BookStock.Stock);
    }

    [TestMethod]
    [DataRow("", DisplayName = "UUIDが空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "UUIDが空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "UUIDがnullならDomainExceptionをスローする")]
    public void Restore_TestCase2(string? invalidUuid)
    {
        // Arrange
        var category = CreateCategory();
        var bookStock = BookStock.Restore(Guid.NewGuid().ToString(), 7);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Restore(invalidUuid!, "書名", "著者", category, bookStock));
        Assert.AreEqual("bookUuid", ex.ParamName);
    }

    [TestMethod(DisplayName = "蔵書(BookStock)がnullならDomainExceptionをスローする")]
    public void Restore_TestCase3()
    {
        // Arrange
        var bookUuid = Guid.NewGuid().ToString();
        var category = CreateCategory();

        // Act / Assert
        // 復元元のデータに蔵書が欠けている状態は許可しない
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Restore(bookUuid, "書名", "著者", category, null!));
        Assert.AreEqual("bookStock", ex.ParamName);
    }

    [TestMethod(DisplayName = "分類がnullならDomainExceptionをスローする")]
    public void Restore_TestCase4()
    {
        // Arrange
        var bookUuid = Guid.NewGuid().ToString();
        var bookStock = BookStock.Restore(Guid.NewGuid().ToString(), 7);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Book.Restore(bookUuid, "書名", "著者", null!, bookStock));
        Assert.AreEqual("category", ex.ParamName);
    }

    // ───────────────────────────────────────────
    // ChangeBookInfo(図書情報の変更)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "書名・著者名・蔵書数が変更される")]
    public void ChangeBookInfo_TestCase1()
    {
        // Arrange
        var category = CreateCategory();
        var book = Book.Create("初版タイトル", "初版著者", category, 3);

        // Act
        book.ChangeBookInfo("改訂版タイトル", "改訂版著者", 10);

        // Assert
        Assert.AreEqual("改訂版タイトル", book.Title);
        Assert.AreEqual("改訂版著者", book.Author);
        // 蔵書数の変更が、内包する BookStock へ委譲されていること
        Assert.AreEqual(10, book.BookStock.Stock);
    }

    [TestMethod(DisplayName = "変更時も内包する蔵書のインスタンスは差し替わらない")]
    public void ChangeBookInfo_TestCase2()
    {
        // Arrange
        var category = CreateCategory();
        var book = Book.Create("タイトル", "著者", category, 3);
        var originalStock = book.BookStock;

        // Act
        book.ChangeBookInfo("タイトル", "著者", 10);

        // Assert
        // 蔵書数は ChangeStock で更新され、BookStock 自体は同じインスタンスのまま
        Assert.AreSame(originalStock, book.BookStock);
        Assert.AreEqual(10, book.BookStock.Stock);
    }

    [TestMethod(DisplayName = "書名が不正ならDomainExceptionをスローする")]
    public void ChangeBookInfo_TestCase3()
    {
        // Arrange
        var category = CreateCategory();
        var book = Book.Create("タイトル", "著者", category, 3);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => book.ChangeBookInfo("", "著者", 10));
        Assert.AreEqual("title", ex.ParamName);
    }

    [TestMethod(DisplayName = "蔵書数が不正なら内包するBookStockがDomainExceptionをスローする")]
    public void ChangeBookInfo_TestCase4()
    {
        // Arrange
        var category = CreateCategory();
        var book = Book.Create("タイトル", "著者", category, 3);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => book.ChangeBookInfo("タイトル", "著者", -1));
        Assert.AreEqual("stock", ex.ParamName);
    }
}