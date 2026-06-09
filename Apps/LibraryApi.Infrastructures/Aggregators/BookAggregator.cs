using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Aggregators;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Models;
namespace LibraryApi.Infrastructure.Aggregators;
/// <summary>
/// EF Core エンティティ BookEntity から、ドメインの集約 Book を構築する Aggregator
///
/// 【役割】
/// Book は集約ルートであり、分類(Category)を参照し、蔵書(BookStock)を内包する。
/// 本クラスは、BookEntity が内包する CategoryEntity・BookStockEntity を、
/// それぞれの Adapter に変換を委譲したうえで、ドメインの復元ファクトリ Book.Restore に渡し、
/// 集約全体を組み立てる。
/// 個々の変換ロジックは Adapter に委譲し、本クラスは「集約を組み立てる段取り」に専念する。
/// </summary>
public class BookAggregator : IAggregator<BookEntity, Book>
{
    private readonly IAdapter<Category, CategoryEntity> _categoryAdapter;
    private readonly IAdapter<BookStock, BookStockEntity> _bookStockAdapter;

    /// <summary>
    /// コンストラクタ
    /// 子エンティティの変換を担う Adapter を注入する
    /// </summary>
    /// <param name="categoryAdapter">分類の変換を担う Adapter</param>
    /// <param name="bookStockAdapter">蔵書の変換を担う Adapter</param>
    public BookAggregator(
        IAdapter<Category, CategoryEntity> categoryAdapter,
        IAdapter<BookStock, BookStockEntity> bookStockAdapter)
    {
        _categoryAdapter = categoryAdapter;
        _bookStockAdapter = bookStockAdapter;
    }

    /// <summary>
    /// BookEntity からドメインの集約 Book を構築する
    /// </summary>
    /// <param name="source">構築元の BookEntity(CategoryEntity・BookStockEntity を内包)</param>
    /// <returns>構築されたドメインの集約 Book</returns>
    public Book Aggregate(BookEntity source)
    {
        // 子エンティティを、それぞれの Adapter に委譲してドメインへ復元する
        // (source.Category / source.BookStock は、取得時に Include されている前提)
        var category = _categoryAdapter.Restore(source.Category);
        var bookStock = _bookStockAdapter.Restore(source.BookStock);

        // Book 自身のプロパティ(BookUuid・Title・Author)は、ここで直接扱う
        // 復元済みの子(category・bookStock)と合わせて、ドメインの復元ファクトリへ渡す
        // 集約としての不変条件の検証は Book.Restore が担う
        return Book.Restore(
            source.BookUuid,
            source.Title,
            source.Author,
            category,
            bookStock);
    }
}