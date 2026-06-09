using LibraryApi.Infrastructure.Adapters;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
namespace LibraryApi.Infrastructure.Tests.Adapters;
/// <summary>
/// UserAdapter の単体テストドライバ
/// User(ドメイン)と UserEntity(EF Core)の相互変換を検証する
/// 特に、ドメインの HashedPassword と EF Core の Password の対応づけを確認する
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class UserAdapterTests
{
    private readonly UserAdapter _adapter = new();

    // ───────────────────────────────────────────
    // Convert(ドメイン → EF Core エンティティ)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "ドメインのUserをUserEntityに変換する")]
    public void Convert_TestCase1()
    {
        // Arrange
        var user = User.Create("librarian1", "hashed_dummy_value");

        // Act
        var entity = _adapter.Convert(user);

        // Assert
        Assert.AreEqual(user.UserUuid, entity.UserUuid);
        Assert.AreEqual("librarian1", entity.Username);
        // ドメインの HashedPassword が、EF Core の Password へ移されること
        Assert.AreEqual("hashed_dummy_value", entity.Password);
    }

    [TestMethod(DisplayName = "変換時にIdと日時は設定されない")]
    public void Convert_TestCase2()
    {
        // Arrange
        var user = User.Create("librarian1", "hashed_dummy_value");

        // Act
        var entity = _adapter.Convert(user);

        // Assert
        Assert.AreEqual(0, entity.Id);
        Assert.AreEqual(default, entity.CreatedAt);
        Assert.AreEqual(default, entity.UpdatedAt);
    }

    // ───────────────────────────────────────────
    // Restore(EF Core エンティティ → ドメイン)
    // ───────────────────────────────────────────

    [TestMethod(DisplayName = "UserEntityをドメインのUserに復元する")]
    public void Restore_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var entity = new UserEntity
        {
            Id = 1,
            UserUuid = uuid,
            Username = "user1",
            Password = "hashed_dummy_value",
        };

        // Act
        var user = _adapter.Restore(entity);

        // Assert
        Assert.AreEqual(uuid, user.UserUuid);
        Assert.AreEqual("user1", user.Username);
        // EF Core の Password が、ドメインの HashedPassword へ移されること
        Assert.AreEqual("hashed_dummy_value", user.HashedPassword);
    }

    [TestMethod(DisplayName = "復元時にドメインの検証が働きUUIDが空ならDomainExceptionをスローする")]
    public void Restore_TestCase2()
    {
        // Arrange
        var entity = new UserEntity
        {
            UserUuid = "",
            Username = "user1",
            Password = "hashed_dummy_value",
        };

        // Act / Assert
        Assert.ThrowsExactly<DomainException>(() => _adapter.Restore(entity));
    }
}