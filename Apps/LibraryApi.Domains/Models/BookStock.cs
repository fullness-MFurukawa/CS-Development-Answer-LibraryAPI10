using LibraryApi.Domains.Exceptions;

namespace LibraryApi.Domains.Models;

/// <summary>
/// 蔵書(book_stock)エンティティクラス
/// 図書ごとの蔵書数を表すドメインモデル
/// 1つの図書に対して必ず1つの蔵書情報が存在する(1対1の関係)
///
/// 【集約】本エンティティは、集約ルートである Book に内包される内部エンティティである
/// そのため、外部から直接生成・操作されることは想定せず、必ず Bookを通じて扱う
/// また、図書への参照(book_id)は保持しない。図書との紐付けは、集約ルートであるBookが
/// 一方向(Book → BookStock)で保持することで表現する
///
/// Entityを継承することで、UUID(StockUuid)に基づく同一性判定を可能にする
/// </summary>
public class BookStock : Entity
{
    /// <summary>
    /// 識別Id(UUID形式)
    /// API のレスポンスで外部公開する識別子であり、
    /// かつエンティティの同一性判定の根拠となるIdentityが返す値
    /// </summary>
    public string StockUuid { get; private set; }

    /// <summary>
    /// 蔵書数(所蔵している冊数)
    /// 【業務ルール】0以上の整数とする(マイナス値は許可しない)
    /// テーブル定義のチェック制約(stock >= 0)に対応する
    /// </summary>
    public int Stock { get; private set; }

    /// <summary>
    /// 基底クラスEntityに、同一性判定に使う値としてUUIDを提供する
    /// これにより Equals() / GetHashCode()がStockUuid を基準に動作する
    /// </summary>
    protected override string Identity => StockUuid;

    /// <summary>
    /// 蔵書数の下限値
    /// 業務ルール(0以上)およびテーブル定義のチェック制約(stock >= 0)に対応する制約を定数化したもの
    /// </summary>
    private const int StockMinValue = 0;

    /// <summary>
    /// コンストラクタ(private)
    /// インスタンス生成の経路をファクトリメソッド(Create / Restore)に一本化するため、外部からの直接生成を禁止する
    /// </summary>
    /// <param name="stockUuid">識別Id(UUID形式)</param>
    /// <param name="stock">蔵書数</param>
    private BookStock(string stockUuid, int stock)
    {
        StockUuid = stockUuid;
        Stock = stock;
    }

    /// <summary>
    /// 新しい蔵書を生成する(新規作成)
    /// 【業務ルール】識別Id(UUID)はシステムが自動採番する
    /// 呼び出し側からUUID を受け取らないことで、採番ルールをエンティティ内に閉じ込める
    /// </summary>
    /// <param name="stock">蔵書数(0以上の整数)</param>
    /// <exception cref="DomainException">蔵書数が制約に違反する場合</exception>
    public static BookStock Create(int stock)
    {
        ValidateStock(stock);

        // 新規作成時のみ UUID を採番する
        var stockUuid = Guid.NewGuid().ToString();

        return new BookStock(stockUuid, stock);
    }

    /// <summary>
    /// 既存の蔵書を復元する(DBやViewModelなど、既にUUIDを持つデータからの再構築)
    /// Adapterが、EF CoreエンティティやViewModelから本メソッドを呼び出して
    /// ドメインエンティティを組み立てる
    /// </summary>
    /// <param name="stockUuid">既存の識別Id(UUID形式)</param>
    /// <param name="stock">蔵書数(0以上の整数)</param>
    /// <exception cref="DomainException">引数が制約に違反する場合</exception>
    public static BookStock Restore(string stockUuid, int stock)
    {
        // 復元時も、UUID と蔵書数の妥当性は検証する
        if (string.IsNullOrWhiteSpace(stockUuid))
        {
            throw new DomainException("識別Idは必須です。", nameof(stockUuid));
        }
        ValidateStock(stock);

        return new BookStock(stockUuid, stock);
    }

    /// <summary>
    /// 蔵書数を変更する
    /// プロパティを直接書き換えさせず、このメソッド経由でのみ変更を許可することで、
    /// 変更時にも必ずバリデーションが働くことを保証する
    /// 【集約】本メソッドは集約ルートである Book を通じて呼び出されることを想定する
    /// </summary>
    /// <param name="stock">変更後の蔵書数(0以上の整数)</param>
    /// <exception cref="DomainException">蔵書数が制約に違反する場合</exception>
    public void ChangeStock(int stock)
    {
        ValidateStock(stock);

        Stock = stock;
    }

    /// <summary>
    /// 蔵書数の妥当性を検証する共通ロジック
    /// 生成時(Create)・復元時(Restore)・変更時(ChangeStock)から呼び出し、
    /// 検証ルールを一箇所に集約することで重複と漏れを防ぐ
    /// </summary>
    private static void ValidateStock(int stock)
    {
        // 【業務ルール】蔵書数は0以上(マイナス値は許可しない)
        if (stock < StockMinValue)
        {
            throw new DomainException(
                $"蔵書数は{StockMinValue}以上で指定してください。", nameof(stock));
        }
    }
}