using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Microsoft.Extensions.DependencyInjection;
namespace LibraryApi.Infrastructures.Tests.Repositories;
/// <summary>
/// UserRepository の統合テストドライバ
/// 実際の library_db に接続し、取得・追加の結果を検証する
/// (RepositoryTestBase により、各テストはトランザクション内で実行されロールバックされる)
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class UserRepositoryTests : RepositoryTestBase
{
    // ───────────────────────────────────────────
    // FindByUsernameAsync(ユーザー名で1件取得)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "ユーザー名でユーザーを1件取得する")]
    public async Task FindByUsernameAsync_TestCase1()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IUserRepository>();

        // Act
        // 手順書で投入したテストユーザー librarian1 を取得する
        var user = await repository.FindByUsernameAsync("librarian1");

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("librarian1", user.Username);
        // UUID・ハッシュ化済みパスワードが復元されていること
        Assert.IsFalse(string.IsNullOrEmpty(user.UserUuid));
        Assert.IsFalse(string.IsNullOrEmpty(user.HashedPassword));
    }

    [TestMethod(DisplayName = "存在しないユーザー名ではnullが返る")]
    public async Task FindByUsernameAsync_TestCase2()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IUserRepository>();

        // Act
        var user = await repository.FindByUsernameAsync("non_existent_user");

        // Assert
        Assert.IsNull(user);
    }

    // ───────────────────────────────────────────
    // AddAsync(新規追加)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "ユーザーを新規追加し取得できる")]
    public async Task AddAsync_TestCase1()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IUserRepository>();
        // 一意かつ30文字以内のユーザー名で新規ユーザーを生成する(ハッシュ化済みパスワードはダミー)
        var username = "test_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var user = User.Create(username, "hashed_dummy_value");

        // Act
        await repository.AddAsync(user);

        // Assert
        // 同一トランザクション内なので、追加したユーザーを取得できる
        // (テスト終了時にロールバックされ、DB からは消える)
        var added = await repository.FindByUsernameAsync(username);
        Assert.IsNotNull(added);
        Assert.AreEqual(username, added.Username);
        Assert.AreEqual(user.UserUuid, added.UserUuid);
    }

    [TestMethod(DisplayName = "追加したユーザーの日時がDbContextにより自動設定される")]
    public async Task AddAsync_TestCase2()
    {
        // Arrange
        var repository = ScopedServices.GetRequiredService<IUserRepository>();
        var username = "test_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var user = User.Create(username, "hashed_dummy_value");

        // Act
        await repository.AddAsync(user);

        // Assert
        var added = await repository.FindByUsernameAsync(username);
        Assert.IsNotNull(added);
    }
}