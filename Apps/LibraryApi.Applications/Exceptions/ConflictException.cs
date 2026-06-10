namespace LibraryApi.Applications.Exceptions;
/// <summary>
/// リソースの競合(既に存在するなど)により処理できない場合にスローされる例外
/// (プレゼンテーション層で 409 Conflict に変換される)
///
/// エラーの種類を表すコード(ErrorCode)と、具体的な内容(Message)を保持する。
/// 例:ユーザー名が既に使用されている(DuplicateUsername)。
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// エラーの種類を表すコード(例:DuplicateUsername)
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="errorCode">エラーの種類を表すコード</param>
    /// <param name="message">エラーの具体的な内容</param>
    public ConflictException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}