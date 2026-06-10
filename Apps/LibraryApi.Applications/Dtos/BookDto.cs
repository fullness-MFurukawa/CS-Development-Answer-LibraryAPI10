namespace LibraryApi.Applications.Dtos;
/// <summary>
/// 図書の情報を転送するための DTO(データ転送オブジェクト)
///
/// 検索系・登録系・変更系のユースケースで、図書(集約)の情報を表現する。
/// 参照する分類は、入れ子の CategoryDto として保持する。
/// データを運ぶことに徹し、バリデーションや業務ルールは持たない。
/// </summary>
public class BookDto
{
    /// <summary>
    /// 図書の識別Id(UUID形式)
    /// </summary>
    public string BookId { get; set; } = string.Empty;

    /// <summary>
    /// 書名
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 著者名
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 分類情報(入れ子の DTO)
    /// </summary>
    public CategoryDto Category { get; set; } = new();

    /// <summary>
    /// 蔵書数
    /// </summary>
    public int Stock { get; set; }
}