using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryApi.Infrastructure.Entities;
/// <summary>
/// book_stock テーブルに対応する EF Core エンティティ(永続化モデル)
///
/// データベースの book_stock テーブルの構造を、そのまま映したクラス
/// ・基本的なマッピング(テーブル名・カラム名・主キー・桁数)は属性で表現する
/// ・図書(book)への外部キー book_id を保持する(1対1。外部キーは本テーブル側にある)
/// ・逆参照(BookEntity への参照)は持たない(図書を起点に蔵書を取得する片方向のみ)
/// ・蔵書数の「0以上」のチェック制約は属性で表現できないため、Fluent API で定義する
/// ドメインエンティティとの相互変換は Adapter が担う
/// </summary>
[Table("book_stock")]
public class BookStockEntity
{
    /// <summary>
    /// 蔵書Id(主キー、SERIAL による自動採番)
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 識別Id(UUID形式)
    /// </summary>
    [Required]
    [Column("stock_uuid")]
    [MaxLength(36)]
    public string StockUuid { get; set; } = string.Empty;

    /// <summary>
    /// 所蔵している冊数(0以上)
    /// 「0以上」のチェック制約は Fluent API(HasCheckConstraint)で定義する
    /// </summary>
    [Required]
    [Column("stock")]
    public int Stock { get; set; }

    /// <summary>
    /// 図書Id(book.id への外部キー、1対1)
    /// 外部キーのカラム自体は属性で表現し、リレーションの関連付けは Fluent API で定義する
    /// </summary>
    [Required]
    [Column("book_id")]
    public int BookId { get; set; }

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