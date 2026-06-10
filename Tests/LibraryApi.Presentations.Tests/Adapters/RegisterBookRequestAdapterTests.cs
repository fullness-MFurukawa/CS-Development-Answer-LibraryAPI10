using LibraryApi.Applications.Dtos;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Tests.Adapters;
[TestClass]
[TestCategory("Adapters")]
public class RegisterBookRequestAdapterTests
{
    [TestMethod(DisplayName = "Restore:RegisterBookRequestをRegisterBookDtoに変換する")]
    public void Restore_TestCase01()
    {
        var adapter = new RegisterBookRequestAdapter();
        var request = new RegisterBookRequest
        {
            Title = "テスト駆動開発",
            Author = "Kent Beck",
            CategoryId = "category-uuid-001",
            Stock = 5,
        };

        var dto = adapter.Restore(request);

        Assert.AreEqual("テスト駆動開発", dto.Title);
        Assert.AreEqual("Kent Beck", dto.Author);
        Assert.AreEqual("category-uuid-001", dto.CategoryId);
        Assert.AreEqual(5, dto.Stock);
    }

    [TestMethod(DisplayName = "Convert:サポートしないためNotSupportedExceptionとなる")]
    public void Convert_TestCase01()
    {
        var adapter = new RegisterBookRequestAdapter();
        Assert.ThrowsExactly<NotSupportedException>(
            () => adapter.Convert(new RegisterBookDto()));
    }
}