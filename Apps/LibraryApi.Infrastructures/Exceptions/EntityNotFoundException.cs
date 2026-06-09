namespace LibraryApi.Infrastructure.Exceptions;
/// <summary>
/// 期待したエンティティ(データ)がデータベースに存在しない場合にスローされる例外
///
/// リポジトリで、更新・削除・参照の対象が見つからない場合に用いる。
/// インフラストラクチャ層(永続化)で発生する問題であることを表す。
/// (アプリケーション層が検索結果の不在をプレゼンテーション層へ通知する目的で用いる
///  例外とは役割が異なり、本例外はデータレベルでの不在を表す)
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public EntityNotFoundException() : base() { }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    public EntityNotFoundException(string message) : base(message) { }
}