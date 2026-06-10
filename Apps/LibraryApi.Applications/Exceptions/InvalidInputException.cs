namespace LibraryApi.Applications.Exceptions;
/// <summary>
/// 入力された値が業務上の妥当性を満たさない場合にスローされる例外(プレゼンテーション層で 400 に変換される)
///
/// エラーの種類を表すコード(ErrorCode)と、具体的な内容(Message)を保持する。
/// </summary>
public class InvalidInputException : Exception
{
    /// <summary>
    /// エラーの種類を表すコード(例:CategoryNotFound)
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="errorCode">エラーの種類を表すコード</param>
    /// <param name="message">エラーの具体的な内容</param>
    public InvalidInputException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}