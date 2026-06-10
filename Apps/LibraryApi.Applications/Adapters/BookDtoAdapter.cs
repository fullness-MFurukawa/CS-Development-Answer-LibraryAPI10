using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Adapters;
/// <summary>
/// ドメインの集約 Book と、DTO である BookDto を変換する Adapter
///
/// ・Convert : ドメイン → DTO(検索・登録・変更の結果を返す際に使用)
///   集約 Book から値を取り出して平らな BookDto に展開する。
///   参照する分類(Category)の変換は、CategoryDtoAdapter に委譲する。
/// ・Restore : DTO → ドメイン。レスポンス用 BookDto からの集約復元は行わない
///   (図書登録・変更では、入力用のリクエストから集約を構築するため、本変換は使用しない)。
/// </summary>
public class BookDtoAdapter : IAdapter<Book, BookDto>
{
    private readonly IAdapter<Category, CategoryDto> _categoryDtoAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categoryDtoAdapter">分類の DTO 変換を担う Adapter</param>
    public BookDtoAdapter(IAdapter<Category, CategoryDto> categoryDtoAdapter)
    {
        _categoryDtoAdapter = categoryDtoAdapter;
    }

    /// <summary>
    /// ドメインの集約を DTO に変換する(検索系:ドメイン → DTO)
    /// </summary>
    /// <param name="source">ドメインの集約 Book</param>
    /// <returns>DTO である BookDto</returns>
    public BookDto Convert(Book source)
    {
        return new BookDto
        {
            BookId = source.BookUuid,
            Title = source.Title,
            Author = source.Author,
            // 参照する分類は、CategoryDtoAdapter に変換を委譲する
            Category = _categoryDtoAdapter.Convert(source.Category),
            // 内包する蔵書から蔵書数を取り出す
            Stock = source.BookStock.Stock,
        };
    }

    /// <summary>
    /// DTO をドメインに復元する(本 Adapter では未サポート)
    ///
    /// レスポンス用 BookDto から集約 Book を復元する場面は存在しない。
    /// (図書登録・変更では、入力用のリクエストから集約を構築するため、必要になった際は
    ///  そのユースケースに適した形で実装すること)
    /// </summary>
    /// <exception cref="NotSupportedException">本変換はサポートしていない</exception>
    public Book Restore(BookDto source)
    {
        throw new NotSupportedException(
            "BookDtoからドメインへの変換はサポートしていません。" +
            "図書の登録・変更では、入力用リクエストから集約を構築してください。");
    }
}