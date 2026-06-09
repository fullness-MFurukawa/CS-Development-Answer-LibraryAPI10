using LibraryApi.Domains.Aggregators;
using LibraryApi.Infrastructure.Contexts;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Infrastructure.Exceptions;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Microsoft.EntityFrameworkCore;
namespace LibraryApi.Infrastructure.Repositories;
/// <summary>
/// IBookRepository の実装(図書の永続化を担う)
///
/// Book は集約ルートであり、分類(Category)を参照し蔵書(BookStock)を内包する。
/// 取得時は Include で分類・蔵書を読み込み、BookAggregator で Book 集約に構築する。
/// 保存時は図書と蔵書を集約として一体で扱い、データ操作を完結させる(SaveChangesAsync まで呼ぶ)。
/// </summary>
public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;
    private readonly IAggregator<BookEntity, Book> _bookAggregator;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">EF Core データベースコンテキスト</param>
    /// <param name="bookAggregator">BookEntity から Book 集約を構築する Aggregator</param>
    public BookRepository(
        AppDbContext context,
        IAggregator<BookEntity, Book> bookAggregator)
    {
        _context = context;
        _bookAggregator = bookAggregator;
    }

    /// <summary>
    /// 書名のキーワードで図書を部分一致検索する(UC-03)
    /// </summary>
    /// <param name="keyword">書名に対する部分一致検索キーワード</param>
    /// <returns>該当する図書の一覧(0件の場合は空のリスト)</returns>
    public async Task<IReadOnlyList<Book>> FindByTitleKeywordAsync(string keyword)
    {
        // 集約の構築に必要な分類・蔵書を Include で読み込む
        // 書名の部分一致は EF.Functions.Like を用いる(SQL の LIKE に変換される)
        var entities = await _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.BookStock)
            .Where(b => EF.Functions.Like(b.Title, $"%{keyword}%"))
            .ToListAsync();

        // 各 BookEntity を Aggregator で Book 集約に構築する
        return entities
            .Select(entity => _bookAggregator.Aggregate(entity))
            .ToList();
    }

    /// <summary>
    /// 識別Id(UUID)で図書を1件取得する
    /// </summary>
    /// <param name="bookUuid">図書の識別Id(UUID形式)</param>
    /// <returns>該当する図書。存在しない場合はnull</returns>
    public async Task<Book?> FindByUuidAsync(string bookUuid)
    {
        // 集約の構築に必要な分類・蔵書を Include で読み込む
        var entity = await _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.BookStock)
            .FirstOrDefaultAsync(b => b.BookUuid == bookUuid);

        // 見つからなければ null、見つかれば Aggregator で Book 集約に構築して返す
        return entity is null
            ? null
            : _bookAggregator.Aggregate(entity);
    }

   
    /// <summary>
    /// 図書を新規追加する(図書と蔵書を集約として一体で保存する)
    /// データ操作を完結させるため、SaveChangesAsync までを本メソッドで行う
    /// (日時は AppDbContext の SaveChangesAsync が自動設定する)
    /// </summary>
    /// <param name="book">追加する図書(分類を参照し、蔵書を内包する)</param>
    /// <exception cref="DomainException">参照する分類が存在しない場合</exception>
    public async Task AddAsync(Book book)
    {
        // 参照する分類の DB上の id を、category_uuid から解決する
        // (ドメインの Book は category_uuid は知るが、DB採番の category.id は知らないため)
        var categoryId = await ResolveCategoryIdAsync(book.Category.CategoryUuid);

        // ドメインの集約 Book を、EF Core エンティティ(図書+蔵書)に分解して組み立てる
        var bookEntity = new BookEntity
        {
            BookUuid = book.BookUuid,
            Title = book.Title,
            Author = book.Author,
            CategoryId = categoryId,
            // 蔵書はナビゲーションプロパティに紐づける
            // EF Core が図書の採番後、その id を蔵書の book_id に自動設定してまとめて保存する
            BookStock = new BookStockEntity
            {
                StockUuid = book.BookStock.StockUuid,
                Stock = book.BookStock.Stock,
            },
        };

        // 図書(と内包する蔵書)をまとめて追加し、永続化する
        await _context.Books.AddAsync(bookEntity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 図書を更新する(書名・著者名・蔵書数を変更する。分類は変更対象外:UC-05 BR-02)
    /// 既存レコードを book_uuid で特定し、値を上書きして保存する
    /// </summary>
    /// <param name="book">更新内容を持つ図書</param>
    /// <exception cref="DomainException">対象の図書が存在しない場合</exception>
    public async Task UpdateAsync(Book book)
    {
        // 更新対象の既存レコードを、蔵書とともに取得する
        // (更新するため、ここでは AsNoTracking を付けない:追跡された状態で値を変更する)
        var bookEntity = await _context.Books
            .Include(b => b.BookStock)
            .FirstOrDefaultAsync(b => b.BookUuid == book.BookUuid);

        if (bookEntity is null)
        {
             throw new EntityNotFoundException("対象の図書が存在しません。");
        }

        // 変更可能な項目(書名・著者名)を上書きする
        bookEntity.Title = book.Title;
        bookEntity.Author = book.Author;

        // 蔵書数を上書きする(内包する蔵書レコードに反映)
        bookEntity.BookStock.Stock = book.BookStock.Stock;

        // 追跡中のエンティティへの変更を永続化する
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 図書を削除する(図書削除時、内包する蔵書も同時に削除する:UC-06 BR-02)
    /// 蔵書の削除は、マッピングの OnDelete(Cascade) により自動的に行われる
    /// </summary>
    /// <param name="book">削除する図書</param>
    /// <exception cref="DomainException">対象の図書が存在しない場合</exception>
    public async Task DeleteAsync(Book book)
    {
        // 削除対象の既存レコードを取得する
        var bookEntity = await _context.Books
        .Include(b => b.BookStock)
        .FirstOrDefaultAsync(b => b.BookUuid == book.BookUuid);
        
        if (bookEntity is null)
        {
            throw new EntityNotFoundException("対象の図書が存在しません。");
        }

        // 図書を削除する。内包する蔵書は OnDelete(Cascade) により自動的に削除される
        _context.Books.Remove(bookEntity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// category_uuid から、DB上の分類 id を解決する
    /// 図書保存時、ドメインが知らない category.id を補うために用いる
    /// </summary>
    /// <param name="categoryUuid">分類の識別Id(UUID形式)</param>
    /// <returns>分類の DB上の id</returns>
    /// <exception cref="DomainException">該当する分類が存在しない場合</exception>
    private async Task<int> ResolveCategoryIdAsync(string categoryUuid)
    {
        // 指定された分類が実在するかを確認しつつ、その id を取得する(UC-04 BR-03:参照整合性)
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryUuid == categoryUuid);

        if (category is null)
        {
            throw new EntityNotFoundException("指定された分類が存在しません。");
        }

        return category.Id;
    }
}