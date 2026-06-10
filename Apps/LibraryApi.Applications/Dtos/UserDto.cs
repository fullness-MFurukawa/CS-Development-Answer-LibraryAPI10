namespace LibraryApi.Applications.Dtos;
/// <summary>
/// ユーザーの情報を転送するための DTO
///
/// ユーザー登録の結果などで、ユーザーの識別Idと名前を運ぶ。
/// パスワード(ハッシュ化済みを含む)は、セキュリティ上 DTO に含めない。
/// </summary>
public class UserDto
{
    /// <summary>
    /// ユーザーの識別Id(UUID形式)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string Username { get; set; } = string.Empty;
}