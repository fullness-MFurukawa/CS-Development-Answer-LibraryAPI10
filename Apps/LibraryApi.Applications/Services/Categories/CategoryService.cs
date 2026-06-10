using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
namespace LibraryApi.Applications.Services.Categories;
/// <summary>
/// ICategoryService の実装
///
/// 分類リポジトリ(ICategoryRepository)を利用して、分類のデータ操作を行う。
/// 入出力はドメインエンティティで扱い、DTO への変換は上位(UseCase)が担う。
/// トランザクション境界は UseCase が管理するため、本サービスは意識しない。
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categoryRepository">分類リポジトリ</param>
    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// すべての分類を取得する
    /// </summary>
    /// <returns>分類の一覧(0件の場合は空のリスト)</returns>
    public async Task<IReadOnlyList<Category>> FindAllAsync()
    {
        return await _categoryRepository.FindAllAsync();
    }

    /// <summary>
    /// 識別Id(UUID)で分類を1件取得する
    /// </summary>
    /// <param name="categoryUuid">分類の識別Id(UUID形式)</param>
    /// <returns>該当する分類。存在しない場合はnull</returns>
    public async Task<Category?> FindByUuidAsync(string categoryUuid)
    {
        return await _categoryRepository.FindByUuidAsync(categoryUuid);
    }
}