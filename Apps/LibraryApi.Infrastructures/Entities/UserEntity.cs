using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryApi.Infrastructure.Entities;
/// <summary>
/// users テーブルに対応する EF Core エンティティ(永続化モデル)
///
/// データベースの users テーブルの構造を、そのまま映したクラス
/// ・基本的なマッピング(テーブル名・カラム名・主キー・桁数)は属性で表現する
/// ・他テーブルへの参照を持たない独立したエンティティのため、属性のみで完結する
/// ・password カラムには、ハッシュ化済みのパスワードが格納される
///   (ハッシュ化の処理はアプリケーション層の責務であり、本クラスは関与しない)
/// ドメインエンティティとの相互変換は Adapter が担う
/// ITimestampedインターフェイス
/// ・CreatedAt と UpdatedAt の両方を持つエンティティが実装するインターフェイス
/// </summary>
[Table("users")]
public class UserEntity  : ITimestamped
{
    /// <summary>
    /// ユーザーId(主キー、SERIAL による自動採番)
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 識別Id(UUID形式)
    /// </summary>
    [Required]
    [Column("user_uuid")]
    [MaxLength(36)]
    public string UserUuid { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名(ログインに使用、システム内で一意)
    /// </summary>
    [Required]
    [Column("username")]
    [MaxLength(30)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// ハッシュ化済みパスワード(PBKDF2)
    /// </summary>
    [Required]
    [Column("password")]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

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