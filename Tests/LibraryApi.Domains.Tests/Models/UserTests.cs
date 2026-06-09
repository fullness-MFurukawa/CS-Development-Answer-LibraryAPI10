using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Tests.Models;
/// <summary>
/// Userエンティティの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Models")]
public class UserTests
{
    // ───────────────────────────────────────────
    // Create(新規作成)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "有効なユーザー名とハッシュ化済みパスワードを渡すとインスタンスを生成する")]
    public void Create_TestCase1()
    {
        // Arrange
        var username = "library_user";
        var hashedPassword = "hashed_dummy_value";

        // Act
        var user = User.Create(username, hashedPassword);

        // Assert
        Assert.AreEqual(username, user.Username);
        Assert.AreEqual(hashedPassword, user.HashedPassword);
        // 新規作成時は UUID が自動採番される(空でないこと)
        Assert.IsFalse(string.IsNullOrEmpty(user.UserUuid));
    }

    [TestMethod(DisplayName = "生成のたびに異なるUUIDが採番される")]
    public void Create_TestCase2()
    {
        // Arrange / Act
        var user1 = User.Create("user_a", "hashed_a");
        var user2 = User.Create("user_b", "hashed_b");

        // Assert
        // ユーザーごとに UUID は個別に採番されるため一致しない
        Assert.AreNotEqual(user1.UserUuid, user2.UserUuid);
    }

    [TestMethod(DisplayName = "ユーザー名が境界値の30文字なら生成できる")]
    public void Create_TestCase3()
    {
        // Arrange
        var username = new string('a', 30);

        // Act
        var user = User.Create(username, "hashed_dummy_value");

        // Assert
        Assert.AreEqual(username, user.Username);
    }

    [TestMethod]
    [DataRow("", DisplayName = "ユーザー名が空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "ユーザー名が空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "ユーザー名がnullならDomainExceptionをスローする")]
    public void Create_TestCase4(string? invalidUsername)
    {
        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => User.Create(invalidUsername!, "hashed_dummy_value"));
        Assert.AreEqual("username", ex.ParamName);
    }

    [TestMethod(DisplayName = "ユーザー名が31文字以上ならDomainExceptionをスローする")]
    public void Create_TestCase5()
    {
        // Arrange
        var tooLongUsername = new string('a', 31);

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => User.Create(tooLongUsername, "hashed_dummy_value"));
        Assert.AreEqual("username", ex.ParamName);
    }

    [TestMethod]
    [DataRow("", DisplayName = "パスワードが空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "パスワードが空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "パスワードがnullならDomainExceptionをスローする")]
    public void Create_TestCase6(string? invalidPassword)
    {
        // Act / Assert
        // ドメイン層では「ハッシュ化済みパスワードが存在すること(必須)」のみを検証する
        // (平文の長さ制約はアプリケーション層の責務)
        var ex = Assert.ThrowsExactly<DomainException>(
            () => User.Create("library_user", invalidPassword!));
        Assert.AreEqual("hashedPassword", ex.ParamName);
    }

    // ───────────────────────────────────────────
    // Restore(復元)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "既存のUUID・ユーザー名・パスワードから復元するとUUIDが引き継がれる")]
    public void Restore_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var username = "existing_user";
        var hashedPassword = "hashed_dummy_value";

        // Act
        var user = User.Restore(uuid, username, hashedPassword);

        // Assert
        // 復元ではUUIDは新規採番されず、渡した値がそのまま引き継がれる
        Assert.AreEqual(uuid, user.UserUuid);
        Assert.AreEqual(username, user.Username);
        Assert.AreEqual(hashedPassword, user.HashedPassword);
    }

    [TestMethod]
    [DataRow("", DisplayName = "UUIDが空文字ならDomainExceptionをスローする")]
    [DataRow("   ", DisplayName = "UUIDが空白のみならDomainExceptionをスローする")]
    [DataRow(null, DisplayName = "UUIDがnullならDomainExceptionをスローする")]
    public void Restore_TestCase2(string? invalidUuid)
    {
        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => User.Restore(invalidUuid!, "existing_user", "hashed_dummy_value"));
        Assert.AreEqual("userUuid", ex.ParamName);
    }

    [TestMethod(DisplayName = "ユーザー名が不正ならDomainExceptionをスローする")]
    public void Restore_TestCase3()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => User.Restore(uuid, "", "hashed_dummy_value"));
        Assert.AreEqual("username", ex.ParamName);
    }

    [TestMethod(DisplayName = "パスワードが不正ならDomainExceptionをスローする")]
    public void Restore_TestCase4()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();

        // Act / Assert
        var ex = Assert.ThrowsExactly<DomainException>(
            () => User.Restore(uuid, "existing_user", ""));
        Assert.AreEqual("hashedPassword", ex.ParamName);
    }
}