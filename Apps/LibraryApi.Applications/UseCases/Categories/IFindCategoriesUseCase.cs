using LibraryApi.Applications.Dtos;
namespace LibraryApi.Applications.UseCases.Categories;
/// <summary>
/// 分類一覧を取得するユースケースのインターフェイス
///
/// Controllerはこのインターフェイスに依存し、実装(Interactor)には依存しない。
/// </summary>
public interface IFindCategoriesUseCase
{
    /// <summary>
    /// 分類一覧取得を実行する
    /// </summary>
    /// <returns>分類の DTO 一覧(0件の場合は空のリスト)</returns>
    Task<IReadOnlyList<CategoryDto>> ExecuteAsync();
}