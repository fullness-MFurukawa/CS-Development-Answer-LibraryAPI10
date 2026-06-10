using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// アプリケーション層の CategoryDto と、プレゼンテーション層の CategoryResponse(ViewModel)を
/// 変換する Adapter
///
/// ・Convert : DTO → ViewModel(レスポンスを組み立てる際に使用)
/// ・Restore : ViewModel → DTO。分類のレスポンスは出力専用のため、現状サポートしない。
///
/// 層の境界での変換を、ドメイン層で定義した IAdapter で一貫して表現する。
/// </summary>
public class CategoryResponseAdapter : IAdapter<CategoryDto, CategoryResponse>
{
    /// <summary>
    /// DTO を ViewModel に変換する(DTO → ViewModel)
    /// DTO のプロパティ名(CategoryUuid)を、API レスポンスのフィールド名に対応する
    /// ViewModel のプロパティ(CategoryId)へ移す。
    /// </summary>
    /// <param name="source">アプリケーション層の CategoryDto</param>
    /// <returns>プレゼンテーション層の CategoryResponse</returns>
    public CategoryResponse Convert(CategoryDto source)
    {
        return new CategoryResponse
        {
            CategoryId = source.CategoryUuid,
            Name = source.Name,
        };
    }

    /// <summary>
    /// ViewModel を DTO に変換する(ViewModel → DTO)。現状サポートしない。
    /// </summary>
    /// <exception cref="NotSupportedException">本変換はサポートしていない</exception>
    public CategoryDto Restore(CategoryResponse source)
    {
        throw new NotSupportedException(
            "CategoryResponseからCategoryDtoへの変換はサポートしていません。");
    }
}