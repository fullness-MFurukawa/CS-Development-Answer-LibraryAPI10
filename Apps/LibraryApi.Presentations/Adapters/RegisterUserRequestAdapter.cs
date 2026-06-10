using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// RegisterUserDto と RegisterUserRequest(ViewModel)を変換する Adapter
///
/// ・Restore : ViewModel → DTO(リクエストを受け取り、UseCase へ渡す DTO を組み立てる際に使用)
/// ・Convert : DTO → ViewModel。入力専用のため、現状サポートしない。
/// </summary>
public class RegisterUserRequestAdapter : IAdapter<RegisterUserDto, RegisterUserRequest>
{
    /// <summary>
    /// DTO を ViewModel に変換する(DTO → ViewModel)。入力専用のため未サポート。
    /// </summary>
    /// <exception cref="NotSupportedException">本変換はサポートしていない</exception>
    public RegisterUserRequest Convert(RegisterUserDto source)
    {
        throw new NotSupportedException(
            "RegisterUserDto から RegisterUserRequest への変換はサポートしていません。");
    }

    /// <summary>
    /// ViewModel を DTO に変換する(ViewModel → DTO)
    /// </summary>
    /// <param name="source">プレゼンテーション層の RegisterUserRequest</param>
    /// <returns>アプリケーション層の RegisterUserDto</returns>
    public RegisterUserDto Restore(RegisterUserRequest source)
    {
        return new RegisterUserDto
        {
            Username = source.Username,
            Password = source.Password,
        };
    }
}