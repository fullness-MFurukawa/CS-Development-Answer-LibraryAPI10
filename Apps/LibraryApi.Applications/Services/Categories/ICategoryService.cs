using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Services.Categories;
/// <summary>
/// 分類に関する操作を提供するサービスのインターフェイス
///
/// リポジトリを利用して分類のデータ操作を行う。
/// 入出力はドメインエンティティ(Category)で扱い、DTO への変換は上位(UseCase)が担う。
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// すべての分類を取得する
    /// </summary>
    /// <returns>分類の一覧(0件の場合は空のリスト)</returns>
    Task<IReadOnlyList<Category>> FindAllAsync();

    /// <summary>
    /// 識別Id(UUID)で分類を1件取得する
    /// 図書登録時に、指定された分類が実在するかの確認で使用する
    /// </summary>
    /// <param name="categoryUuid">分類の識別Id(UUID形式)</param>
    /// <returns>該当する分類。存在しない場合はnull</returns>
    Task<Category?> FindByUuidAsync(string categoryUuid);
}