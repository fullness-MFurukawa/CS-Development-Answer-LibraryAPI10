using LibraryApi.Applications.UseCases.UnitOfWorks;
using LibraryApi.Infrastructure.Contexts;
namespace LibraryApi.Infrastructures.UnitOfWorks;
/// <summary>
/// IUnitOfWork の実装
/// EF Core の AppDbContext を用いてトランザクションを制御する
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">DbContext継承クラス</param> 
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// トランザクションを開始する
    /// </summary
    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// トランザクションをコミットする(一連の変更を確定する)
    /// </summary>
    public async Task CommitAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    /// <summary>
    /// トランザクションをロールバックする(一連の変更を取り消す)
    /// </summary>
    public async Task RollbackAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }
}