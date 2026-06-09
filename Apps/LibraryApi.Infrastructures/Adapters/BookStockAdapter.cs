using LibraryApi.Domains.Adapters;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Models;
namespace LibraryApi.Infrastructure.Adapters;

/// <summary>
/// ドメインエンティティ BookStock と、EF Core エンティティ BookStockEntity を相互変換する Adapter
///
/// 【集約に関する注意】
/// ドメインの BookStock は集約の内部エンティティであり、図書への参照(book_id)を持たない。
/// そのため、
/// ・Restore では BookStockEntity.BookId は使わず、UUID と蔵書数だけをドメインへ渡す
/// ・Convert では BookId を設定できない(ドメインが book_id を知らないため)
///   BookId の設定は、集約全体(Book と BookStock の関係)を扱う段で行う
/// Id・日時は Convert で設定しない(Id は DB採番、日時は DbContext が自動設定する)
/// </summary>
public class BookStockAdapter : IAdapter<BookStock, BookStockEntity>
{
    /// <summary>
    /// ドメインエンティティを EF Core エンティティに変換する(保存用)
    /// BookId は設定しない(集約を扱う段で、図書の Id を結びつける)
    /// </summary>
    public BookStockEntity Convert(BookStock source)
    {
        return new BookStockEntity
        {
            StockUuid = source.StockUuid,
            Stock = source.Stock,
            // BookId はここでは設定しない(ドメインが book_id を持たないため)
        };
    }

    /// <summary>
    /// EF Core エンティティをドメインエンティティに復元する(取得用)
    /// ドメインの復元ファクトリ BookStock.Restore に委譲する
    /// </summary>
    public BookStock Restore(BookStockEntity source)
    {
        // BookId は使わない(ドメインの BookStock は図書への参照を持たない)
        return BookStock.Restore(source.StockUuid, source.Stock);
    }
}