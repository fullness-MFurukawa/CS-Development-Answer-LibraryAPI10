using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Aggregators;
using LibraryApi.Infrastructure.Adapters;
using LibraryApi.Infrastructure.Aggregators;
using LibraryApi.Infrastructure.Contexts;
using LibraryApi.Infrastructure.Entities;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Infrastructure.Repositories;
using LibraryApi.Applications.UseCases.UnitOfWorks;
using LibraryApi.Infrastructures.UnitOfWorks;
namespace LibraryApi.Infrastructure.Extensions;
/// <summary>
/// インフラストラクチャ層の構成要素を DI コンテナへ登録する拡張メソッドを提供する
///
/// インフラ層の登録に関する知識を本クラスに閉じ込めることで、
/// プレゼンテーション層は AddInfrastructure を一度呼ぶだけでよく、
/// インフラ層の内部構造(どんなリポジトリ・Adapter があるか)を知らずに済む。
/// 接続文字列は外部(プレゼンテーション層)から受け取り、依存方向を内向きに保つ。
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// インフラストラクチャ層の構成要素(DbContext・Adapter・Aggregator・リポジトリ)を登録する
    /// </summary>
    /// <param name="services">DI コンテナ</param>
    /// <param name="connectionString">PostgreSQL への接続文字列</param>
    /// <returns>登録後の DI コンテナ(メソッドチェーン用)</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        // ───────────────────────────────────────────
        // DbContext(PostgreSQL / Npgsql)
        // AddDbContext は既定で Scoped(1リクエストにつき1インスタンス)
        // リポジトリと UnitOfWork が同一インスタンスを共有できるようにするため Scoped が必須
        // ───────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // ───────────────────────────────────────────
        // Adapter(単一エンティティの相互変換)
        // 状態を持たない純粋な変換ロジックのため Singleton で共有する
        // ───────────────────────────────────────────
        services.AddSingleton<IAdapter<Category, CategoryEntity>, CategoryAdapter>();
        services.AddSingleton<IAdapter<User, UserEntity>, UserAdapter>();
        services.AddSingleton<IAdapter<BookStock, BookStockEntity>, BookStockAdapter>();

        // ───────────────────────────────────────────
        // Aggregator(集約の構築)
        // Adapter を受け取るのみで状態を持たないため Singleton で共有する
        // ───────────────────────────────────────────
        services.AddSingleton<IAggregator<BookEntity, Book>, BookAggregator>();

        // ───────────────────────────────────────────
        // Repository(永続化)
        // AppDbContext(Scoped)に依存するため Scoped で登録する
        // ───────────────────────────────────────────
        
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // ───────────────────────────────────────────
        // UnitOfWork(トランザクション境界の制御)
        // AppDbContext(Scoped)を用いるため Scoped で登録する。
        // インターフェイス IUnitOfWork はアプリケーション層、実装 UnitOfWork はインフラ層にあり、
        // 実装が属する本層で登録する(依存性逆転:アプリケーション層は実装を知らない)。
        // ───────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}