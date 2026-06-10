using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Adapters;
/// <summary>
/// ドメインエンティティ User と、DTO である UserDto を変換する Adapter
///
/// ・Convert : ドメイン → DTO(ユーザー登録の結果などを返す際に使用)
///   パスワード(ハッシュ化済み)は DTO に含めない。
/// ・Restore : DTO → ドメイン。UserDto からドメインを復元する場面は無いため、現状サポートしない。
/// </summary>
public class UserDtoAdapter : IAdapter<User, UserDto>
{
    /// <summary>
    /// ドメインエンティティを DTO に変換する(ドメイン → DTO)
    /// </summary>
    /// <param name="source">ドメインエンティティ User</param>
    /// <returns>DTO である UserDto</returns>
    public UserDto Convert(User source)
    {
        return new UserDto
        {
            UserId = source.UserUuid,
            Username = source.Username,
        };
    }

    /// <summary>
    /// DTO をドメインに復元する(DTO → ドメイン)。現状サポートしない。
    /// </summary>
    /// <exception cref="NotSupportedException">本変換はサポートしていない</exception>
    public User Restore(UserDto source)
    {
        throw new NotSupportedException(
            "UserDto から User への変換はサポートしていません。");
    }
}