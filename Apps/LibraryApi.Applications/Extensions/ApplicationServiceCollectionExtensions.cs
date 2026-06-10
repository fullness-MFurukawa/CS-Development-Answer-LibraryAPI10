using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Authentications;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Services;
using LibraryApi.Applications.Services.Books;
using LibraryApi.Applications.Services.Categories;
using LibraryApi.Applications.Services.Users;
using LibraryApi.Applications.UseCases;
using LibraryApi.Applications.UseCases.Books;
using LibraryApi.Applications.UseCases.Categories;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using Microsoft.Extensions.DependencyInjection;
namespace LibraryApi.Applications.Extensions;
/// <summary>
/// アプリケーション層の構成要素を DI コンテナへ登録する拡張メソッドを提供する
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// アプリケーション層の構成要素(Service・UseCase・認証コンポーネントなど)を登録する
    /// </summary>
    /// <param name="services">DI コンテナ</param>
    /// <param name="jwtSettings">JWT の設定値(プレゼンテーション層が外部設定から組み立てて渡す)</param>
    /// <returns>登録後の DI コンテナ(メソッドチェーン用)</returns>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        JwtSettings jwtSettings)
    {
        // Service(リポジトリに依存するため Scoped)
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBookService, BookService>();

        // UseCase(Service・Adapter に依存するため Scoped)
        services.AddScoped<IFindCategoriesUseCase, FindCategoriesInteractor>();
        services.AddScoped<ISearchBooksUseCase, SearchBooksInteractor>();
        services.AddScoped<IFindBookUseCase, FindBookInteractor>();
        services.AddScoped<IRegisterBookUseCase, RegisterBookInteractor>();
        services.AddScoped<IUpdateBookUseCase, UpdateBookInteractor>();
        services.AddScoped<IDeleteBookUseCase, DeleteBookInteractor>(); // 追加(抜けていた)

        // DTO Adapter(状態を持たない変換ロジックのため Singleton)
        services.AddSingleton<IAdapter<Category, CategoryDto>, CategoryDtoAdapter>();
        services.AddSingleton<IAdapter<Book, BookDto>, BookDtoAdapter>();

        // パスワードのハッシュ化・照合を行うコンポーネント(状態を持たないため Singleton)
        services.AddSingleton<IPasswordService, PasswordService>();

        // JWT の設定値を登録し、JwtTokenProvider に注入できるようにする
        services.AddSingleton(jwtSettings);
        // JWT の発行を行うコンポーネント(状態を持たないため Singleton)
        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
        
        services.AddScoped<IRegisterUserUseCase, RegisterUserInteractor>();
        services.AddSingleton<IAdapter<User, UserDto>, UserDtoAdapter>();

        services.AddScoped<ILoginUseCase, LoginInteractor>();


        return services;
    }
}