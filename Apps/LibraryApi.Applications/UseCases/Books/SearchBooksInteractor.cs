using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Services;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// ISearchBooksUseCase の実装(Interactor)
///
/// 図書サービスからキーワードに部分一致する図書(集約)の一覧を取得し、
/// Adapter で DTO に変換して返す。
/// 検索結果が0件でも、エラーではなく空のリストを返す(UC-03 BR-03)。
/// 読み取りのみのため、トランザクションは用いない。
/// </summary>
public class SearchBooksInteractor : ISearchBooksUseCase
{
    private readonly IBookService _bookService;
    private readonly IAdapter<Book, BookDto> _bookDtoAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookService">図書サービス</param>
    /// <param name="bookDtoAdapter">Book と BookDto を変換する Adapter</param>
    public SearchBooksInteractor(
        IBookService bookService,
        IAdapter<Book, BookDto> bookDtoAdapter)
    {
        _bookService = bookService;
        _bookDtoAdapter = bookDtoAdapter;
    }

    /// <summary>
    /// 書名のキーワードで図書を部分一致検索する
    /// </summary>
    /// <param name="keyword">書名に対する部分一致検索キーワード</param>
    /// <returns>該当する図書の DTO 一覧(0件の場合は空のリスト)</returns>
    public async Task<IReadOnlyList<BookDto>> ExecuteAsync(string keyword)
    {
        // キーワードに部分一致する図書(集約)の一覧を取得する
        var books = await _bookService.FindByTitleKeywordAsync(keyword);

        // 各図書を DTO に変換して返す(ドメイン → DTO)
        // 0件の場合は空のリストが返る(UC-03 BR-03:該当なしも正常系)
        return books
            .Select(book => _bookDtoAdapter.Convert(book))
            .ToList();
    }
}