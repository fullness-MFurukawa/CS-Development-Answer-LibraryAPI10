namespace LibraryApi.Domains.Models;

/// <summary>
/// すべてのエンティティが継承する抽象基底クラス
/// ドメイン駆動設計における「エンティティの同一性(identity)」を一元的に提供する
/// 
/// 各エンティティはこのクラスを継承し、Identity プロパティに
/// 自身の UUID を返す実装を与えるだけで、正しい同一性判定を獲得できる
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// 読み取り専用プロパティ
    /// 同一性判定に用いる識別子(UUID)
    /// </summary>
    protected abstract string Identity { get; }

    /// <summary>
    /// 2つのエンティティが同一かどうかを、UUID の一致で判定する。
    /// </summary>
    public override bool Equals(object? obj)
    {
        // nullや、異なる型のオブジェクトとは決して等しくない。
        // GetType()メソッドで厳密に型を比較し、別エンティティ型との誤った一致を防ぐ
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }
        var other = (Entity)obj;

        // 【重要】UUIDが未設定(空文字)の場合の扱い。
        // 新規生成直後でまだ UUIDが割り当たっていないケースを想定し、
        // その場合は値での比較を行わず、参照が同一かどうかでのみ判定する。
        // これにより、UUID 未設定の別インスタンス同士が誤って同一と判定されることを防ぐ
        if (string.IsNullOrEmpty(Identity) || string.IsNullOrEmpty(other.Identity))
        {
            return ReferenceEquals(this, other);
        }

        // UUIDが一致すれば、同一エンティティ
        return Identity == other.Identity;
    }

    /// <summary>
    /// ハッシュコードを返す
    /// Equals()メソッドと一貫性を保つため、UUIDから算出する
    /// </summary>
    public override int GetHashCode()
    {
        return string.IsNullOrEmpty(Identity)
            ? base.GetHashCode()
            : Identity.GetHashCode();
    }

    /// <summary>
    /// == 演算子
    /// Equals()に委譲し、値ベース(UUID)での比較を提供する
    /// null同士の比較にも正しく対応する
    /// </summary>
    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        // 一方だけが null の場合は等しくない。
        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// != 演算子
    /// == の否定として実装する
    /// </summary>
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}