using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// アプリケーション層の UpdateBookDto と、プレゼンテーション層の UpdateBookRequest(ViewModel)を
/// 変換する Adapter
///
/// 型引数は、他の ViewModel 変換 Adapter と同様「DTO が左、ViewModel が右」で統一する。
/// ・Restore : ViewModel → DTO(リクエストを受け取り、UseCase へ渡す DTO を組み立てる際に使用)
/// ・Convert : DTO → ViewModel。図書変更リクエストは入力専用のため、現状サポートしない。
/// </summary>
public class UpdateBookRequestAdapter : IAdapter<UpdateBookDto, UpdateBookRequest>
{
    /// <summary>
    /// DTO を ViewModel に変換する(DTO → ViewModel)。入力専用のため未サポート。
    /// </summary>
    /// <exception cref="NotSupportedException">本変換はサポートしていない</exception>
    public UpdateBookRequest Convert(UpdateBookDto source)
    {
        throw new NotSupportedException(
            "UpdateBookDto から UpdateBookRequest への変換はサポートしていません。");
    }

    /// <summary>
    /// ViewModel を DTO に変換する(ViewModel → DTO)
    /// </summary>
    /// <param name="source">プレゼンテーション層の UpdateBookRequest</param>
    /// <returns>アプリケーション層の UpdateBookDto</returns>
    public UpdateBookDto Restore(UpdateBookRequest source)
    {
        return new UpdateBookDto
        {
            Title = source.Title,
            Author = source.Author,
            Stock = source.Stock,
        };
    }
}