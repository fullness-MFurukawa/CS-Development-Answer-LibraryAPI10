using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;
/// <summary>
/// 図書(Book)の永続化を担うリポジトリのインターフェイス
/// Book は集約ルートであり、内包する蔵書(BookStock)・参照する分類(Category)を伴って取得される
/// 実装はインフラストラクチャ層に配置する
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// 書名のキーワードで図書を部分一致検索する
    /// 図書検索API(GET /books?keyword=)で使用する(UC-03)
    /// </summary>
    /// <param name="keyword">書名に対する部分一致検索キーワード</param>
    /// <returns>該当する図書の一覧(0件の場合は空のリスト)</returns>
    Task<IReadOnlyList<Book>> FindByTitleKeywordAsync(string keyword);

    /// <summary>
    /// 識別Id(UUID)で図書を1件取得する
    /// 図書詳細取得API(GET /books/{bookId})、および変更・削除時の対象取得で使用する
    /// </summary>
    /// <param name="bookUuid">図書の識別Id(UUID形式)</param>
    /// <returns>該当する図書。存在しない場合はnull</returns>
    Task<Book?> FindByUuidAsync(string bookUuid);

    /// <summary>
    /// 図書を新規追加する(変更の登録のみ。永続化の確定はUnitOfWorkが行う)
    /// 図書登録API(POST /books)で使用する(UC-04)
    /// </summary>
    /// <param name="book">追加する図書</param>
    Task AddAsync(Book book);

    /// <summary>
    /// 図書を更新する(変更の登録のみ。永続化の確定はUnitOfWorkが行う)
    /// 図書変更API(PUT /books/{bookId})で使用する(UC-05)
    /// </summary>
    /// <param name="book">更新する図書</param>
    Task UpdateAsync(Book book);

    /// <summary>
    /// 図書を削除する(変更の登録のみ。永続化の確定はUnitOfWorkが行う)
    /// 図書削除API(DELETE /books/{bookId})で使用する(UC-06)
    /// </summary>
    /// <param name="book">削除する図書</param>
    Task DeleteAsync(Book book);
}