using System.ComponentModel.DataAnnotations;
namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// ログインのリクエストを表す ViewModel(UC-02)
///
/// リクエストボディ(username, password)を受け取る。
/// ここでは必須チェックのみ行い、資格情報の正否は UseCase の認証処理で判定する。
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// ユーザー名(必須)
    /// </summary>
    [Required(ErrorMessage = "ユーザー名は必須項目です")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// パスワード(必須)
    /// </summary>
    [Required(ErrorMessage = "パスワードは必須項目です")]
    public string Password { get; set; } = string.Empty;
}