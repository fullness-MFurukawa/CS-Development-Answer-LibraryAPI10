using LibraryApi.Applications.Services.Users;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Moq;
namespace LibraryApi.Applications.Tests.Services;
/// <summary>
/// UserService の単体テスト
///
/// 依存する IUserRepository をモックに差し替え、Service がリポジトリへ
/// 正しく委譲し、その結果をそのまま返すことを検証する。
/// </summary>
[TestClass]
[TestCategory("Services")]
public class UserServiceTests
{
    [TestMethod(DisplayName = "ユーザー名でユーザーを取得し結果をそのまま返す")]
    public async Task FindByUsernameAsync_TestCase01()
    {
        // Arrange:特定のユーザー名でユーザーが返るよう設定する
        var user = User.Restore("user-uuid-1", "yamada_taro", "hashed-password");

        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(r => r.FindByUsernameAsync("yamada_taro"))
            .ReturnsAsync(user);

        var service = new UserService(repositoryMock.Object);

        // Act
        var result = await service.FindByUsernameAsync("yamada_taro");

        // Assert:リポジトリの結果がそのまま返ること
        Assert.IsNotNull(result);
        Assert.AreEqual("user-uuid-1", result.UserUuid);
        Assert.AreEqual("yamada_taro", result.Username);

        // 正しい引数でリポジトリが呼ばれたことを確認する
        repositoryMock.Verify(r => r.FindByUsernameAsync("yamada_taro"), Times.Once);
    }

    [TestMethod(DisplayName = "該当なしの場合はnullを返す")]
    public async Task FindByUsernameAsync_TestCase02()
    {
        // Arrange:リポジトリが null を返すよう設定する
        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(r => r.FindByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var service = new UserService(repositoryMock.Object);

        // Act
        var result = await service.FindByUsernameAsync("not-exist");

        // Assert:null がそのまま返ること
        Assert.IsNull(result);
    }

    [TestMethod(DisplayName = "ユーザーをリポジトリへ渡して追加する")]
    public async Task AddAsync_TestCase01()
    {
        // Arrange
        var user = User.Restore("user-uuid-1", "yamada_taro", "hashed-password");

        var repositoryMock = new Mock<IUserRepository>();
        repositoryMock
            .Setup(r => r.AddAsync(user))
            .Returns(Task.CompletedTask);

        var service = new UserService(repositoryMock.Object);

        // Act
        await service.AddAsync(user);

        // Assert:同じユーザーを引数に、リポジトリの AddAsync が1回呼ばれたこと
        repositoryMock.Verify(r => r.AddAsync(user), Times.Once);
    }
}