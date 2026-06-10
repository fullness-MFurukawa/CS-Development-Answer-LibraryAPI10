namespace LibraryApi.Applications.UseCases.UnitOfWorks; 
/// <summary>
/// トランザクションの境界を制御する Unit of Work のインターフェイス
///
/// 個々のデータ操作(SaveChanges を含む)はリポジトリが完結させるが、
/// 複数の操作を「ひとつのまとまり(ユースケース)」として扱うために、
/// UseCase が本インターフェイスでトランザクションを開始・コミット・ロールバックする。
///
/// 実装はインフラストラクチャ層に置き、EF Core の AppDbContext を用いて制御する。
/// (インターフェイスをアプリケーション層に置くことで、依存方向を内向きに保つ)
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// トランザクションを開始する
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// トランザクションをコミットする(一連の変更を確定する)
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// トランザクションをロールバックする(一連の変更を取り消す)
    /// </summary>
    Task RollbackAsync();
}