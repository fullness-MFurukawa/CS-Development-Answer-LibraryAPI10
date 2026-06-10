using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.UseCases.Books;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LibraryApi.Presentations.Controllers;
/// <summary>
/// 図書に関する API を提供する
/// </summary>
[ApiController]
[Route("library/api/books")]
[Tags("図書の取得・登録・更新・削除")]
[Authorize] 
public class BooksController : ControllerBase
{
    private readonly ISearchBooksUseCase _searchBooksUseCase;
    private readonly IFindBookUseCase _findBookUseCase;
    private readonly IRegisterBookUseCase _registerBookUseCase;
    private readonly IUpdateBookUseCase _updateBookUseCase;
    private readonly IDeleteBookUseCase _deleteBookUseCase;
    private readonly IAdapter<BookDto, BookResponse> _bookResponseAdapter;
    private readonly IAdapter<RegisterBookDto, RegisterBookRequest> _registerBookRequestAdapter;
    private readonly IAdapter<UpdateBookDto, UpdateBookRequest> _updateBookRequestAdapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="searchBooksUseCase">図書検索ユースケース</param>
    /// <param name="findBookUseCase">図書取得ユースケース</param>
    /// <param name="registerBookUseCase">図書登録ユースケース</param>
    /// <param name="updateBookUseCase">図書更新ユースケース</param>
    /// <param name="deleteBookUseCase">図書削除ユースケース</param>
    /// <param name="bookResponseAdapter">BookDtoをBookResponseに変換するAdapter</param>
    /// <param name="registerBookRequestAdapter">RegisterBookRequestをRegisterBookDtoに変換するAdapter</param>
    /// <param name="updateBookRequestAdapter">UpdateBookRequestをUpdateBookDtoに変換するAdapter</param>
    public BooksController(
        ISearchBooksUseCase searchBooksUseCase,
        IFindBookUseCase findBookUseCase,
        IRegisterBookUseCase registerBookUseCase,
        IUpdateBookUseCase updateBookUseCase,
        IDeleteBookUseCase deleteBookUseCase,
        IAdapter<BookDto, BookResponse> bookResponseAdapter,
        IAdapter<RegisterBookDto, RegisterBookRequest> registerBookRequestAdapter,
        IAdapter<UpdateBookDto, UpdateBookRequest> updateBookRequestAdapter)
    {
        _searchBooksUseCase = searchBooksUseCase;
        _findBookUseCase = findBookUseCase;
        _registerBookUseCase = registerBookUseCase;
        _updateBookUseCase = updateBookUseCase;
        _deleteBookUseCase = deleteBookUseCase;
        _bookResponseAdapter = bookResponseAdapter;
        _registerBookRequestAdapter = registerBookRequestAdapter;
        _updateBookRequestAdapter = updateBookRequestAdapter;
    }

    /// <summary>
    /// 書名のキーワードで図書を検索する
    /// GET /library/api/books?keyword={keyword}
    /// </summary>
    /// <param name="request">検索リクエスト(キーワード)</param>
    /// <returns>該当する図書の一覧(200 OK。0件の場合は空配列)</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookResponse>>> SearchBooks(
        [FromQuery] SearchBooksRequest request)
    {
        // キーワードで検索する(入力検証は ViewModel の属性により実施済み)
        var dtos = await _searchBooksUseCase.ExecuteAsync(request.Keyword);

        // DTO を ViewModel に変換する(DTO → ViewModel)
        var response = dtos
            .Select(dto => _bookResponseAdapter.Convert(dto))
            .ToList();

        // 200 OKで図書一覧を返す(0件でも空配列を返す:UC-03 BR-03)
        return Ok(response);
    }

    /// <summary>
    /// 識別Idで図書の詳細を取得する
    /// GET /library/api/books/{bookId}
    /// </summary>
    /// <param name="bookId">図書の識別Id(UUID形式)</param>
    /// <returns>図書の詳細(200 OK)。存在しない場合は 404。</returns>
    [HttpGet("{bookId}")]
    public async Task<ActionResult<BookResponse>> GetBook(string bookId)
    {
        // 図書を取得する(存在しない場合、UseCaseがNotFoundExceptionスロー、
        //  ミドルウェアが404に変換する)
        var dto = await _findBookUseCase.ExecuteAsync(bookId);

        var response = _bookResponseAdapter.Convert(dto);

        return Ok(response);
    }

    /// <summary>
    /// 新しい図書を登録する
    /// POST /library/api/books
    /// </summary>
    /// <param name="request">図書登録リクエスト</param>
    /// <returns>登録された図書(201 Created)。分類が存在しない場合は 400。</returns>
    [HttpPost]
    public async Task<ActionResult<BookResponse>> RegisterBook(
        [FromBody] RegisterBookRequest request)
    {
        // リクエスト(ViewModel)を入力用 DTO に変換する(ViewModel → DTO)
        var input = _registerBookRequestAdapter.Restore(request);

        // 図書を登録する(分類が存在しない場合、UseCase が InvalidInputException を投げ、
        //  ミドルウェアが 400 に変換する)
        var dto = await _registerBookUseCase.ExecuteAsync(input);

        // 登録結果(DTO)をレスポンス(ViewModel)に変換する(DTO → ViewModel)
        var response = _bookResponseAdapter.Convert(dto);

        // 201 Created で返す。Location ヘッダに、作成されたリソースの取得 URL を付与する。
        return CreatedAtAction(
            nameof(GetBook),
            new { bookId = response.BookId },
            response);
    }

    /// <summary>
    /// 図書を変更する
    /// PUT /library/api/books/{bookId}
    /// </summary>
    /// <param name="bookId">変更対象の図書の識別Id(UUID形式)</param>
    /// <param name="request">変更内容</param>
    /// <returns>変更後の図書(200 OK)。存在しない場合は 404。</returns>
    [HttpPut("{bookId}")]
    public async Task<ActionResult<BookResponse>> UpdateBook(
        string bookId,
        [FromBody] UpdateBookRequest request)
    {
        // リクエスト(ViewModel)を入力用 DTO に変換する(ViewModel → DTO)
        var input = _updateBookRequestAdapter.Restore(request);

        // 図書を変更する(存在しない場合、UseCase が NotFoundException を投げ、
        //  ミドルウェアが404に変換する)
        var dto = await _updateBookUseCase.ExecuteAsync(bookId, input);

        // 変更結果(DTO)をレスポンス(ViewModel)に変換する(DTO → ViewModel)
        var response = _bookResponseAdapter.Convert(dto);

        // 200OKで返す(変更は作成ではないため201ではなく200)
        return Ok(response);
    }

    /// <summary>
    /// 図書を削除する
    /// DELETE /library/api/books/{bookId}
    /// </summary>
    /// <param name="bookId">削除対象の図書の識別Id(UUID形式)</param>
    /// <returns>204 No Content。存在しない場合は 404。</returns>
    [HttpDelete("{bookId}")]
    public async Task<IActionResult> DeleteBook(string bookId)
    {
        // 図書を削除する(存在しない場合、UseCase が NotFoundException を投げ、
        //  ミドルウェアが 404 に変換する)
        await _deleteBookUseCase.ExecuteAsync(bookId);

        // 204 No Content を返す(レスポンスボディなし)
        return NoContent();
    }
}