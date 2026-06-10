using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.UseCases.Users;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.Controllers;
using LibraryApi.Presentations.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LibraryApi.Presentations.Tests.Controllers;
/// <summary>
/// AuthController の単体テスト
///
/// ログイン:認証成功時に 200 とメッセージを返し、access_token Cookie をセットすること。
///          認証失敗例外を素通しすること。
/// ログアウト:200 とメッセージを返し、access_token Cookie を削除すること。
/// ※ Cookie 操作のため、HttpContext(DefaultHttpContext)を用意する。
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class AuthControllerTests
{
    private static AuthController CreateController(Mock<ILoginUseCase> loginMock)
    {
        var controller = new AuthController(loginMock.Object, new LoginRequestAdapter());

        // Response.Cookies を使えるよう、HttpContext を用意する
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [TestMethod(DisplayName = "ログイン:成功時200 OKでメッセージを返しCookieをセットする")]
    public async Task Login_TestCase01()
    {
        var loginMock = new Mock<ILoginUseCase>();
        loginMock
            .Setup(u => u.ExecuteAsync(It.IsAny<LoginDto>()))
            .ReturnsAsync(new LoginResultDto { AccessToken = "issued-token" });

        var controller = CreateController(loginMock);
        var request = new LoginRequest { Username = "test_user", Password = "password" };

        var actionResult = await controller.Login(request);

        // 200 OK であること
        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        // メッセージが返ること
        var response = okResult.Value as LoginResponse;
        Assert.IsNotNull(response);
        Assert.IsFalse(string.IsNullOrEmpty(response.Message));

        // Set-Cookie ヘッダに access_token がセットされていること
        var setCookie = controller.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("access_token", setCookie);
        Assert.Contains("issued-token", setCookie);
    }

    [TestMethod(DisplayName = "ログイン:認証失敗例外を素通しする")]
    public async Task Login_TestCase02()
    {
        var loginMock = new Mock<ILoginUseCase>();
        loginMock
            .Setup(u => u.ExecuteAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new AuthenticationException(
                "AuthenticationFailed", "ユーザー名またはパスワードが正しくありません。"));

        var controller = CreateController(loginMock);
        var request = new LoginRequest { Username = "x", Password = "y" };

        await Assert.ThrowsExactlyAsync<AuthenticationException>(
            () => controller.Login(request));
    }

    [TestMethod(DisplayName = "ログアウト:200 OKでメッセージを返しCookieを削除する")]
    public void Logout_TestCase01()
    {
        // ログアウトは UseCase を使わないが、コンストラクタには必要なのでモックを渡す
        var loginMock = new Mock<ILoginUseCase>();
        var controller = CreateController(loginMock);

        var actionResult = controller.Logout();

        // 200 OK であること
        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var response = okResult.Value as LogoutResponse;
        Assert.IsNotNull(response);
        Assert.IsFalse(string.IsNullOrEmpty(response.Message));

        // Cookie 削除のため、Set-Cookie ヘッダに access_token が現れること
        // (削除は、有効期限を過去にする Set-Cookie として表現される)
        var setCookie = controller.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("access_token", setCookie);
    }
}