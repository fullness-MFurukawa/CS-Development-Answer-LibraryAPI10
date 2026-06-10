using LibraryApi.Applications.Dtos;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Extensions;
/// <summary>
/// プレゼンテーション層の構成要素を DI コンテナへ登録する拡張メソッドを提供する
///
/// プレゼンテーション層固有の登録(DTO ⇄ ViewModel を変換する Adapter など)を
/// 本クラスに閉じ込め、Program.cs は AddPresentation を一度呼ぶだけでよいようにする。
/// </summary>
public static class PresentationServiceCollectionExtensions
{
    /// <summary>
    /// プレゼンテーション層の構成要素(ViewModel 変換 Adapter など)を登録する
    /// </summary>
    /// <param name="services">DI コンテナ</param>
    /// <returns>登録後の DI コンテナ(メソッドチェーン用)</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // ViewModel 変換 Adapter(状態を持たない変換ロジックのため Singleton)
        services.AddSingleton<IAdapter<CategoryDto, CategoryResponse>, CategoryResponseAdapter>();
        services.AddSingleton<IAdapter<BookDto, BookResponse>, BookResponseAdapter>();
        services.AddSingleton<IAdapter<RegisterBookDto, RegisterBookRequest>, RegisterBookRequestAdapter>();
        services.AddSingleton<IAdapter<UpdateBookDto, UpdateBookRequest>, UpdateBookRequestAdapter>();
  
        services.AddSingleton<IAdapter<RegisterUserDto, RegisterUserRequest>, RegisterUserRequestAdapter>();
        services.AddSingleton<IAdapter<UserDto, UserResponse>, UserResponseAdapter>();

        services.AddSingleton<IAdapter<LoginDto, LoginRequest>, LoginRequestAdapter>();


        return services;
    }
}