using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// アプリケーション層の RegisterBookDto と、プレゼンテーション層の RegisterBookRequest(ViewModel)を
/// 変換する Adapter
///
/// 型引数は、他の ViewModel 変換 Adapter と同様「DTO が左、ViewModel が右」で統一する。
/// ・Restore : ViewModel → DTO(リクエストを受け取り、UseCase へ渡す DTO を組み立てる際に使用)
/// ・Convert : DTO → ViewModel。図書登録リクエストは入力専用のため、現状サポートしない。
/// </summary>
public class RegisterBookRequestAdapter : IAdapter<RegisterBookDto, RegisterBookRequest>
{
    /// <summary>
    /// DTO を ViewModel に変換する(DTO → ViewModel)。入力専用のため未サポート。
    /// </summary>
    /// <exception cref="NotSupportedException">本変換はサポートしていない</exception>
    public RegisterBookRequest Convert(RegisterBookDto source)
    {
        throw new NotSupportedException(
            "RegisterBookDto から RegisterBookRequest への変換はサポートしていません。");
    }

    /// <summary>
    /// ViewModel を DTO に変換する(ViewModel → DTO)
    /// リクエストで受け取った入力を、UseCase へ渡す入力用 DTO に変換する。
    /// </summary>
    /// <param name="source">プレゼンテーション層の RegisterBookRequest</param>
    /// <returns>アプリケーション層の RegisterBookDto</returns>
    public RegisterBookDto Restore(RegisterBookRequest source)
    {
        return new RegisterBookDto
        {
            Title = source.Title,
            Author = source.Author,
            CategoryId = source.CategoryId,
            Stock = source.Stock,
        };
    }
}