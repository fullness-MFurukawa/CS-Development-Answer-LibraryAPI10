using LibraryApi.Applications.Dtos;
namespace LibraryApi.Applications.UseCases.Users;
/// <summary>
/// ユーザーを登録するユースケースのインターフェイス(UC-01)
/// </summary>
public interface IRegisterUserUseCase
{
    /// <summary>
    /// 新しいユーザーを登録する
    /// </summary>
    /// <param name="input">ユーザー登録の入力情報(ユーザー名・パスワード)</param>
    /// <returns>登録されたユーザーの情報(識別Id・ユーザー名)</returns>
    Task<UserDto> ExecuteAsync(RegisterUserDto input);
}