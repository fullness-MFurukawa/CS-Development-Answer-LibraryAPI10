namespace LibraryApi.Applications.Exceptions;
/// <summary>
/// 認証に失敗した場合にスローされる例外(プレゼンテーション層で 401 Unauthorized に変換される)
///
/// ユーザーが存在しない場合と、パスワードが一致しない場合を区別せず、
/// いずれも本例外で表す(列挙攻撃を防ぐため:UC-02 BR-04)。
/// </summary>
public class AuthenticationException : Exception
{
    /// <summary>
    /// エラーの種類を表すコード(例:AuthenticationFailed)
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="errorCode">エラーの種類を表すコード</param>
    /// <param name="message">エラーの具体的な内容</param>
    public AuthenticationException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}