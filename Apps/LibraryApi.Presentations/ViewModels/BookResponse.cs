namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// 図書のレスポンスを表す ViewModel
///
/// API のレスポンス JSON の形(bookId, title, author, category, stock)を表現する。
/// アプリケーション層の BookDto を、本 ViewModel に変換して返す。
/// 分類は、入れ子の CategoryResponse として保持する(分類一覧と同じ表現を再利用)。
/// </summary>
public class BookResponse
{
    /// <summary>
    /// 図書の識別Id(UUID形式)。JSON では "bookId"。
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// 書名。JSON では "title"。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 著者名。JSON では "author"。
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 分類情報(入れ子)。JSON では "category"。
    /// </summary>
    public CategoryResponse Category { get; set; } = new();

    /// <summary>
    /// 蔵書数。JSON では "stock"。
    /// </summary>
    public int Stock { get; set; }
}