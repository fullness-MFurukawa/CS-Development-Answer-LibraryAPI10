namespace LibraryApi.Domains.Aggregators;

/// <summary>
/// ある構築元(EF Core エンティティの集約、ViewModel など)から、
/// ドメインの集約を構築する Aggregator のインターフェイス
///
/// 【役割】
/// 単一エンティティの相互変換は Adapter(IAdapter)が担うのに対し、
/// Aggregator は「複数の部品から1つの集約を組み立てる」という、より複雑な構築を担う。
/// 個々の部品の変換は Adapter に委譲し、Aggregator は組み立ての段取りに専念する。
///
/// 構築元の型(TSource)を差し替えることで、構築元が変わっても
/// (例: EF Core エンティティから、あるいは ViewModel から)同じ枠組みで集約を構築できる。
/// </summary>
/// <typeparam name="TSource">構築元の型(例: BookEntity、BookViewModel)</typeparam>
/// <typeparam name="TAggregate">構築されるドメインの集約の型(例: Book)</typeparam>
public interface IAggregator<TSource, TAggregate>
{
    /// <summary>
    /// 構築元から、ドメインの集約を構築する
    /// </summary>
    /// <param name="source">構築元</param>
    /// <returns>構築されたドメインの集約</returns>
    TAggregate Aggregate(TSource source);
}