using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;
/// <summary>
/// 分類(Category)の永続化を担うリポジトリのインターフェイス
/// 実装はインフラストラクチャ層に配置し、依存性逆転の原則によりドメイン層は実装に依存しない
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// すべての分類を取得する
    /// 分類一覧取得API(GET /categories)で使用する
    /// </summary>
    /// <returns>分類の一覧(0件の場合は空のリスト)</returns>
    Task<IReadOnlyList<Category>> FindAllAsync();

    /// <summary>
    /// 識別Id(UUID)で分類を1件取得する
    /// 図書登録時に、指定された分類が実在するかの確認(UC-04 BR-03)で使用する
    /// </summary>
    /// <param name="categoryUuid">分類の識別Id(UUID形式)</param>
    /// <returns>該当する分類。存在しない場合はnull</returns>
    Task<Category?> FindByUuidAsync(string categoryUuid);
}