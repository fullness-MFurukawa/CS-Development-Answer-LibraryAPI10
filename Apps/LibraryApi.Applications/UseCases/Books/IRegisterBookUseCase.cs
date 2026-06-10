using LibraryApi.Applications.Dtos;
namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// 新しい図書を登録するユースケースのインターフェイス(UC-04)
/// </summary>
public interface IRegisterBookUseCase
{
    /// <summary>
    /// 図書を新規登録する(図書と蔵書を同一トランザクションで永続化する)
    /// </summary>
    /// <param name="input">図書登録の入力情報</param>
    /// <returns>登録された図書の DTO</returns>
    Task<BookDto> ExecuteAsync(RegisterBookDto input);
}