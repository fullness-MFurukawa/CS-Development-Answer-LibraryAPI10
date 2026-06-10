using System.ComponentModel.DataAnnotations;
namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// 図書変更のリクエストを表す ViewModel(UC-05)
///
/// リクエストボディ(title, author, stock)を受け取り、入力の形式的な検証を担う。
/// 分類(categoryId)は変更対象外(UC-05 BR-02)のため含めない。
/// 変更対象の図書Id(bookId)はパスパラメータのため、本 ViewModel には含めない。
/// </summary>
public class UpdateBookRequest
{
    /// <summary>
    /// 変更後の書名(必須・1~50文字)
    /// </summary>
    [Required(ErrorMessage = "書名は必須項目です")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "書名は1~50文字で入力してください")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 変更後の著者名(必須・1~30文字)
    /// </summary>
    [Required(ErrorMessage = "著者名は必須項目です")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "著者名は1~30文字で入力してください")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 変更後の蔵書数(必須・0以上の整数)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "蔵書数は0以上の整数で入力してください")]
    public int Stock { get; set; }
}