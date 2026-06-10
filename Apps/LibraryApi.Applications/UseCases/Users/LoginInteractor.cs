using LibraryApi.Applications.Authentications;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services.Users;
namespace LibraryApi.Applications.UseCases.Users;
/// <summary>
/// ILoginUseCase の実装(Interactor)
///
/// ユーザー名でユーザーを検索し、パスワードを照合する。
/// 認証に成功すれば JWT を発行して返す。
/// 認証に失敗する場合(ユーザーが存在しない、またはパスワードが一致しない)は、
/// それらを区別せず、同一の認証失敗例外をスローする(列挙攻撃を防ぐため:UC-02 BR-04)。
/// 読み取りのみのため、トランザクションは用いない。
/// </summary>
public class LoginInteractor : ILoginUseCase
{
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public LoginInteractor(
        IUserService userService,
        IPasswordService passwordService,
        IJwtTokenProvider jwtTokenProvider)
    {
        _userService = userService;
        _passwordService = passwordService;
        _jwtTokenProvider = jwtTokenProvider;
    }

    /// <summary>
    /// ユーザー名とパスワードで認証し、成功すれば JWT を発行する
    /// </summary>
    /// <param name="input">ログインの入力情報</param>
    /// <returns>発行された JWT を含むログイン結果</returns>
    /// <exception cref="AuthenticationFailedException">認証に失敗した場合</exception>
    public async Task<LoginResultDto> ExecuteAsync(LoginDto input)
    {
        // ユーザー名でユーザーを検索する
        var user = await _userService.FindByUsernameAsync(input.Username);

        // ユーザーが存在しない場合も、パスワードが一致しない場合も、
        // 区別せずに同一の認証失敗として扱う(列挙攻撃を防ぐ:UC-02 BR-04)
        if (user is null)
        {
            throw new AuthenticationException(
                "AuthenticationFailed", "ユーザー名またはパスワードが正しくありません。");
        }

        // LoginInteractor の Verify 呼び出しを修正
        var isValid = _passwordService.Verify(user.HashedPassword, input.Password);
        if (!isValid)
        {
            throw new AuthenticationException(
                "AuthenticationFailed", "ユーザー名またはパスワードが正しくありません。");
        }

        // 認証成功 → JWT を発行する
        var accessToken = _jwtTokenProvider.IssueAccessToken(user);

        return new LoginResultDto
        {
            AccessToken = accessToken,
        };
    }
}