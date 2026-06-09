using LibraryApi.Domains.Adapters;
using LibraryApi.Infrastructure.Contexts;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Microsoft.EntityFrameworkCore;
namespace LibraryApi.Infrastructure.Repositories;
/// <summary>
/// ICategoryRepository の実装(分類の永続化を担う)
///
/// AppDbContext を用いて category テーブルにアクセスし、
/// 取得した CategoryEntity を Adapter でドメインエンティティ Category に変換して返す。
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    private readonly IAdapter<Category, CategoryEntity> _categoryAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">EF Core データベースコンテキスト</param>
    /// <param name="categoryAdapter">分類の変換を担う Adapter</param>
    public CategoryRepository(
        AppDbContext context,
        IAdapter<Category, CategoryEntity> categoryAdapter)
    {
        _context = context;
        _categoryAdapter = categoryAdapter;
    }

    /// <summary>
    /// すべての分類を取得する
    /// </summary>
    /// <returns>分類の一覧(0件の場合は空のリスト)</returns>
    public async Task<IReadOnlyList<Category>> FindAllAsync()
    {
        // EF Core で全件取得する。読み取り専用のため AsNoTracking で追跡コストを省く
        var entities = await _context.Categories
            .AsNoTracking()
            .ToListAsync();

        // 取得した各 CategoryEntity を、Adapter でドメインの Category に変換する
        return entities
            .Select(entity => _categoryAdapter.Restore(entity))
            .ToList();
    }

    /// <summary>
    /// 識別Id(UUID)で分類を1件取得する
    /// </summary>
    /// <param name="categoryUuid">分類の識別Id(UUID形式)</param>
    /// <returns>該当する分類。存在しない場合はnull</returns>
    public async Task<Category?> FindByUuidAsync(string categoryUuid)
    {
        // category_uuid で1件を検索する(該当なしの場合は null が返る)
        var entity = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CategoryUuid == categoryUuid);

        // 見つからなければnull、見つかればAdapterでドメインに変換して返す
        return entity is null
            ? null
            : _categoryAdapter.Restore(entity);
    }
}