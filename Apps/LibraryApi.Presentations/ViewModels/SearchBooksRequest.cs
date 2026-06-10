using System.ComponentModel.DataAnnotations;
namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// 図書検索のリクエストを表す ViewModel
///
/// クエリパラメータ keyword を受け取り、入力の形式的な検証(必須・文字数)を担う。
/// 検証は ASP.NET Core のモデル検証(データ注釈)によって行われ、
/// 違反時は [ApiController] により自動的に 400 Bad Request が返る。
/// (業務的な検証や処理は、これより内側の層が担う)
/// </summary>
public class SearchBooksRequest
{
    /// <summary>
    /// 書名に対する部分一致検索キーワード(必須・1~50文字)
    /// </summary>
    [Required(ErrorMessage = "キーワードは必須項目です")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "キーワードは1~50文字で入力してください")]
    public string Keyword { get; set; } = string.Empty;
}