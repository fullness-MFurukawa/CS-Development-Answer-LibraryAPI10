using LibraryApi.Applications.Authentications;
namespace LibraryApi.Applications.Tests.Authentications;
/// <summary>
/// PasswordService の単体テスト
///
/// パスワードのハッシュ化・照合を検証する。
/// ハッシュ化ロジックそのものが対象のため、モックは用いない。
/// </summary>
[TestClass]
[TestCategory("Authentications")]
public class PasswordServiceTests
{
    [TestMethod(DisplayName = "ハッシュ化したパスワードは元のパスワードと照合できる")]
    public void HashAndVerify_TestCase01()
    {
        // Arrange
        var service = new PasswordService();
        var password = "P@ssw0rd123";

        // Act:ハッシュ化し、同じパスワードで照合する
        var hashed = service.Hash(password);
        var result = service.Verify(hashed, password);

        // Assert:照合が成功すること
        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "誤ったパスワードは照合に失敗する")]
    public void HashAndVerify_TestCase02()
    {
        // Arrange
        var service = new PasswordService();
        var hashed = service.Hash("P@ssw0rd123");

        // Act:異なるパスワードで照合する
        var result = service.Verify(hashed, "WrongPassword");

        // Assert:照合が失敗すること
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "ハッシュ化結果は元のパスワードと異なる文字列になる")]
    public void Hash_TestCase01()
    {
        // Arrange
        var service = new PasswordService();
        var password = "P@ssw0rd123";

        // Act
        var hashed = service.Hash(password);

        // Assert:平文がそのまま保存されていないこと
        Assert.AreNotEqual(password, hashed);
        Assert.IsFalse(string.IsNullOrEmpty(hashed));
    }

    [TestMethod(DisplayName = "同じパスワードでもハッシュ化のたびに異なる結果になる(ソルト)")]
    public void Hash_TestCase02()
    {
        // Arrange
        var service = new PasswordService();
        var password = "P@ssw0rd123";

        // Act:同じパスワードを2回ハッシュ化する
        var hashed1 = service.Hash(password);
        var hashed2 = service.Hash(password);

        // Assert:ソルトにより異なるハッシュになるが、どちらも照合は成功する
        Assert.AreNotEqual(hashed1, hashed2);
        Assert.IsTrue(service.Verify(hashed1, password));
        Assert.IsTrue(service.Verify(hashed2, password));
    }
}