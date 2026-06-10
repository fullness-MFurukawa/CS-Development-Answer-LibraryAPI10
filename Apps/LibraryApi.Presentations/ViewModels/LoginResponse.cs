namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// ログインのレスポンスを表す ViewModel(UC-02)
///
/// ログイン結果のメッセージを返す。認証トークンは HttpOnly Cookie で扱うため、本文には含めない。
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 処理結果メッセージ。JSON では "message"。
    /// </summary>
    public string Message { get; set; } = string.Empty;
}