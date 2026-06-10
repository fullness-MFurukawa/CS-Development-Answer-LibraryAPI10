using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.UseCases.UnitOfWorks; 
namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// IDeleteBookUseCase の実装(Interactor)
///
/// 削除対象の図書(集約)を取得し、図書と蔵書を同一トランザクションで削除する
/// (UC-06 BR-01。蔵書の削除はカスケードにより行われる:UC-06 BR-02)。
/// 削除対象の図書が存在しない場合は NotFoundException をスローする(UC-06 E2、404相当)。
/// </summary>
public class DeleteBookInteractor : IDeleteBookUseCase
{
    private readonly IBookService _bookService;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public DeleteBookInteractor(
        IBookService bookService,
        IUnitOfWork unitOfWork)
    {
        _bookService = bookService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 図書を削除する
    /// </summary>
    /// <param name="bookId">削除対象の図書の識別Id(UUID形式)</param>
    /// <exception cref="NotFoundException">削除対象の図書が存在しない場合</exception>
    public async Task ExecuteAsync(string bookId)
    {
        // 削除対象の図書(集約)を取得する
        var book = await _bookService.FindByUuidAsync(bookId);
        if (book is null)
        {
            // 削除対象のリソースが存在しない → 404 相当
            throw new NotFoundException("BookNotFound", "指定された図書が存在しません。");
        }

        // 図書と蔵書を同一トランザクションで削除する(UC-06 BR-01)
        // (蔵書はカスケード削除される:UC-06 BR-02)
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _bookService.DeleteAsync(book);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}