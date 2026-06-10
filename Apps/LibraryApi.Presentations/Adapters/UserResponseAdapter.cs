using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// UserDto と UserResponse(ViewModel)を変換する Adapter
///
/// ・Convert : DTO → ViewModel(レスポンスを組み立てる際に使用)
/// ・Restore : ViewModel → DTO。出力専用のため、現状サポートしない。
/// </summary>
public class UserResponseAdapter : IAdapter<UserDto, UserResponse>
{
    /// <summary>
    /// DTO を ViewModel に変換する(DTO → ViewModel)
    /// </summary>
    /// <param name="source">アプリケーション層の UserDto</param>
    /// <returns>プレゼンテーション層の UserResponse</returns>
    public UserResponse Convert(UserDto source)
    {
        return new UserResponse
        {
            UserId = source.UserId,
            Username = source.Username,
        };
    }

    /// <summary>
    /// ViewModel を DTO に変換する(ViewModel → DTO)。出力専用のため未サポート。
    /// </summary>
    /// <exception cref="NotSupportedException">本変換はサポートしていない</exception>
    public UserDto Restore(UserResponse source)
    {
        throw new NotSupportedException(
            "UserResponse から UserDto への変換はサポートしていません。");
    }
}