using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Tests.Models;
/// <summary>
/// Entity基底クラスの単体テストドライバ
/// Entity は抽象クラスのため、テスト専用の具象クラス(TestEntity)を用意して
/// 同一性判定(Equals / GetHashCode / == / !=)の振る舞いを検証する
/// </summary>
[TestClass]
[TestCategory("Models")]
public class EntityTests
{
    /// <summary>
    /// Entity の振る舞いを検証するためのテスト専用具象クラス
    /// 任意の UUID(空文字含む)を設定できるようにし、各分岐を検証可能にする
    /// </summary>
    private class TestEntity : Entity
    {
        private readonly string _identity;

        // 同一性判定に使う UUID を、コンストラクタで自由に指定できるようにする
        public TestEntity(string identity)
        {
            _identity = identity;
        }

        protected override string Identity => _identity;
    }

    // ───────────────────────────────────────────
    // Equals
    // ───────────────────────────────────────────

    [TestMethod(DisplayName ="同じUUIDを持つエンティティ同士は等しい")]
    public void Equals_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var entity1 = new TestEntity(uuid);
        var entity2 = new TestEntity(uuid);

        // Act / Assert
        Assert.IsTrue(entity1.Equals(entity2));
    }

    [TestMethod(DisplayName ="異なるUUIDを持つエンティティ同士は等しくない")]
    public void Equals_TestCase2()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid().ToString());
        var entity2 = new TestEntity(Guid.NewGuid().ToString());

        // Act / Assert
        Assert.IsFalse(entity1.Equals(entity2));
    }

    [TestMethod(DisplayName ="nullとは等しくない")]
    public void Equals_TestCase3()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid().ToString());

        // Act / Assert
        Assert.IsFalse(entity.Equals(null));
    }

    [TestMethod(DisplayName ="UUIDが未設定(空文字)の場合は参照が同一のときのみ等しい")]
    public void Equals_TestCase4()
    {
        // Arrange
        // UUID が空文字の2つの別インスタンス
        var entity1 = new TestEntity(string.Empty);
        var entity2 = new TestEntity(string.Empty);

        // Act / Assert
        // 空文字同士でも、別インスタンスなら等しくない(値比較ではなく参照比較になる)
        Assert.IsFalse(entity1.Equals(entity2));
        // 自分自身(参照が同一)とは等しい
        Assert.IsTrue(entity1.Equals(entity1));
    }

    // ───────────────────────────────────────────
    // GetHashCode
    // ───────────────────────────────────────────

    [TestMethod(DisplayName ="同じUUIDを持つエンティティは同じハッシュコードを返す")]
    public void GetHashCode_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var entity1 = new TestEntity(uuid);
        var entity2 = new TestEntity(uuid);

        // Act / Assert
        // Equals が true なら GetHashCode も一致する必要がある(両者の一貫性)
        Assert.AreEqual(entity1.GetHashCode(), entity2.GetHashCode());
    }

    // ───────────────────────────────────────────
    // == / != 演算子
    // ───────────────────────────────────────────

    [TestMethod(DisplayName ="同じUUIDを持つエンティティは==でtrueになる")]
    public void EqualityOperator_TestCase1()
    {
        // Arrange
        var uuid = Guid.NewGuid().ToString();
        var entity1 = new TestEntity(uuid);
        var entity2 = new TestEntity(uuid);

        // Act / Assert
        Assert.IsTrue(entity1 == entity2);
        Assert.IsFalse(entity1 != entity2);
    }

    [TestMethod(DisplayName ="異なるUUIDを持つエンティティは==でfalseになる")]
    public void EqualityOperator_TestCase2()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid().ToString());
        var entity2 = new TestEntity(Guid.NewGuid().ToString());

        // Act / Assert
        Assert.IsFalse(entity1 == entity2);
        Assert.IsTrue(entity1 != entity2);
    }

    [TestMethod(DisplayName ="null同士は==でtrueになる")]
    public void EqualityOperator_TestCase3()
    {
        // Arrange
        TestEntity? entity1 = null;
        TestEntity? entity2 = null;

        // Act / Assert
        Assert.IsTrue(entity1 == entity2);
        Assert.IsFalse(entity1 != entity2);
    }

    [TestMethod(DisplayName ="一方だけがnullなら==でfalseになる")]
    public void EqualityOperator_TestCase4()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid().ToString());

        // Act / Assert
        Assert.IsFalse(entity == null);
        Assert.IsFalse(null == entity);
        Assert.IsTrue(entity != null);
    }
}