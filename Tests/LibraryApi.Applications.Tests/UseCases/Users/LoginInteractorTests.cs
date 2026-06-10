using LibraryApi.Applications.Authentications;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services.Users;
using LibraryApi.Applications.UseCases.Users;
using LibraryApi.Domains.Models;
using Moq;
namespace LibraryApi.Applications.Tests.UseCases.Users;
/// <summary>
/// LoginInteractor の単体テスト
///
/// ・認証成功時:JWT を発行して返す
/// ・ユーザーが存在しない場合:認証失敗例外(JWT 発行しない)
/// ・パスワードが一致しない場合:認証失敗例外(JWT 発行しない)
///   ※ ユーザー不在とパスワード不一致は区別しない(UC-02 BR-04)
/// </summary>
[TestClass]
[TestCategory("UseCases.Users")]
public class LoginInteractorTests
{
    [TestMethod(DisplayName = "認証成功時:JWTを発行して返す")]
    public async Task ExecuteAsync_TestCase01()
    {
        // Arrange:ユーザーが存在し、パスワードが一致する
        var user = User.Restore("user-uuid", "yamada_taro", "hashed-password");

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.FindByUsernameAsync("yamada_taro"))
            .ReturnsAsync(user);

        // Arrange の中(passwordServiceMock の設定)
        var passwordServiceMock = new Mock<IPasswordService>();
        passwordServiceMock
            .Setup(p => p.Verify("hashed-password", "plain-password"))  // ハッシュが先、平文が後
            .Returns(true);

        var jwtProviderMock = new Mock<IJwtTokenProvider>();
        jwtProviderMock
            .Setup(j => j.IssueAccessToken(user, null))
            .Returns("issued-jwt-token");

        var input = new LoginDto { Username = "yamada_taro", Password = "plain-password" };

        var interactor = new LoginInteractor(
            userServiceMock.Object,
            passwordServiceMock.Object,
            jwtProviderMock.Object);

        // Act
        var result = await interactor.ExecuteAsync(input);

        // Assert
        Assert.AreEqual("issued-jwt-token", result.AccessToken);
        jwtProviderMock.Verify(
            j => j.IssueAccessToken(user, It.IsAny<IEnumerable<System.Security.Claims.Claim>?>()),
            Times.Once);
    }

    [TestMethod(DisplayName = "ユーザーが存在しない場合:認証失敗例外をスローしJWTを発行しない")]
    public async Task ExecuteAsync_TestCase02()
    {
        // Arrange:ユーザーが存在しない
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.FindByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var passwordServiceMock = new Mock<IPasswordService>();
        var jwtProviderMock = new Mock<IJwtTokenProvider>();

        var input = new LoginDto { Username = "unknown", Password = "plain-password" };

        var interactor = new LoginInteractor(
            userServiceMock.Object,
            passwordServiceMock.Object,
            jwtProviderMock.Object);

        // Act & Assert(例外の型は、実際に作った認証失敗例外の名前に合わせる)
        await Assert.ThrowsExactlyAsync<AuthenticationException>(
            () => interactor.ExecuteAsync(input));

        // JWT は発行されない
        jwtProviderMock.Verify(
            j => j.IssueAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<System.Security.Claims.Claim>?>()),
            Times.Never);
    }

    [TestMethod(DisplayName = "パスワードが一致しない場合:認証失敗例外をスローしJWTを発行しない")]
    public async Task ExecuteAsync_TestCase03()
    {
        // Arrange:ユーザーは存在するが、パスワードが一致しない
        var user = User.Restore("user-uuid", "yamada_taro", "hashed-password");

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.FindByUsernameAsync("yamada_taro"))
            .ReturnsAsync(user);

        var passwordServiceMock = new Mock<IPasswordService>();
        passwordServiceMock
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var jwtProviderMock = new Mock<IJwtTokenProvider>();

        var input = new LoginDto { Username = "yamada_taro", Password = "wrong-password" };

        var interactor = new LoginInteractor(
            userServiceMock.Object,
            passwordServiceMock.Object,
            jwtProviderMock.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<AuthenticationException>(
            () => interactor.ExecuteAsync(input));

        jwtProviderMock.Verify(
            j => j.IssueAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<System.Security.Claims.Claim>?>()),
            Times.Never);
    }
}