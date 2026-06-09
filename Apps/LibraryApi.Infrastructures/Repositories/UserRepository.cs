using LibraryApi.Domains.Adapters;
using LibraryApi.Infrastructure.Contexts;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Microsoft.EntityFrameworkCore;
namespace LibraryApi.Infrastructure.Repositories;
/// <summary>
/// IUserRepository の実装(ユーザーの永続化を担う)
///
/// AppDbContext を用いて users テーブルにアクセスし、
/// UserEntity とドメインエンティティ User の変換は Adapter に委譲する。
/// 追加操作では SaveChangesAsync までを呼び、データ操作を完結させる。
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly IAdapter<User, UserEntity> _userAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">EF Core データベースコンテキスト</param>
    /// <param name="userAdapter">ユーザーの変換を担う Adapter</param>
    public UserRepository(
        AppDbContext context,
        IAdapter<User, UserEntity> userAdapter)
    {
        _context = context;
        _userAdapter = userAdapter;
    }

    /// <summary>
    /// ユーザー名でユーザーを1件取得する
    /// ユーザー登録時の重複確認、およびログイン時のユーザー検索で使用する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <returns>該当するユーザー。存在しない場合はnull</returns>
    public async Task<User?> FindByUsernameAsync(string username)
    {
        // username で1件を検索する(該当なしの場合は null が返る)
        var entity = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Username == username);

        return entity is null
            ? null
            : _userAdapter.Restore(entity);
    }

    /// <summary>
    /// ユーザーを新規追加する
    /// データ操作を完結させるため、SaveChangesAsync までを本メソッドで行う
    /// (日時 created_at / updated_at は AppDbContext の SaveChangesAsync が自動設定する)
    /// </summary>
    /// <param name="user">追加するユーザー</param>
    public async Task AddAsync(User user)
    {
        // ドメインエンティティを EF Core エンティティに変換する
        var entity = _userAdapter.Convert(user);

        // コンテキストへ追加登録し、永続化する
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}