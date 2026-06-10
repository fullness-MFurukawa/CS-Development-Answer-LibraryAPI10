using LibraryApi.Applications.Dtos;
namespace LibraryApi.Applications.UseCases.Books;
/// <summary>
/// 図書をキーワードで検索するユースケースのインターフェイス(UC-03)
/// </summary>
public interface ISearchBooksUseCase
{
    /// <summary>
    /// 書名のキーワードで図書を部分一致検索する
    /// </summary>
    /// <param name="keyword">書名に対する部分一致検索キーワード(入力検証は上位層で実施済みの前提)</param>
    /// <returns>該当する図書の DTO 一覧(0件の場合は空のリスト)</returns>
    Task<IReadOnlyList<BookDto>> ExecuteAsync(string keyword);
}