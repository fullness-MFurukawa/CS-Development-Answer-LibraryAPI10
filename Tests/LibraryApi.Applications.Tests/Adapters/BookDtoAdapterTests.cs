using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Tests.Adapters;
/// <summary>
/// BookDtoAdapter の単体テスト
///
/// 集約 Book を DTO BookDto に変換することを検証する。
/// 分類部分の変換は実物の CategoryDtoAdapter に委譲して、通しで確認する。
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class BookDtoAdapterTests
{
    [TestMethod(DisplayName = "Convert:集約をDTOに変換する(分類・蔵書数も含む)")]
    public void Convert_TestCase01()
    {
        // Arrange:テスト用の Book(集約)を構築する
        // ※ BookStock.Restore / Book.Restore は BookServiceTests と同じ要領で
        var category = Category.Restore("category-uuid-1", "技術書");
        var bookStock = BookStock.Restore("stock-uuid-1", 5);
        var book = Book.Restore("book-uuid-1", "ドメイン駆動設計入門", "山田太郎", category, bookStock);

        // 分類変換は実物の CategoryDtoAdapter を注入する
        IAdapter<Category, CategoryDto> categoryDtoAdapter = new CategoryDtoAdapter();
        var adapter = new BookDtoAdapter(categoryDtoAdapter);

        // Act
        var dto = adapter.Convert(book);

        // Assert:Book 自身のプロパティ
        Assert.AreEqual("book-uuid-1", dto.BookId);
        Assert.AreEqual("ドメイン駆動設計入門", dto.Title);
        Assert.AreEqual("山田太郎", dto.Author);
        // 蔵書数(内包する BookStock から)
        Assert.AreEqual(5, dto.Stock);
        // 分類(委譲された変換結果)
        Assert.IsNotNull(dto.Category);
        Assert.AreEqual("category-uuid-1", dto.Category.CategoryUuid);
        Assert.AreEqual("技術書", dto.Category.Name);
    }

    [TestMethod(DisplayName = "Restore:現状サポートしておらず例外をスローする")]
    public void Restore_TestCase01()
    {
        // Arrange
        IAdapter<Category, CategoryDto> categoryDtoAdapter = new CategoryDtoAdapter();
        var adapter = new BookDtoAdapter(categoryDtoAdapter);
        var dto = new BookDto();

        // Act & Assert
        Assert.ThrowsExactly<NotSupportedException>(() => adapter.Restore(dto));
    }
}