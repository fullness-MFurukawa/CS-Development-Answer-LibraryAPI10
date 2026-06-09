using LibraryApi.Infrastructure.Exceptions;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Microsoft.Extensions.DependencyInjection;
namespace LibraryApi.Infrastructures.Tests.Repositories;
/// <summary>
/// BookRepository の統合テストドライバ(検索系)
/// 実際の library_db に接続し、Include による集約の構築を含めて取得結果を検証する
/// (RepositoryTestBase により、各テストはトランザクション内で実行されロールバックされる)
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class BookRepositoryTests : RepositoryTestBase
{
    // ───────────────────────────────────────────
    // FindByTitleKeywordAsync(書名の部分一致検索)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "書名のキーワードで図書を部分一致検索する")]
    public async Task FindByTitleKeywordAsync_TestCase1()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IBookRepository>();

        // Act
        // 手順書の図書に含まれる書名の一部で検索する(例:「の」を含む書名)
        var books = await repository.FindByTitleKeywordAsync("の");

        // Assert
        // 1件以上ヒットすること
        Assert.IsNotEmpty(books);
        // ヒットした図書の書名に、キーワードが含まれること
        Assert.IsTrue(books.All(b => b.Title.Contains("の")));
    }

    [TestMethod(DisplayName = "検索結果の図書は分類と蔵書を含む集約として構築される")]
    public async Task FindByTitleKeywordAsync_TestCase2()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IBookRepository>();

        // Act
        var books = await repository.FindByTitleKeywordAsync("の");

        // Assert
        // Include により、各図書が分類(Category)と蔵書(BookStock)を伴って構築されること
        var book = books.First();
        Assert.IsNotNull(book.Category);
        Assert.IsFalse(string.IsNullOrEmpty(book.Category.CategoryUuid));
        Assert.IsNotNull(book.BookStock);
        Assert.IsFalse(string.IsNullOrEmpty(book.BookStock.StockUuid));
    }

    [TestMethod(DisplayName = "ヒットしないキーワードでは空のリストが返る")]
    public async Task FindByTitleKeywordAsync_TestCase3()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IBookRepository>();

        // Act
        var books = await repository.FindByTitleKeywordAsync("絶対に存在しない書名XYZ123");

        // Assert
        Assert.IsEmpty(books);
    }

    // ───────────────────────────────────────────
    // FindByUuidAsync(識別Idで1件取得)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "識別Idで図書を1件取得する")]
    public async Task FindByUuidAsync_TestCase1()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IBookRepository>();
        // 検索で1件取り出し、その UUID を検索キーに使う(具体値をハードコードしない)
        var some = await repository.FindByTitleKeywordAsync("の");
        var target = some.First();

        // Act
        var book = await repository.FindByUuidAsync(target.BookUuid);

        // Assert
        Assert.IsNotNull(book);
        Assert.AreEqual(target.BookUuid, book.BookUuid);
        Assert.AreEqual(target.Title, book.Title);
        // 集約として、分類と蔵書も構築されていること
        Assert.IsNotNull(book.Category);
        Assert.IsNotNull(book.BookStock);
    }

    [TestMethod(DisplayName = "存在しない識別Idではnullが返る")]
    public async Task FindByUuidAsync_TestCase2()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IBookRepository>();

        // Act
        var book = await repository.FindByUuidAsync("non-existent-uuid");

        // Assert
        Assert.IsNull(book);
    }

    // ───────────────────────────────────────────
    // AddAsync(図書と蔵書をまとめて新規追加)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "図書と蔵書をまとめて新規追加し集約として取得できる")]
    public async Task AddAsync_TestCase1()
    {
        // Arrange
        var bookRepository = ScopedServices.GetRequiredService<IBookRepository>();
        var categoryRepository = ScopedServices.GetRequiredService<ICategoryRepository>();
        // 実在する分類を1件取得し、それを参照する図書を作る
        var category = (await categoryRepository.FindAllAsync()).First();
        var book = Book.Create("テスト駆動開発入門", "テスト太郎", category, 5);

        // Act
        await bookRepository.AddAsync(book);

        // Assert
        // 同一トランザクション内で取得し、集約が正しく構築されることを確認
        var added = await bookRepository.FindByUuidAsync(book.BookUuid);
        Assert.IsNotNull(added);
        Assert.AreEqual("テスト駆動開発入門", added.Title);
        Assert.AreEqual("テスト太郎", added.Author);
        // 内包する蔵書がまとめて保存されていること(EF Core が book_id を自動設定)
        Assert.AreEqual(5, added.BookStock.Stock);
        // 参照する分類が、CategoryId 解決を経て正しく紐づくこと
        Assert.AreEqual(category.CategoryUuid, added.Category.CategoryUuid);
    }

    [TestMethod(DisplayName = "存在しない分類を参照して追加するとEntityNotFoundExceptionをスローする")]
    public async Task AddAsync_TestCase2()
    {
        // Arrange
        var bookRepository = ScopedServices.GetRequiredService<IBookRepository>();
        // 実在しない分類(UUIDを新規採番した、DBに無い分類)を参照する図書を作る
        var nonExistentCategory = Category.Create("存在しない分類");
        var book = Book.Create("書名", "著者", nonExistentCategory, 1);

        // Act / Assert
        // ResolveCategoryIdAsync が、参照先の分類を見つけられず例外を投げる(UC-04 BR-03)
        await Assert.ThrowsExactlyAsync<EntityNotFoundException>(
            () => bookRepository.AddAsync(book));
    }

    // ───────────────────────────────────────────
    // UpdateAsync(書名・著者名・蔵書数の更新)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "図書の書名・著者名・蔵書数を更新する")]
    public async Task UpdateAsync_TestCase1()
    {
        // Arrange
        var bookRepository = ScopedServices.GetRequiredService<IBookRepository>();
        // 既存の図書を1件取得する
        var existing = (await bookRepository.FindByTitleKeywordAsync("の")).First();
        // 取得した図書の情報を変更する(集約ルート経由で書名・著者名・蔵書数を変更)
        existing.ChangeBookInfo("改訂版タイトル", "改訂版著者", 99);

        // Act
        await bookRepository.UpdateAsync(existing);

        // Assert
        // 更新後に取得し、変更が反映されていることを確認
        var updated = await bookRepository.FindByUuidAsync(existing.BookUuid);
        Assert.IsNotNull(updated);
        Assert.AreEqual("改訂版タイトル", updated.Title);
        Assert.AreEqual("改訂版著者", updated.Author);
        // 蔵書数の変更が、内包する蔵書レコードに反映されていること
        Assert.AreEqual(99, updated.BookStock.Stock);
    }

    [TestMethod(DisplayName = "存在しない図書を更新するとEntityNotFoundExceptionをスローする")]
    public async Task UpdateAsync_TestCase2()
    {
        // Arrange
        var bookRepository = ScopedServices.GetRequiredService<IBookRepository>();
        var categoryRepository = ScopedServices.GetRequiredService<ICategoryRepository>();
        var category = (await categoryRepository.FindAllAsync()).First();
        // DB に存在しない図書(UUIDを新規採番した、保存していない図書)
        var notStored = Book.Create("未保存の図書", "著者", category, 1);

        // Act / Assert
        await Assert.ThrowsExactlyAsync<EntityNotFoundException>(
            () => bookRepository.UpdateAsync(notStored));
    }

    // ───────────────────────────────────────────
    // DeleteAsync(図書削除、蔵書もカスケード削除)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "図書を削除すると取得できなくなる")]
    public async Task DeleteAsync_TestCase1()
    {
        // Arrange
        var bookRepository = ScopedServices.GetRequiredService<IBookRepository>();
        var categoryRepository = ScopedServices.GetRequiredService<ICategoryRepository>();
        // 削除対象として、新規に図書を追加しておく(既存データを消さないため)
        var category = (await categoryRepository.FindAllAsync()).First();
        var book = Book.Create("削除対象の図書", "著者", category, 3);
        await bookRepository.AddAsync(book);

        // Act
        await bookRepository.DeleteAsync(book);

        // Assert
        // 削除後は取得できない(蔵書も OnDelete(Cascade) により削除される)
        var deleted = await bookRepository.FindByUuidAsync(book.BookUuid);
        Assert.IsNull(deleted);
    }

    [TestMethod(DisplayName = "存在しない図書を削除するとEntityNotFoundExceptionをスローする")]
    public async Task DeleteAsync_TestCase2()
    {
        // Arrange
        var bookRepository = ScopedServices.GetRequiredService<IBookRepository>();
        var categoryRepository = ScopedServices.GetRequiredService<ICategoryRepository>();
        var category = (await categoryRepository.FindAllAsync()).First();
        var notStored = Book.Create("未保存の図書", "著者", category, 1);

        // Act / Assert
        await Assert.ThrowsExactlyAsync<EntityNotFoundException>(
            () => bookRepository.DeleteAsync(notStored));
    }
}