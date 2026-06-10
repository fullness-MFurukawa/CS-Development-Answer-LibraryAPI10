namespace LibraryApi.Applications.Dtos;

/// <summary>
/// 図書変更の入力情報を転送するための DTO(UC-05)
///
/// 変更内容(書名・著者名・蔵書数)を運ぶ。分類は変更対象外(UC-05 BR-02)。
/// 変更対象の図書Id(bookId)はパスパラメータのため、本DTOには含めない。
/// </summary>
public class UpdateBookDto
{
    /// <summary>
    /// 変更後の書名
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 変更後の著者名
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 変更後の蔵書数(0以上)
    /// </summary>
    public int Stock { get; set; }
}