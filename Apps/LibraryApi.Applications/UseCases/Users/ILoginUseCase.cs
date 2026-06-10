using LibraryApi.Applications.Dtos;
namespace LibraryApi.Applications.UseCases.Users;
/// <summary>
/// ログイン(認証)を行うユースケースのインターフェイス(UC-02)
/// </summary>
public interface ILoginUseCase
{
    /// <summary>
    /// ユーザー名とパスワードで認証し、成功すれば JWT を発行する
    /// </summary>
    /// <param name="input">ログインの入力情報(ユーザー名・パスワード)</param>
    /// <returns>発行されたJWTを含むログイン結果</returns>
    Task<LoginResultDto> ExecuteAsync(LoginDto input);
}