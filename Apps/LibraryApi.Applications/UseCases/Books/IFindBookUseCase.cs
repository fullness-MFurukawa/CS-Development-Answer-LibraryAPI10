using LibraryApi.Applications.Dtos;
namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// 図書の詳細を取得するユースケースのインターフェイス(補助API:UC-05 図書変更の前提)
/// </summary>
public interface IFindBookUseCase
{
    /// <summary>
    /// 識別Id(UUID)で図書の詳細を取得する
    /// </summary>
    /// <param name="bookId">図書の識別Id(UUID形式)</param>
    /// <returns>図書の DTO</returns>
    Task<BookDto> ExecuteAsync(string bookId);
}