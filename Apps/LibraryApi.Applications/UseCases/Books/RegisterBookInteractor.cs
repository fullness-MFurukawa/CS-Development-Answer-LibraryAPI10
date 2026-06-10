using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.Services.Categories;
using LibraryApi.Applications.UseCases.UnitOfWorks; 
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// IRegisterBookUseCase の実装(Interactor)
///
/// 指定された分類の実在を確認し、図書(集約)を構築して永続化する。
/// 図書と蔵書は同一トランザクションで永続化する(UC-04 BR-01)。
/// 指定された分類が存在しない場合は InvalidInputException をスローする(UC-04 BR-03、400相当)。
/// </summary>
public class RegisterBookInteractor : IRegisterBookUseCase
{
    private readonly ICategoryService _categoryService;
    private readonly IBookService _bookService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdapter<Book, BookDto> _bookDtoAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public RegisterBookInteractor(
        ICategoryService categoryService,
        IBookService bookService,
        IUnitOfWork unitOfWork,
        IAdapter<Book, BookDto> bookDtoAdapter)
    {
        _categoryService = categoryService;
        _bookService = bookService;
        _unitOfWork = unitOfWork;
        _bookDtoAdapter = bookDtoAdapter;
    }

    /// <summary>
    /// 図書を新規登録する
    /// </summary>
    /// <param name="input">図書登録の入力情報</param>
    /// <returns>登録された図書のDTO</returns>
    /// <exception cref="InvalidInputException">指定された分類が存在しない場合</exception>
    public async Task<BookDto> ExecuteAsync(RegisterBookDto input)
    {
        // 指定された分類の実在を確認する(トランザクション開始前の前提条件チェック)
        var category = await _categoryService.FindByUuidAsync(input.CategoryId);
        if (category is null)
        {
            // 形式は妥当だが、指す対象が存在しない → 入力の不正として扱う(400相当)
            throw new InvalidInputException("CategoryNotFound", "指定された分類が存在しません。");
        }

        // 実在する分類を用いて、図書(集約)を構築する
        // (書名・著者名・蔵書数の妥当性は Book.Create内のドメイン検証が担う)
        var book = Book.Create(input.Title, input.Author, category, input.Stock);
        // 図書と蔵書を同一トランザクションで永続化する(UC-04 BR-01)
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _bookService.AddAsync(book);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            // 永続化に失敗した場合はロールバックし、例外は上位へ伝える
            await _unitOfWork.RollbackAsync();
            throw;
        }

        // 登録した図書を DTO に変換して返す(ドメイン → DTO)
        return _bookDtoAdapter.Convert(book);
    }
}