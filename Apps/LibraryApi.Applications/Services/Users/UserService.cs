using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
namespace LibraryApi.Applications.Services.Users;
/// <summary>
/// IUserService の実装
///
/// ユーザーリポジトリ(IUserRepository)を利用して、ユーザーのデータ操作を行う。
/// 入出力はドメインエンティティで扱い、認証(ハッシュ化・照合)は UseCase が担う。
/// トランザクション境界は UseCase が管理するため、本サービスは意識しない。
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="userRepository">ユーザーリポジトリ</param>
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// ユーザー名でユーザーを1件取得する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <returns>該当するユーザー。存在しない場合はnull</returns>
    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await _userRepository.FindByUsernameAsync(username);
    }

    /// <summary>
    /// ユーザーを新規追加する
    /// </summary>
    /// <param name="user">追加するユーザー(ハッシュ化済みパスワードを保持)</param>
    public async Task AddAsync(User user)
    {
        await _userRepository.AddAsync(user);
    }
}