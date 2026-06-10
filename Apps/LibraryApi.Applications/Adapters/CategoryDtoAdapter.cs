using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Adapters;
/// <summary>
/// ドメインエンティティ Category と、DTO である CategoryDto を相互変換する Adapter
///
/// ・Convert : ドメイン → DTO(検索系で、UseCase の出力を組み立てる際に使用)
/// ・Restore : DTO → ドメイン(更新系で使用する想定。分類には現状、更新系のユースケースが
///   存在しないため未実装とし、必要になった時点で実装する)
/// </summary>
public class CategoryDtoAdapter : IAdapter<Category, CategoryDto>
{
    /// <summary>
    /// ドメインエンティティを DTO に変換する(検索系:ドメイン → DTO)
    /// </summary>
    /// <param name="source">ドメインエンティティ Category</param>
    /// <returns>DTO である CategoryDto</returns>
    public CategoryDto Convert(Category source)
    {
        return new CategoryDto
        {
            CategoryUuid = source.CategoryUuid,
            Name = source.Name,
        };
    }

    /// <summary>
    /// DTO をドメインエンティティに復元する(更新系:DTO → ドメイン)
    ///
    /// 分類には現状、更新系のユースケース(DTO からドメインを構築する場面)が存在しないため、
    /// 未実装とする。必要になった時点で、Category.Restore へ委譲して実装すること。
    /// </summary>
    /// <param name="source">DTO である CategoryDto</param>
    /// <returns>ドメインエンティティ Category</returns>
    /// <exception cref="NotSupportedException">現状、本変換はサポートしていない</exception>
    public Category Restore(CategoryDto source)
    {
        throw new NotSupportedException(
            "CategoryDtoからドメインへの変換は現状サポートしていません。" +
            "分類の更新系ユースケースを追加する際に、Category.Restoreへ委譲して実装してください。");
    }
}