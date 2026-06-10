namespace LibraryApi.Applications.Dtos;

/// <summary>
/// ユーザー登録の入力情報を転送するための DTO(UC-01)
///
/// ユーザー名と平文パスワードを運ぶ。パスワードのハッシュ化は UseCase 内で行う。
/// データを運ぶことに徹し、入力検証はプレゼンテーション層の ViewModel が担う。
/// </summary>
public class RegisterUserDto
{
    /// <summary>
    /// ユーザー名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 平文パスワード(UseCase 内でハッシュ化される)
    /// </summary>
    public string Password { get; set; } = string.Empty;
}