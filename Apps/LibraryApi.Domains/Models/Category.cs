using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Models;
/// <summary>
/// 分類(category)エンティティクラス
/// 図書を分類するカテゴリー(例:小説、技術書、児童書)を表すドメインモデル
/// 1つの分類に対して複数の図書が紐づく(category 1 : N book)
/// Entityを継承することで、UUID(CategoryUuid)に基づく同一性判定を可能にする
/// </summary>
public class Category : Entity
{
    /// <summary>
    /// 識別Id(UUID形式)
    /// API のレスポンスで外部公開する識別子であり、
    /// かつエンティティの同一性判定の根拠となるIdentityが返す値
    /// </summary>
    public string CategoryUuid { get; private set; }

    /// <summary>
    /// 分類名(例:小説、技術書、児童書)
    /// 業務ルール上、重複を許可する(同名の分類が複数存在してよい)
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 基底クラスEntityに、同一性判定に使う値としてUUIDを提供する
    /// これにより Equals() / GetHashCode()がCategoryUuid を基準に動作する
    /// </summary>
    protected override string Identity => CategoryUuid;

    /// <summary>
    /// 分類名の最大文字数
    /// テーブル定義のVARCHAR(20)に対応する制約を定数化したもの
    /// </summary>
    private const int NameMaxLength = 20;

    /// <summary>
    /// コンストラクタ(private)
    /// インスタンス生成の経路をファクトリメソッド(Create / Restore)に一本化するため、外部からの直接生成を禁止する
    /// </summary>
    /// <param name="categoryUuid">識別Id(UUID形式)</param>
    /// <param name="name">分類名</param>
    private Category(string categoryUuid, string name)
    {
        CategoryUuid = categoryUuid;
        Name = name;
    }

    /// <summary>
    /// 新しい分類を生成する(新規作成)
    /// 【業務ルール】識別Id(UUID)はシステムが自動採番する
    /// 呼び出し側からUUID を受け取らないことで、採番ルールをエンティティ内に閉じ込める
    /// </summary>
    /// <param name="name">分類名(1~20文字)</param>
    /// <exception cref="DomainException">分類名が制約に違反する場合</exception>
    public static Category Create(string name)
    {
        ValidateName(name);

        // 新規作成時のみ UUID を採番する
        var categoryUuid = Guid.NewGuid().ToString();

        return new Category(categoryUuid, name);
    }

    /// <summary>
    /// 既存の分類を復元する(DBやViewModelなど、既にUUIDを持つデータからの再構築)
    /// Adapterが、EF CoreエンティティやViewModelから本メソッドを呼び出して
    /// ドメインエンティティを組み立てる
    /// </summary>
    /// <param name="categoryUuid">既存の識別Id(UUID形式)</param>
    /// <param name="name">分類名(1~20文字)</param>
    /// <exception cref="DomainException">引数が制約に違反する場合</exception>
    public static Category Restore(string categoryUuid, string name)
    {
        // 復元時も、UUID と分類名の妥当性は検証する
        if (string.IsNullOrWhiteSpace(categoryUuid))
        {
            throw new DomainException($"識別Idは必須です。",  nameof(categoryUuid));
        }
        ValidateName(name);

        return new Category(categoryUuid, name);
    }

    /// <summary>
    /// 分類名の妥当性を検証する共通ロジック
    /// 生成時(Create)・復元時(Restore)・変更時(ChangeName)から呼び出し、
    /// 検証ルールを一箇所に集約することで重複と漏れを防ぐ
    /// </summary>
    private static void ValidateName(string name)
    {
        // 必須チェック。null・空文字・空白のみ、をまとめて弾く
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("分類名は必須です。",nameof(name));
        }

        // 最大文字数チェック。DBの桁あふれを、永続化前にドメイン層で検出する
        if (name.Length > NameMaxLength)
        {
            throw new DomainException(
                $"分類名は{NameMaxLength}文字以内で指定してください。", nameof(name));
        }
    }

    /// <summary>
    /// 分類名を変更する
    /// プロパティを直接書き換えさせず、このメソッド経由でのみ変更を許可することで、
    /// 変更時にも必ずバリデーションが働くことを保証する
    /// </summary>
    /// <param name="name">変更後の分類名(1~20文字)</param>
    /// <exception cref="DomainException">業務制約に違反する場合</exception>
    public void ChangeName(string name)
    {
        ValidateName(name);

        Name = name;
    }
}