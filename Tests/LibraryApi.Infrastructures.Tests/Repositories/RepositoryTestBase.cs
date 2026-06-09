using LibraryApi.Infrastructure.Contexts;
using LibraryApi.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace LibraryApi.Infrastructures.Tests.Repositories;
/// <summary>
/// リポジトリ統合テストの共通基底クラス
///
/// ・appsettings.Test.json から接続文字列を読み、AddInfrastructure で本番と同じ DI を構成する
/// ・各テストの前に DI スコープを作成し、同一スコープから AppDbContext とリポジトリを解決する
///   (Scoped 登録のため、同一スコープ内では同じ AppDbContext インスタンスが共有される)
/// ・各テストの前にトランザクションを開始し、後でロールバックする
///   これにより、追加・変更・削除を行うテストでも、データベースの状態は元に戻る
///   (演習成果物の library_db をそのまま使い、データを汚さないため)
/// </summary>
public abstract class RepositoryTestBase
{
    private ServiceProvider _provider = null!;
    private IServiceScope _scope = null!;

    // 派生クラス(各リポジトリのテスト)から、解決済みのスコープを使えるようにする
    protected IServiceProvider ScopedServices => _scope.ServiceProvider;

    // リポジトリと同一インスタンスの AppDbContext(トランザクション制御に用いる)
    protected AppDbContext DbContext { get; private set; } = null!;

    [TestInitialize]
    public void BaseInitialize()
    {
        // appsettings.Test.json から接続文字列を読み込む
        // LibraryApi.Infrastructures.Tests.csprojを参照
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json")
            .Build();
        var connectionString = configuration.GetConnectionString("LibraryDb")!;

        // 本番と同じ AddInfrastructure で DI を構成する
        var services = new ServiceCollection();
        services.AddInfrastructure(connectionString);
        _provider = services.BuildServiceProvider();

        // Scoped を解決するためスコープを作成し、同一スコープから DbContext を取得する
        _scope = _provider.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // トランザクションを開始する(テスト後にロールバックして元に戻す)
        DbContext.Database.BeginTransaction();
    }

    [TestCleanup]
    public void BaseCleanup()
    {
        // トランザクションをロールバックし、テスト中の変更を巻き戻す
        DbContext.Database.RollbackTransaction();

        // スコープと ServiceProvider を破棄する
        _scope.Dispose();
        _provider.Dispose();
    }
}