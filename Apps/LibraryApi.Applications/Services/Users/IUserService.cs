using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Services.Users;
/// <summary>
/// ユーザーに関する操作を提供するサービスのインターフェイス
///
/// リポジトリを利用してユーザーのデータ操作を行う。
/// 入出力はドメインエンティティ(User)で扱う。
/// パスワードのハッシュ化・照合といった認証の関心事は本サービスでは扱わず、UseCase が担う。
/// </summary>
public interface IUserService
{
    /// <summary>
    /// ユーザー名でユーザーを1件取得する
    /// ログイン時のユーザー取得、および登録時の重複確認で使用する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <returns>該当するユーザー。存在しない場合はnull</returns>
    Task<User?> FindByUsernameAsync(string username);

    /// <summary>
    /// ユーザーを新規追加する
    /// 渡される User は、ハッシュ化済みのパスワードを保持している前提
    /// (ハッシュ化は UseCase が行う)
    /// </summary>
    /// <param name="user">追加するユーザー</param>
    Task AddAsync(User user);
}