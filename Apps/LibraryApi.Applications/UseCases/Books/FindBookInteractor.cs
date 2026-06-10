using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// IFindBookUseCase の実装(Interactor)
///
/// 識別Id(UUID)で図書(集約)を取得し、AdapterでDTOに変換して返す。
/// 指定された図書が存在しない場合は、NotFoundException をスローする
/// (プレゼンテーション層で 404 Not Found に変換される)。
/// 読み取りのみのため、トランザクションは用いない。
/// </summary>
public class FindBookInteractor : IFindBookUseCase
{
    private readonly IBookService _bookService;
    private readonly IAdapter<Book, BookDto> _bookDtoAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookService">図書サービス</param>
    /// <param name="bookDtoAdapter">Book と BookDto を変換する Adapter</param>
    public FindBookInteractor(
        IBookService bookService,
        IAdapter<Book, BookDto> bookDtoAdapter)
    {
        _bookService = bookService;
        _bookDtoAdapter = bookDtoAdapter;
    }

    /// <summary>
    /// 識別Id(UUID)で図書の詳細を取得する
    /// </summary>
    /// <param name="bookId">図書の識別Id(UUID形式)</param>
    /// <returns>図書の DTO</returns>
    /// <exception cref="NotFoundException">指定された図書が存在しない場合</exception>
    public async Task<BookDto> ExecuteAsync(string bookId)
    {
        // 識別Idで図書(集約)を取得する
        var book = await _bookService.FindByUuidAsync(bookId);

        // 見つからない場合は、ユースケースの結果としてのリソース不在を表す例外を投げる
        // (プレゼンテーション層がこれを 404 Not Found に変換する)
        if (book is null)
        {
            throw new NotFoundException("BookNotFound", "指定された図書が存在しません。");
        }

        // 取得した図書を DTO に変換して返す(ドメイン → DTO)
        return _bookDtoAdapter.Convert(book);
    }
}