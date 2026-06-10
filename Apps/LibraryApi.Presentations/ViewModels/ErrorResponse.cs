namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// エラーレスポンスを表す ViewModel
///
/// API のエラー形式 {"error": "...", "message": "..."} を表現する。
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// エラーの種類を表すコード。JSON では "error"。
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// エラーの具体的な内容。JSON では "message"。
    /// </summary>
    public string Message { get; set; } = string.Empty;
}