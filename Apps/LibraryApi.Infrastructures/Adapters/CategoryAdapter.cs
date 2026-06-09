using LibraryApi.Domains.Adapters;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Models;
namespace LibraryApi.Infrastructure.Adapters;
/// <summary>
/// ドメインエンティティ Category と、EF Core エンティティ CategoryEntity を相互変換する Adapter
///
/// ・Convert : ドメイン → EF Core エンティティ(保存時に使用)
/// ・Restore : EF Core エンティティ → ドメイン(DB取得時に使用)
///
/// ドメインの Category は DB採番の Id や日時を持たないため、
/// ・Restore では Id を使わず、UUID と各値だけをドメインの復元ファクトリへ渡す
/// ・Convert では Id・日時を設定しない(Id は DB採番、日時は DbContext が保存時に自動設定する)
/// </summary>
public class CategoryAdapter : IAdapter<Category, CategoryEntity>
{
    /// <summary>
    /// ドメインエンティティを EF Core エンティティに変換する(保存用)
    /// Id は設定しない(新規追加時は DB が採番する)
    /// 日時は設定しない(DbContext の SaveChangesAsync が自動設定する)
    /// </summary>
    /// <param name="source">ドメインエンティティ Category</param>
    /// <returns>EF Core エンティティ CategoryEntity</returns>
    public CategoryEntity Convert(Category source)
    {
        return new CategoryEntity
        {
            CategoryUuid = source.CategoryUuid,
            Name = source.Name,
        };
    }

    /// <summary>
    /// EF Core エンティティをドメインエンティティに復元する(取得用)
    /// ドメインの復元ファクトリ Category.Restore に委譲することで、
    /// 復元時もドメインの不変条件(検証)が働くことを保証する
    /// </summary>
    /// <param name="source">EF Core エンティティ CategoryEntity</param>
    /// <returns>ドメインエンティティ Category</returns>
    public Category Restore(CategoryEntity source)
    {
        return Category.Restore(source.CategoryUuid, source.Name);
    }
}