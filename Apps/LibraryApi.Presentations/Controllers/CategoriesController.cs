using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.UseCases.Categories;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LibraryApi.Presentations.Controllers;
/// <summary>
/// 分類に関するAPIを提供する
///
/// 補助API:図書登録時にクライアントが分類を選択するための分類一覧を提供する。
/// </summary>
[ApiController]
[Route("library/api/categories")]
[Tags("図書の分類")]
[Authorize] 
public class CategoriesController : ControllerBase
{
    private readonly IFindCategoriesUseCase _findCategoriesUseCase;
    private readonly IAdapter<CategoryDto, CategoryResponse> _categoryResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="findCategoriesUseCase">分類一覧取得ユースケース</param>
    /// <param name="categoryResponseAdapter">CategoryDtoをCategoryResponseに変換するAdapter</param>
    public CategoriesController(
        IFindCategoriesUseCase findCategoriesUseCase,
        IAdapter<CategoryDto, CategoryResponse> categoryResponseAdapter)
    {
        _findCategoriesUseCase = findCategoriesUseCase;
        _categoryResponseAdapter = categoryResponseAdapter;
    }

    /// <summary>
    /// 分類一覧を取得する
    /// GET /library/api/categories
    /// </summary>
    /// <returns>分類一覧(200 OK)</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
    {
        // ユースケースを実行し、DTOの一覧を取得する
        var dtos = await _findCategoriesUseCase.ExecuteAsync();

        // DTOをViewModelに変換する(DTO → ViewModel)
        var response = dtos
            .Select(dto => _categoryResponseAdapter.Convert(dto))
            .ToList();

        // 200OKで分類一覧を返す
        return Ok(response);
    }
}