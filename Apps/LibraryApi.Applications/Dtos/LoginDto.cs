namespace LibraryApi.Applications.Dtos;
/// <summary>
/// ログインの入力情報を転送するための DTO(UC-02)
///
/// ユーザー名と平文パスワードを運ぶ。パスワードの照合はUseCase内で行う。
/// </summary>
public class LoginDto
{
    /// <summary>
    /// ユーザー名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 平文パスワード(UseCase 内でハッシュと照合される)
    /// </summary>
    public string Password { get; set; } = string.Empty;
}