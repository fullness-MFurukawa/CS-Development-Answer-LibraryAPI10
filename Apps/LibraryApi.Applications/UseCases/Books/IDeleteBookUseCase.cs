namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// 図書を削除するユースケースのインターフェイス(UC-06)
/// </summary>
public interface IDeleteBookUseCase
{
    /// <summary>
    /// 図書を削除する(対応する蔵書も同時に削除する)
    /// </summary>
    /// <param name="bookId">削除対象の図書の識別Id(UUID形式)</param>
    Task ExecuteAsync(string bookId);
}