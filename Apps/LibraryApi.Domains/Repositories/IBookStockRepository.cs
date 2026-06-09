using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;
/// <summary>
/// 蔵書(BookStock)の永続化を担うリポジトリのインターフェイス
/// 蔵書は図書(Book)に内包される集約の内部エンティティだが、本プロジェクトの方針として
/// エンティティごとにリポジトリを設け、アプリケーション層が図書のリポジトリと協調させて
/// 同一トランザクションで永続化する
/// 取得は図書(Book)経由で行うため、本リポジトリは書き込み系の操作のみを提供する
/// 実装はインフラストラクチャ層に配置する
/// </summary>
public interface IBookStockRepository
{
    /// <summary>
    /// 蔵書を新規追加する(変更の登録のみ。永続化の確定はUnitOfWorkが行う)
    /// 図書登録API(POST /books)で、図書と同一トランザクションで使用する(UC-04)
    /// </summary>
    /// <param name="bookStock">追加する蔵書</param>
    Task AddAsync(BookStock bookStock);

    /// <summary>
    /// 蔵書を更新する(変更の登録のみ。永続化の確定はUnitOfWorkが行う)
    /// 図書変更API(PUT /books/{bookId})で、図書と同一トランザクションで使用する(UC-05)
    /// </summary>
    /// <param name="bookStock">更新する蔵書</param>
    Task UpdateAsync(BookStock bookStock);

    /// <summary>
    /// 蔵書を削除する(変更の登録のみ。永続化の確定はUnitOfWorkが行う)
    /// 図書削除API(DELETE /books/{bookId})で、図書と同一トランザクションで使用する(UC-06)
    /// </summary>
    /// <param name="bookStock">削除する蔵書</param>
    Task DeleteAsync(BookStock bookStock);
}