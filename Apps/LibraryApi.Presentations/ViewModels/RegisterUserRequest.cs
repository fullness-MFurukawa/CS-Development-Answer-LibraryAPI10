using System.ComponentModel.DataAnnotations;
namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// ユーザー登録のリクエストを表す ViewModel(UC-01)
///
/// リクエストボディ(username, password)を受け取り、入力の形式的な検証を担う。
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// ユーザー名(必須・1~30文字)
    /// </summary>
    [StringLength(30, MinimumLength = 1, ErrorMessage = "ユーザー名は1~30文字で入力してください")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// パスワード(必須)
    /// </summary>
    [Required(ErrorMessage = "パスワードは必須項目です")]
    public string Password { get; set; } = string.Empty;
}