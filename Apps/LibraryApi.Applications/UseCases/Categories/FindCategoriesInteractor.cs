using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Services.Categories;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.UseCases.Categories;
/// <summary>
/// IFindCategoriesUseCase の実装(Interactor)
///
/// 分類サービスからドメインエンティティ(Category)の一覧を取得し、
/// Adapter で DTO(CategoryDto)に変換して返す。
/// 読み取りのみのため、トランザクションは用いない。
/// </summary>
public class FindCategoriesInteractor : IFindCategoriesUseCase
{
    private readonly ICategoryService _categoryService;
    private readonly IAdapter<Category, CategoryDto> _categoryDtoAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categoryService">分類サービス</param>
    /// <param name="categoryDtoAdapter">CategoryとCategoryDtoを変換するAdapter</param>
    public FindCategoriesInteractor(
        ICategoryService categoryService,
        IAdapter<Category, CategoryDto> categoryDtoAdapter)
    {
        _categoryService = categoryService;
        _categoryDtoAdapter = categoryDtoAdapter;
    }

    /// <summary>
    /// 分類一覧取得を実行する
    /// </summary>
    /// <returns>分類の DTO 一覧(0件の場合は空のリスト)</returns>
    public async Task<IReadOnlyList<CategoryDto>> ExecuteAsync()
    {
        // ドメインエンティティの一覧を取得する
        var categories = await _categoryService.FindAllAsync();
        // 各ドメインエンティティをDTOに変換して返す(ドメイン → DTO)
        return categories
            .Select(category => _categoryDtoAdapter.Convert(category))
            .ToList();
    }
}