using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
namespace LibraryApi.Applications.Services.Books;
/// <summary>
/// 
/// IBookService の実装
///
/// 図書リポジトリ(IBookRepository)を利用して、図書(集約)のデータ操作を行う。
/// 入出力はドメインエンティティで扱う。
/// トランザクション境界は UseCase が管理するため、本サービスは意識しない。
/// </summary>
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookRepository">図書リポジトリ</param>
    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    /// <summary>
    /// 書名のキーワードで図書を部分一致検索する
    /// </summary>
    public async Task<IReadOnlyList<Book>> FindByTitleKeywordAsync(string keyword)
    {
        return await _bookRepository.FindByTitleKeywordAsync(keyword);
    }

    /// <summary>
    /// 識別Id(UUID)で図書を1件取得する
    /// </summary>
    public async Task<Book?> FindByUuidAsync(string bookUuid)
    {
        return await _bookRepository.FindByUuidAsync(bookUuid);
    }

    /// <summary>
    /// 図書を新規追加する
    /// </summary>
    public async Task AddAsync(Book book)
    {
        await _bookRepository.AddAsync(book);
    }

    /// <summary>
    /// 図書を更新する
    /// </summary>
    public async Task UpdateAsync(Book book)
    {
        await _bookRepository.UpdateAsync(book);
    }

    /// <summary>
    /// 図書を削除する
    /// </summary>
    public async Task DeleteAsync(Book book)
    {
        await _bookRepository.DeleteAsync(book);
    }
}