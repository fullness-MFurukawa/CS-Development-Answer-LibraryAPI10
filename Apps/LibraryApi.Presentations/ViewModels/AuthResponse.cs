namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// ログイン・ログアウトのレスポンスを表す ViewModel
///
/// 処理結果を表すメッセージを返す。認証トークンは Cookie で扱うため、本文には含めない。
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// 処理結果メッセージ。JSON では "message"。
    /// </summary>
    public string Message { get; set; } = string.Empty;
}