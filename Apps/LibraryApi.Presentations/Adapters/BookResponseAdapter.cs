using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// アプリケーション層の BookDto と、プレゼンテーション層の BookResponse(ViewModel)を変換する Adapter
///
/// ・Convert : DTO → ViewModel(レスポンスを組み立てる際に使用)
///   入れ子の分類(CategoryDto → CategoryResponse)の変換は、CategoryResponseAdapter に委譲する。
/// ・Restore : ViewModel → DTO。図書のレスポンスは出力専用のため、現状サポートしない。
/// </summary>
public class BookResponseAdapter : IAdapter<BookDto, BookResponse>
{
    private readonly IAdapter<CategoryDto, CategoryResponse> _categoryResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categoryResponseAdapter">分類の DTO → ViewModel 変換を担う Adapter</param>
    public BookResponseAdapter(IAdapter<CategoryDto, CategoryResponse> categoryResponseAdapter)
    {
        _categoryResponseAdapter = categoryResponseAdapter;
    }

    /// <summary>
    /// DTO を ViewModel に変換する(DTO → ViewModel)
    /// </summary>
    /// <param name="source">アプリケーション層の BookDto</param>
    /// <returns>プレゼンテーション層の BookResponse</returns>
    public BookResponse Convert(BookDto source)
    {
        return new BookResponse
        {
            BookId = source.BookId,
            Title = source.Title,
            Author = source.Author,
            // 入れ子の分類は、CategoryResponseAdapter に変換を委譲する
            Category = _categoryResponseAdapter.Convert(source.Category),
            Stock = source.Stock,
        };
    }

    /// <summary>
    /// ViewModel を DTO に変換する(ViewModel → DTO)。図書のレスポンスは出力専用のため未サポート。
    /// </summary>
    /// <exception cref="NotSupportedException">本変換はサポートしていない</exception>
    public BookDto Restore(BookResponse source)
    {
        throw new NotSupportedException(
            "BookResponse から BookDto への変換はサポートしていません。");
    }
}