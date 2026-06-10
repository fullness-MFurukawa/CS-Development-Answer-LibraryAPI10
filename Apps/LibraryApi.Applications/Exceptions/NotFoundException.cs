namespace LibraryApi.Applications.Exceptions;
/// <summary>
/// 要求されたリソースが見つからない場合にスローされる例外(プレゼンテーション層で404に変換される)
///
/// エラーの種類を表すコード(ErrorCode)と、具体的な内容(Message)を保持する。
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// エラーの種類を表すコード(例:BookNotFound)
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="errorCode">エラーの種類を表すコード</param>
    /// <param name="message">エラーの具体的な内容</param>
    public NotFoundException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}