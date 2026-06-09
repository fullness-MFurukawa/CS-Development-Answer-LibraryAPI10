using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryApi.Infrastructure.Entities;
/// <summary>
/// book テーブルに対応する EF Core エンティティ(永続化モデル)
///
/// データベースの book テーブルの構造を、そのまま映したクラス
/// ・基本的なマッピング(テーブル名・カラム名・主キー・桁数)は属性で表現する
/// ・他テーブルへの参照(Category への多対1、BookStock への1対1)は
///   ナビゲーションプロパティとして持ち、リレーションの関連付けは Fluent API で定義する
/// ・逆参照は持たない(図書を起点に分類・蔵書を取得する片方向のみ)
/// ドメインエンティティとの相互変換は Adapter / 集約の構築は Aggregator が担う
/// </summary>
[Table("book")]
public class BookEntity
{
    /// <summary>
    /// 図書Id(主キー、SERIAL による自動採番)
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 識別Id(UUID形式)
    /// </summary>
    [Required]
    [Column("book_uuid")]
    [MaxLength(36)]
    public string BookUuid { get; set; } = string.Empty;

    /// <summary>
    /// 書名
    /// </summary>
    [Required]
    [Column("title")]
    [MaxLength(50)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 著者名
    /// </summary>
    [Required]
    [Column("author")]
    [MaxLength(30)]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 分類Id(category.id への外部キー)
    /// 外部キーのカラム自体は属性で表現し、リレーションの関連付けは Fluent API で定義する
    /// </summary>
    [Required]
    [Column("category_id")]
    public int CategoryId { get; set; }

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

    // ─────────────────────────────────────────────
    // ナビゲーションプロパティ(参照)
    // 属性は付けず、リレーションの定義は Fluent API で行う
    // ─────────────────────────────────────────────

    /// <summary>
    /// 分類への参照(多対1)
    /// 多くの図書が1つの分類に属する。外部キーは本テーブルの category_id
    /// </summary>
    public CategoryEntity Category { get; set; } = null!;

    /// <summary>
    /// 蔵書への参照(1対1)
    /// 1つの図書に1つの蔵書が対応する。外部キー(book_id)は book_stock テーブル側にある
    /// </summary>
    public BookStockEntity BookStock { get; set; } = null!;
}