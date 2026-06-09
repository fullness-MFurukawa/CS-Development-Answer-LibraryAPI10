using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Tests.Models;
/// <summary>
/// Categoryエンティティの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Models")]
public class CategoryTests
{
    // ───────────────────────────────────────────
    // Create(新規作成)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "有効な分類名を渡すとインスタンスを生成する")]
    public void Create_TestCase1()
    {
        // Arrange
        var name = "技術書";
        // Act
        var category = Category.Create(name);
        // Assert
        Assert.AreEqual(name, category.Name);
        // 新規作成時は UUID が自動採番される(空でないこと)
        Assert.IsFalse(string.IsNullOrEmpty(category.CategoryUuid));
    }

    [TestMethod(DisplayName = "生成のたびに異なるUUIDが採番される")]
    public void Create_TestCase2()
    {
        // Arrange / Act
        var category1 = Category.Create("技術書");
        var category2 = Category.Create("技術書");

        // Assert
        // 同じ分類名でも、UUID は個別に採番されるため一致しない
        Assert.AreNotEqual(category1.CategoryUuid, category2.CategoryUuid);
    }

    [TestMethod]
    [DataRow("", DisplayName = "分類名が空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "分類名が空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "分類名がnullならDomainExceptionをスローする")]
    public void Create_TestCase3(string? invalidName)
    {
        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Category.Create(invalidName!));
        // 違反項目が name であることまで検証する
        Assert.AreEqual("name", ex.ParamName);
    }

    [TestMethod(DisplayName = "分類名が21文字以上ならDomainExceptionをスローする")]
    public void Create_TestCase4()
    {
        // Arrange
        var tooLongName = new string('あ', 21);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Category.Create(tooLongName));
        Assert.AreEqual("name", ex.ParamName);
    }

    // ───────────────────────────────────────────
    // Restore(復元)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "既存のUUIDと分類名から復元するとUUIDが引き継がれる")]
    public void Restore_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var name = "小説";

        // Act
        var category = Category.Restore(uuid, name);

        // Assert
        // 復元ではUUIDは新規採番されず、渡した値がそのまま引き継がれる
        Assert.AreEqual(uuid, category.CategoryUuid);
        Assert.AreEqual(name, category.Name);
    }

    [TestMethod]
    [DataRow("", DisplayName = "UUIDが空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "UUIDが空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "UUIDがnullならDomainExceptionをスローする")]
    public void Restore_TestCase2(string? invalidUuid)
    {
        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Category.Restore(invalidUuid!, "小説"));
        Assert.AreEqual("categoryUuid", ex.ParamName);
    }

    [TestMethod(DisplayName = "分類名が不正ならDomainExceptionをスローする")]
    public void Restore_TestCase3()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => Category.Restore(uuid, ""));
        Assert.AreEqual("name", ex.ParamName);
    }

    // ───────────────────────────────────────────
    // ChangeName(分類名の変更)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "有効な分類名を渡すと名前が変更される")]
    public void ChangeName_TestCase1()
    {
        // Arrange
        var category = Category.Create("技術書");

        // Act
        category.ChangeName("実用書");

        // Assert
        Assert.AreEqual("実用書", category.Name);
    }

    [TestMethod(DisplayName = "不正な分類名を渡すとDomainExceptionをスローする")]
    public void ChangeName_TestCase2()
    {
        // Arrange
        var category = Category.Create("技術書");

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => category.ChangeName(""));
        Assert.AreEqual("name", ex.ParamName);
    }

    [TestMethod(DisplayName = "変更が失敗しても元の名前は保持される")]
    public void ChangeName_TestCase3()
    {
        // Arrange
        var category = Category.Create("技術書");

        // Act
        try
        {
            category.ChangeName(""); // 失敗する
        }
        catch (DomainException)
        {
            // 握りつぶす(状態が変わっていないことを確認するため)
        }

        // Assert
        // バリデーションは値を代入する前に行われるため、元の名前が保持される
        Assert.AreEqual("技術書", category.Name);
    }
}