using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryApi.Infrastructure.Entities;
/// <summary>
/// category テーブルに対応する EF Core エンティティ(永続化モデル)
///
/// データベースの category テーブルの構造を、そのまま映したクラス
/// ・基本的なマッピング(テーブル名・カラム名・主キー・桁数)は属性で表現する
/// ・参照(リレーション)など属性で表現しにくい設定はFluent APIで補う
///   (category は他テーブルへの参照を持たないため、属性のみで完結する)
/// ドメインエンティティとの相互変換はAdapterが担う
/// </summary>
[Table("category")]
public class CategoryEntity
{
    /// <summary>
    /// 分類Id(主キー、SERIAL による自動採番)
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 識別Id(UUID形式)
    /// </summary>
    [Required]
    [Column("category_uuid")]
    [MaxLength(36)]
    public string CategoryUuid { get; set; } = string.Empty;

    /// <summary>
    /// 分類名
    /// </summary>
    [Required]
    [Column("name")]
    [MaxLength(20)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// レコード作成日時
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// レコード変更日時
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}