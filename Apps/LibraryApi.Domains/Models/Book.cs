using LibraryApi.Domains.Exceptions;

namespace LibraryApi.Domains.Models;

/// <summary>
/// 図書(book)エンティティクラス
/// 図書管理システムで管理する図書の基本情報を表すドメインモデル
///
/// 【集約】本エンティティは集約ルートであり、以下の2つを保持する
///   ・BookStock(蔵書) … 内包する内部エンティティ。図書に従属し、ライフサイクルを共にする
///   ・Category(分類) … 参照として保持する別集約。図書とは独立して存在する
/// 「従属する部品は内包、独立した別物は参照」という集約設計の考え方に基づく
///
/// 蔵書の操作は、必ず集約ルートである本エンティティを通じて行い、
/// 図書と蔵書の整合性(同一トランザクションでの永続化・更新・削除)を保証する
///
/// Entityを継承することで、UUID(BookUuid)に基づく同一性判定を可能にする
/// </summary>
public class Book : Entity
{
    /// <summary>
    /// 識別Id(UUID形式)
    /// API のレスポンスで外部公開する識別子であり、
    /// かつエンティティの同一性判定の根拠となるIdentityが返す値
    /// </summary>
    public string BookUuid { get; private set; }

    /// <summary>
    /// 書名
    /// 業務ルール上、重複を許可する(同じ書名の図書が複数登録できる)
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// 著者名
    /// 業務ルール上、重複を許可する(同じ著者の図書が複数登録できる)
    /// </summary>
    public string Author { get; private set; }

    /// <summary>
    /// 分類(参照)
    /// 【集約】この図書が属する分類への参照を保持する(内包ではなく参照)
    /// 分類は図書とは独立した別集約であり、複数の図書から共有される(category 1 : N book)
    /// 検索や登録のレスポンスで、図書と一緒に分類情報を返すために保持する
    /// </summary>
    public Category Category { get; private set; }

    /// <summary>
    /// 蔵書(内包)
    /// 【集約】この図書に従属する蔵書情報を内包する(1対1の関係)
    /// 蔵書数の操作は、必ず本エンティティのメソッドを通じて行う
    /// </summary>
    public BookStock BookStock { get; private set; }

    /// <summary>
    /// 基底クラスEntityに、同一性判定に使う値としてUUIDを提供する
    /// これにより Equals() / GetHashCode()がBookUuid を基準に動作する
    /// </summary>
    protected override string Identity => BookUuid;

    /// <summary>
    /// 書名の最大文字数
    /// テーブル定義のVARCHAR(50)に対応する制約を定数化したもの
    /// </summary>
    private const int TitleMaxLength = 50;

    /// <summary>
    /// 著者名の最大文字数
    /// テーブル定義のVARCHAR(30)に対応する制約を定数化したもの
    /// </summary>
    private const int AuthorMaxLength = 30;

    /// <summary>
    /// コンストラクタ(private)
    /// インスタンス生成の経路をファクトリメソッド(Create / Restore)に一本化するため、外部からの直接生成を禁止する
    /// </summary>
    /// <param name="bookUuid">識別Id(UUID形式)</param>
    /// <param name="title">書名</param>
    /// <param name="author">著者名</param>
    /// <param name="category">分類(参照)</param>
    /// <param name="bookStock">蔵書(内包)</param>
    private Book(string bookUuid, string title, string author, Category category, BookStock bookStock)
    {
        BookUuid = bookUuid;
        Title = title;
        Author = author;
        Category = category;
        BookStock = bookStock;
    }

    /// <summary>
    /// 新しい図書を生成する(新規作成)
    /// 【業務ルール】識別Id(UUID)はシステムが自動採番する
    /// 呼び出し側からUUID を受け取らないことで、採番ルールをエンティティ内に閉じ込める
    /// 【集約】蔵書(BookStock)は、本メソッドの中で蔵書数から新規生成し、内包する
    /// これにより、図書と蔵書が必ずセットで生成されること(一方だけの生成を許さない)を保証する
    /// </summary>
    /// <param name="title">書名(1~50文字)</param>
    /// <param name="author">著者名(1~30文字)</param>
    /// <param name="category">分類(参照、null不可)</param>
    /// <param name="stock">初期の蔵書数(0以上の整数)</param>
    /// <exception cref="DomainException">引数が制約に違反する場合</exception>
    public static Book Create(string title, string author, Category category, int stock)
    {
        ValidateTitle(title);
        ValidateAuthor(author);
        ValidateCategory(category);

        // 新規作成時のみ UUID を採番する
        var bookUuid = Guid.NewGuid().ToString();

        // 蔵書を内部で新規生成して内包する(蔵書数のバリデーションは BookStock 側が担う)
        var bookStock = BookStock.Create(stock);

        return new Book(bookUuid, title, author, category, bookStock);
    }

    /// <summary>
    /// 既存の図書を復元する(DBやViewModelなど、既にUUIDを持つデータからの再構築)
    /// Adapterが、EF CoreエンティティやViewModelから本メソッドを呼び出して
    /// ドメインエンティティを組み立てる
    /// 【集約】復元時は、既にUUIDを持つ蔵書(BookStock)を丸ごと受け取って内包する
    /// (新規作成と異なり、蔵書数からの生成ではなく、復元済みの蔵書を引き継ぐ)
    /// </summary>
    /// <param name="bookUuid">既存の識別Id(UUID形式)</param>
    /// <param name="title">書名(1~50文字)</param>
    /// <param name="author">著者名(1~30文字)</param>
    /// <param name="category">分類(参照、null不可)</param>
    /// <param name="bookStock">蔵書(復元済み、null不可)</param>
    /// <exception cref="DomainException">引数が制約に違反する場合</exception>
    public static Book Restore(string bookUuid, string title, string author, Category category, BookStock bookStock)
    {
        // 復元時も、UUID と各項目の妥当性は検証する
        if (string.IsNullOrWhiteSpace(bookUuid))
        {
            throw new DomainException("識別Idは必須です。", nameof(bookUuid));
        }
        ValidateTitle(title);
        ValidateAuthor(author);
        ValidateCategory(category);

        // 蔵書(内包)も必須。復元元のデータに蔵書が欠けている状態は許可しない
        if (bookStock is null)
        {
            throw new DomainException("蔵書は必須です。", nameof(bookStock));
        }

        return new Book(bookUuid, title, author, category, bookStock);
    }

    /// <summary>
    /// 図書情報を変更する
    /// 【業務ルール(UC-05 BR-02)】変更可能な項目は、書名・著者名・蔵書数の3項目のみ
    /// 分類の変更は本ユースケースの対象外とする
    /// 【集約】蔵書数の変更は、内包する BookStock に委譲する
    /// これにより、図書の変更と蔵書の変更が、集約ルートを通じて一括で行われることを保証する
    /// </summary>
    /// <param name="title">変更後の書名(1~50文字)</param>
    /// <param name="author">変更後の著者名(1~30文字)</param>
    /// <param name="stock">変更後の蔵書数(0以上の整数)</param>
    /// <exception cref="DomainException">引数が制約に違反する場合</exception>
    public void ChangeBookInfo(string title, string author, int stock)
    {
        ValidateTitle(title);
        ValidateAuthor(author);

        Title = title;
        Author = author;

        // 蔵書数の変更は内包する蔵書へ委譲する(蔵書数のバリデーションは BookStock 側が担う)
        BookStock.ChangeStock(stock);
    }

    /// <summary>
    /// 書名の妥当性を検証する共通ロジック
    /// 生成時(Create)・復元時(Restore)・変更時(ChangeBookInfo)から呼び出し、
    /// 検証ルールを一箇所に集約することで重複と漏れを防ぐ
    /// </summary>
    private static void ValidateTitle(string title)
    {
        // 必須チェック。null・空文字・空白のみ、をまとめて弾く
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("書名は必須です。", nameof(title));
        }

        // 最大文字数チェック。DB の桁あふれを、永続化前にドメイン層で検出する
        if (title.Length > TitleMaxLength)
        {
            throw new DomainException(
                $"書名は{TitleMaxLength}文字以内で指定してください。", nameof(title));
        }
    }

    /// <summary>
    /// 著者名の妥当性を検証する共通ロジック
    /// 生成時(Create)・復元時(Restore)・変更時(ChangeBookInfo)から呼び出し、
    /// 検証ルールを一箇所に集約することで重複と漏れを防ぐ
    /// </summary>
    private static void ValidateAuthor(string author)
    {
        // 必須チェック。null・空文字・空白のみ、をまとめて弾く
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new DomainException("著者名は必須です。", nameof(author));
        }

        // 最大文字数チェック。DB の桁あふれを、永続化前にドメイン層で検出する
        if (author.Length > AuthorMaxLength)
        {
            throw new DomainException(
                $"著者名は{AuthorMaxLength}文字以内で指定してください。", nameof(author));
        }
    }

    /// <summary>
    /// 分類(参照)の妥当性を検証する共通ロジック
    /// 【役割分担】ここで検証するのは「分類への参照が存在すること(null でないこと)」のみ
    /// 「指定された分類が実在するか(DBに登録済みか)」の確認は、DBアクセスを伴うため
    /// アプリケーション層・リポジトリの責務とする(UC-04 BR-03: 参照整合性)
    /// </summary>
    private static void ValidateCategory(Category category)
    {
        // 【業務ルール(UC-04 BR-03)】図書は必ずいずれかの分類に属する(分類は必須)
        if (category is null)
        {
            throw new DomainException("分類は必須です。", nameof(category));
        }
    }
}