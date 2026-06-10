using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.UseCases.UnitOfWorks; 
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;

namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// IUpdateBookUseCase の実装(Interactor)
///
/// 変更対象の図書(集約)を取得し、書名・著者名・蔵書数を変更して永続化する。
/// 図書と蔵書は同一トランザクションで永続化する(UC-05 BR-01)。
/// 変更対象の図書が存在しない場合は NotFoundException をスローする(UC-05 E2、404相当)。
/// </summary>
public class UpdateBookInteractor : IUpdateBookUseCase
{
    private readonly IBookService _bookService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdapter<Book, BookDto> _bookDtoAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public UpdateBookInteractor(
        IBookService bookService,
        IUnitOfWork unitOfWork,
        IAdapter<Book, BookDto> bookDtoAdapter)
    {
        _bookService = bookService;
        _unitOfWork = unitOfWork;
        _bookDtoAdapter = bookDtoAdapter;
    }

    /// <summary>
    /// 図書を変更する
    /// </summary>
    /// <param name="bookId">変更対象の図書の識別Id(UUID形式)</param>
    /// <param name="input">変更内容</param>
    /// <returns>変更後の図書のDTO</returns>
    /// <exception cref="NotFoundException">変更対象の図書が存在しない場合</exception>
    public async Task<BookDto> ExecuteAsync(string bookId, UpdateBookDto input)
    {
        // 変更対象の図書(集約)を取得する
        var book = await _bookService.FindByUuidAsync(bookId);
        if (book is null)
        {
            // 変更対象のリソースが存在しない → 404 相当
            throw new NotFoundException("BookNotFound", "指定された図書が存在しません。");
        }

        // 書名・著者名・蔵書数を変更する
        // (各値の妥当性はChangeBookInfo内のドメイン検証が担う。分類は変更対象外)
        book.ChangeBookInfo(input.Title, input.Author, input.Stock);

        // 変更内容を同一トランザクションで永続化する(UC-05 BR-01)
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _bookService.UpdateAsync(book);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        // 変更後の図書をDTOに変換して返す(ドメイン → DTO)
        return _bookDtoAdapter.Convert(book);
    }
}