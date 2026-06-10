namespace LibraryApi.Applications.Dtos;
/// <summary>
/// 分類の情報を転送するための DTO(データ転送オブジェクト)
///
/// 検索系のユースケースで、ドメインエンティティ Category を本 DTO に変換し、
/// UseCase の出力として Controller へ渡す。
/// データを運ぶことに徹し、バリデーションや業務ルールは持たない
/// (入力検証はプレゼンテーション層の ViewModel、業務ルールはドメインエンティティが担う)。
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// 識別Id(UUID形式)
    /// </summary>
    public string CategoryUuid { get; set; } = string.Empty;

    /// <summary>
    /// 分類名
    /// </summary>
    public string Name { get; set; } = string.Empty;
}