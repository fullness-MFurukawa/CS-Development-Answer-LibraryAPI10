using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Models;

/// <summary>
/// ユーザー(users)エンティティクラス
/// システム利用者の認証情報を表すドメインモデル
/// 図書管理機能を利用するすべての利用者は、事前にユーザー登録を行う必要がある
/// 他のエンティティ(Category / Book / BookStock)とは関連を持たない認証専用のエンティティ
/// Entityを継承することで、UUID(UserUuid)に基づく同一性判定を可能にする
/// </summary>
public class User : Entity
{
    /// <summary>
    /// 識別Id(UUID形式)
    /// API のレスポンスで外部公開する識別子であり、
    /// かつエンティティの同一性判定の根拠となるIdentityが返す値
    /// </summary>
    public string UserUuid { get; private set; }

    /// <summary>
    /// ユーザー名
    /// ログイン時に使用し、システム全体で一意でなければならない
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// ハッシュ化済みパスワード
    /// 【重要】このエンティティが保持するのは、必ずハッシュ化された後のパスワードである
    /// 平文パスワードのハッシュ化(PBKDF2)は、技術的詳細であるためアプリケーション層が担う
    /// ドメイン層は平文パスワードを一切受け取らず、知らない
    /// </summary>
    public string HashedPassword { get; private set; }

    /// <summary>
    /// 基底クラスEntityに、同一性判定に使う値としてUUIDを提供する
    /// これにより Equals() / GetHashCode()がUserUuid を基準に動作する
    /// </summary>
    protected override string Identity => UserUuid;

    /// <summary>
    /// ユーザー名の最小文字数
    /// テーブル定義およびUC-01の制約(1~30文字)に対応する制約を定数化したもの
    /// </summary>
    private const int UsernameMinLength = 1;

    /// <summary>
    /// ユーザー名の最大文字数
    /// テーブル定義のVARCHAR(30)に対応する制約を定数化したもの
    /// </summary>
    private const int UsernameMaxLength = 30;


    /// <summary>
    /// コンストラクタ(private)
    /// インスタンス生成の経路をファクトリメソッド(Create / Restore)に一本化するため、外部からの直接生成を禁止する
    /// </summary>
    /// <param name="userUuid">識別Id(UUID形式)</param>
    /// <param name="username">ユーザー名</param>
    /// <param name="hashedPassword">ハッシュ化済みパスワード</param>
    private User(string userUuid, string username, string hashedPassword)
    {
        UserUuid = userUuid;
        Username = username;
        HashedPassword = hashedPassword;
    }
  
    /// <summary>
    /// 新しいユーザーを生成する(新規作成)
    /// 【業務ルール】識別Id(UUID)はシステムが自動採番する
    /// 呼び出し側からUUID を受け取らないことで、採番ルールをエンティティ内に閉じ込める
    /// 【役割分担】引数のパスワードは、アプリケーション層でハッシュ化済みのものを受け取る
    /// </summary>
    /// <param name="username">ユーザー名(1~30文字)</param>
    /// <param name="hashedPassword">ハッシュ化済みパスワード</param>
    /// <exception cref="DomainException">引数が制約に違反する場合</exception>
    public static User Create(string username, string hashedPassword)
    {
        ValidateUsername(username);
        ValidateHashedPassword(hashedPassword);

        // 新規作成時のみ UUID を採番する
        var userUuid = Guid.NewGuid().ToString();

        return new User(userUuid, username, hashedPassword);
    }

    /// <summary>
    /// 既存のユーザーを復元する(DBやViewModelなど、既にUUIDを持つデータからの再構築)
    /// Adapterが、EF CoreエンティティやViewModelから本メソッドを呼び出して
    /// ドメインエンティティを組み立てる
    /// </summary>
    /// <param name="userUuid">既存の識別Id(UUID形式)</param>
    /// <param name="username">ユーザー名(1~30文字)</param>
    /// <param name="hashedPassword">ハッシュ化済みパスワード</param>
    /// <exception cref="DomainException">引数が制約に違反する場合</exception>
    public static User Restore(string userUuid, string username, string hashedPassword)
    {
        // 復元時も、UUID と各項目の妥当性は検証する
        if (string.IsNullOrWhiteSpace(userUuid))
        {
            throw new Domains.Exceptions.DomainException("識別Idは必須です。", nameof(userUuid));
        }
        ValidateUsername(username);
        ValidateHashedPassword(hashedPassword);

        return new User(userUuid, username, hashedPassword);
    }

    /// <summary>
    /// ユーザー名の妥当性を検証する共通ロジック
    /// 生成時(Create)・復元時(Restore)から呼び出し、
    /// 検証ルールを一箇所に集約することで重複と漏れを防ぐ
    /// </summary>
    private static void ValidateUsername(string username)
    {
        // 必須チェック。null・空文字・空白のみ、をまとめて弾く
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new DomainException("ユーザー名は必須です。", nameof(username));
        }

        // 文字数チェック。テーブル定義(VARCHAR(30))およびUC-01の制約に対応する
        if (username.Length < UsernameMinLength || username.Length > UsernameMaxLength)
        {
            throw new DomainException(
                $"ユーザー名は{UsernameMinLength}~{UsernameMaxLength}文字で指定してください。",
                nameof(username));
        }
    }

    /// <summary>
    /// ハッシュ化済みパスワードの妥当性を検証する共通ロジック
    /// 【役割分担】平文パスワードの「8文字以上」という制約は、平文に対するものであり、
    /// ハッシュ化後の文字列長は元の長さと無関係なため、ここでは検証できない
    /// よって、平文の長さ検証はアプリケーション層(ハッシュ化前)が担い、
    /// ドメイン層では「ハッシュ化済みパスワードが存在すること(必須)」のみを保証する
    /// </summary>
    private static void ValidateHashedPassword(string hashedPassword)
    {
        // 必須チェックのみ。平文の長さ制約はアプリケーション層の責務
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new DomainException("パスワードは必須です。", nameof(hashedPassword));
        }
    }
}   