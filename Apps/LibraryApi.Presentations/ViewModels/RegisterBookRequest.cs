using System.ComponentModel.DataAnnotations;
namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// 図書登録のリクエストを表す ViewModel(UC-04)
///
/// リクエストボディ(title, author, categoryId, stock)を受け取り、
/// 入力の形式的な検証(必須・文字数・数値範囲)を担う。
/// 検証違反時は [ApiController] により自動的に 400 Bad Request が返る。
/// </summary>
public class RegisterBookRequest
{
    /// <summary>
    /// 書名(必須・1~50文字)
    /// </summary>
    [Required(ErrorMessage = "書名は必須項目です")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "書名は1~50文字で入力してください")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 著者名(必須・1~30文字)
    /// </summary>
    [Required(ErrorMessage = "著者名は必須項目です")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "著者名は1~30文字で入力してください")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 分類の識別Id(必須・UUID形式)
    /// </summary>
    [Required(ErrorMessage = "分類Idは必須項目です")]
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// 蔵書数(必須・0以上の整数)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "蔵書数は0以上の整数で入力してください")]
    public int Stock { get; set; }
}