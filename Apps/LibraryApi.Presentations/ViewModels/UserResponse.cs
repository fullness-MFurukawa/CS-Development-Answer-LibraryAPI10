namespace LibraryApi.Presentations.ViewModels;

/// <summary>
/// ユーザーのレスポンスを表す ViewModel
///
/// API のレスポンス JSON の形(userId, username)を表現する。
/// パスワードは含めない。
/// </summary>
public class UserResponse
{
    /// <summary>
    /// ユーザーの識別Id(UUID形式)。JSON では "userId"。
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名。JSON では "username"。
    /// </summary>
    public string Username { get; set; } = string.Empty;
}