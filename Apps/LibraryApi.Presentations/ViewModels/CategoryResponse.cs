namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// 分類のレスポンスを表す ViewModel
///
/// API のレスポンス JSON の形(categoryId, name)を表現する。
/// アプリケーション層の CategoryDto(CategoryUuid, Name)を、本 ViewModel に変換して返す。
/// JSON フィールド名(categoryId)は、プレゼンテーション層の関心事として本クラスで定める。
/// </summary>
public class CategoryResponse
{
    /// <summary>
    /// 分類の識別Id(UUID形式)。JSON では "categoryId" として出力される。
    /// </summary>
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>
    /// 分類名。JSON では "name" として出力される。
    /// </summary>
    public string Name { get; set; } = string.Empty;
}