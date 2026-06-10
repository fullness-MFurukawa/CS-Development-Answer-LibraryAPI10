using LibraryApi.Applications.Dtos;
namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// 図書を変更するユースケースのインターフェイス(UC-05)
/// </summary>
public interface IUpdateBookUseCase
{
    /// <summary>
    /// 図書の書名・著者名・蔵書数を変更する(図書と蔵書を同一トランザクションで永続化する)
    /// </summary>
    /// <param name="bookId">変更対象の図書の識別Id(UUID形式)</param>
    /// <param name="input">変更内容</param>
    /// <returns>変更後の図書の DTO</returns>
    Task<BookDto> ExecuteAsync(string bookId, UpdateBookDto input);
}