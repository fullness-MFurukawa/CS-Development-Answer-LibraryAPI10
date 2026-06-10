namespace LibraryApi.Applications.Dtos;
/// <summary>
/// ログイン結果を転送するための DTO(UC-02)
///
/// 発行された JWT アクセストークンを運ぶ。
/// このトークンを HttpOnly Cookie にセットするのは、プレゼンテーション層(Controller)の責務。
/// </summary>
public class LoginResultDto
{
    /// <summary>
    /// 発行された JWT アクセストークン
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
}