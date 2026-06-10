using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.UseCases.Users;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace LibraryApi.Presentations.Controllers;
/// <summary>
/// 認証(ログイン・ログアウト)に関する API を提供する
/// </summary>
[ApiController]
[Route("library/api/auth")]
[Tags("認証")]
public class AuthController : ControllerBase
{
    /// <summary>認証トークンを格納する Cookie のキー名</summary>
    private const string AuthCookieName = "access_token";

    private readonly ILoginUseCase _loginUseCase;
    private readonly IAdapter<LoginDto, LoginRequest> _loginRequestAdapter;

    public AuthController(
        ILoginUseCase loginUseCase,
        IAdapter<LoginDto, LoginRequest> loginRequestAdapter)
    {
        _loginUseCase = loginUseCase;
        _loginRequestAdapter = loginRequestAdapter;
    }

    /// <summary>
    /// ログインする
    /// POST /library/api/auth/login
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var input = _loginRequestAdapter.Restore(request);

        var result = await _loginUseCase.ExecuteAsync(input);

        // 発行された JWT を HttpOnly Cookie にセットする
        Response.Cookies.Append(AuthCookieName, result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,                  // ★開発はHTTPのため false
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(60),
        });

        return Ok(new LoginResponse { Message = "ログインに成功しました。" });
    }


    /// <summary>
    /// ログアウトする
    /// POST /library/api/auth/logout
    /// </summary>
    /// <returns>成功メッセージ(200 OK)</returns>
    [HttpPost("logout")]
    [Authorize]   // ログアウトは認証が必要(ログイン中のユーザーが対象)
    public ActionResult<LogoutResponse> Logout()
    {
        // 認証トークンの Cookie を削除する(以降のリクエストで送られなくなる)
        Response.Cookies.Delete(AuthCookieName);

        return Ok(new LogoutResponse { Message = "ログアウトしました。" });
    }
}