using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Services;
/// <summary>
/// 図書に関する操作を提供するサービスのインターフェイス
///
/// リポジトリを利用して図書(集約)のデータ操作を行う。
/// 入出力はドメインエンティティ(Book)で扱う。
/// ユースケースの流れ(UUIDからの取得、分類の取得と図書の構築など)は UseCase が組み立て、
/// 本サービスはリポジトリに対応する個別操作を提供する。
/// トランザクション境界は UseCase が管理するため、本サービスは意識しない。
/// </summary>
public interface IBookService
{
    /// <summary>
    /// 書名のキーワードで図書を部分一致検索する
    /// </summary>
    /// <param name="keyword">書名に対する部分一致検索キーワード</param>
    /// <returns>該当する図書の一覧(0件の場合は空のリスト)</returns>
    Task<IReadOnlyList<Book>> FindByTitleKeywordAsync(string keyword);

    /// <summary>
    /// 識別Id(UUID)で図書を1件取得する
    /// </summary>
    /// <param name="bookUuid">図書の識別Id(UUID形式)</param>
    /// <returns>該当する図書。存在しない場合はnull</returns>
    Task<Book?> FindByUuidAsync(string bookUuid);

    /// <summary>
    /// 図書を新規追加する(図書と蔵書を集約として一体で保存する)
    /// 渡される Book は、参照する分類・内包する蔵書を備えた構築済みの集約である前提
    /// </summary>
    /// <param name="book">追加する図書</param>
    Task AddAsync(Book book);

    /// <summary>
    /// 図書を更新する(書名・著者名・蔵書数)
    /// </summary>
    /// <param name="book">更新内容を反映した図書</param>
    Task UpdateAsync(Book book);

    /// <summary>
    /// 図書を削除する(内包する蔵書も同時に削除される)
    /// </summary>
    /// <param name="book">削除する図書</param>
    Task DeleteAsync(Book book);
}