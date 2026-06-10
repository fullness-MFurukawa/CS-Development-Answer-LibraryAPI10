namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// ログアウトのレスポンスを表す ViewModel(UC-02)
/// </summary>
public class LogoutResponse
{
    /// <summary>
    /// 処理結果メッセージ。JSON では "message"。
    /// </summary>
    public string Message { get; set; } = string.Empty;
}