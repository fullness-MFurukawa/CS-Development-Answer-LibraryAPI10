using LibraryApi.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
namespace LibraryApi.Infrastructure.Contexts;
/// <summary>
/// 図書管理システムの EF Core データベースコンテキスト
///
/// ・各テーブルに対応する DbSet を公開する
/// ・属性で表現できないマッピング(UNIQUE 制約・リレーション・チェック制約)を
///   OnModelCreating の Fluent API で定義する
/// ・日時(CreatedAt / UpdatedAt)の自動設定を SaveChangesAsync のオーバーライドで行う
///
/// 接続文字列は外部(プレゼンテーション層の DI 設定)から DbContextOptions 経由で受け取り、
/// 本クラスは接続文字列を自分で読み込まない(依存方向を内向きに保つため)
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// 分類テーブル
    /// </summary>
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    /// <summary>
    /// 図書テーブル
    /// </summary>
    public DbSet<BookEntity> Books => Set<BookEntity>();
    /// <summary>
    /// 蔵書テーブル
    /// </summary>
    public DbSet<BookStockEntity> BookStocks => Set<BookStockEntity>();

    /// <summary>
    /// ユーザーテーブル
    /// </summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>
    /// コンストラクタ
    /// 接続設定(UseNpgsql など)は DI 登録時に行い、その設定を options 経由で受け取る
    /// </summary>
    /// <param name="options">DbContext の構成オプション</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// モデルのマッピングを定義する
    /// 属性で表現できない設定(UNIQUE 制約・リレーション・チェック制約)をここで補う
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ───────────────────────────────────────────
        // category(分類)
        // ───────────────────────────────────────────
        modelBuilder.Entity<CategoryEntity>(entity =>
        {
            // 識別Id(category_uuid)は一意
            entity.HasIndex(e => e.CategoryUuid).IsUnique();
        });

        // ───────────────────────────────────────────
        // book(図書)
        // ───────────────────────────────────────────
        modelBuilder.Entity<BookEntity>(entity =>
        {
            // 識別Id(book_uuid)は一意
            entity.HasIndex(e => e.BookUuid).IsUnique();

            // 分類への多対1リレーション
            // 多くの図書(Book)が、1つの分類(Category)に属する
            // 外部キーは book.category_id。逆参照は持たないため、相手側のナビゲーションは指定しない
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // 分類削除時に図書を巻き込まない
        });

        // ───────────────────────────────────────────
        // book_stock(蔵書)
        // ───────────────────────────────────────────
        modelBuilder.Entity<BookStockEntity>(entity =>
        {
            // 識別Id(stock_uuid)は一意
            entity.HasIndex(e => e.StockUuid).IsUnique();

            // 図書への外部キー(book_id)は一意 → これにより book と book_stock が1対1になる
            entity.HasIndex(e => e.BookId).IsUnique();

            // 蔵書数は0以上(チェック制約)
            entity.ToTable(t =>
                t.HasCheckConstraint("chk_stock_non_negative", "stock >= 0"));
        });

        // 図書と蔵書の1対1リレーション
        // BookEntity がナビゲーション(BookStock)を持ち、外部キーは book_stock.book_id 側にある
        // 逆参照は持たないため、相手側(BookStockEntity)のナビゲーションは指定しない(WithOne 引数なし)
        modelBuilder.Entity<BookEntity>()
            .HasOne(e => e.BookStock)
            .WithOne()
            .HasForeignKey<BookStockEntity>(e => e.BookId)
            .OnDelete(DeleteBehavior.Cascade); // 図書削除時に蔵書も削除する(UC-06 BR-02)

        // ───────────────────────────────────────────
        // users(ユーザー)
        // ───────────────────────────────────────────
        modelBuilder.Entity<UserEntity>(entity =>
        {
            // 識別Id(user_uuid)は一意
            entity.HasIndex(e => e.UserUuid).IsUnique();

            // ユーザー名(username)は一意(システム内で重複不可)
            entity.HasIndex(e => e.Username).IsUnique();
        });
    }

    /// <summary>
    /// 変更を永続化する(日時の自動設定を行ってから保存)
    ///
    /// ITimestamped を実装したエンティティについて、
    /// ・新規追加(Added)時は CreatedAt と UpdatedAt の両方を現在時刻に設定する
    /// ・更新(Modified)時は UpdatedAt のみを現在時刻に更新する
    /// これにより、ドメイン層・アプリケーション層が日時を意識せずに済む
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 追加・更新されるエンティティの日時を自動設定する
    /// </summary>
    private void ApplyTimestamps()
    {
        var now = DateTime.UtcNow;

        // 変更追跡中のエンティティから、ITimestamped を実装したものだけを対象にする
        foreach (var entry in ChangeTracker.Entries<ITimestamped>())
        {
            if (entry.State == EntityState.Added)
            {
                // 新規作成時は作成日時・変更日時の両方を設定
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                // 更新時は変更日時のみ更新する(作成日時は変更しない)
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}