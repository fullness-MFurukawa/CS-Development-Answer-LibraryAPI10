namespace LibraryApi.Applications.Dtos;
/// <summary>
/// 図書登録の入力情報を転送するための DTO(UC-04)
///
/// 図書登録リクエストの内容(書名・著者名・分類の識別Id・蔵書数)を運ぶ。
/// データを運ぶことに徹し、入力検証はプレゼンテーション層の ViewModel が担う。
/// </summary>
public class RegisterBookDto
{
    /// <summary>
    /// 書名
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 著者名
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 分類の識別Id(UUID形式)
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// 初期の蔵書数(0以上)
    /// </summary>
    public int Stock { get; set; }
}