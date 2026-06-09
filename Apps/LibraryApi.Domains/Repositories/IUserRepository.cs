using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;
/// <summary>
/// ユーザー(User)の永続化を担うリポジトリのインターフェイス
/// 実装はインフラストラクチャ層に配置する
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// ユーザー名でユーザーを1件取得する
    /// ユーザー登録時の重複確認(UC-01 BR-02)、およびログイン時のユーザー検索(UC-02)で使用する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <returns>該当するユーザー。存在しない場合はnull</returns>
    Task<User?> FindByUsernameAsync(string username);

    /// <summary>
    /// ユーザーを新規追加する(変更の登録のみ。永続化の確定はUnitOfWorkが行う)
    /// ユーザー登録API(POST /users)で使用する(UC-01)
    /// </summary>
    /// <param name="user">追加するユーザー</param>
    Task AddAsync(User user);
}