using LibraryApi.Domains.Adapters;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Models;
namespace LibraryApi.Infrastructure.Adapters;
/// <summary>
/// ドメインエンティティ User と、EF Core エンティティ UserEntity を相互変換する Adapter
///
/// ・Convert : ドメイン → EF Core エンティティ(保存時に使用)
/// ・Restore : EF Core エンティティ → ドメイン(DB取得時に使用)
///
/// ドメインの User はハッシュ化済みパスワードを HashedPassword、
/// EF Core の UserEntity は DBカラムに対応する Password で保持するため、変換時に対応づける
/// Id・日時は Convert で設定しない(Id は DB採番、日時は DbContext が自動設定する)
/// </summary>
public class UserAdapter : IAdapter<User, UserEntity>
{
    /// <summary>
    /// ドメインエンティティを EF Core エンティティに変換する(保存用)
    /// </summary>
    public UserEntity Convert(User source)
    {
        return new UserEntity
        {
            UserUuid = source.UserUuid,
            Username = source.Username,
            // ドメインの HashedPassword を、DBカラムに対応する Password へ移す
            Password = source.HashedPassword,
        };
    }

    /// <summary>
    /// EF Core エンティティをドメインエンティティに復元する(取得用)
    /// ドメインの復元ファクトリ User.Restore に委譲する
    /// </summary>
    public User Restore(UserEntity source)
    {
        // EF Core の Password(ハッシュ済み)を、ドメインの hashedPassword 引数へ渡す
        return User.Restore(source.UserUuid, source.Username, source.Password);
    }
}