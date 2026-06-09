namespace LibraryApi.Domains.Adapters;
/// <summary>
/// ドメインエンティティと他のモデルを相互変換するAdapterインターフェイス
/// </summary>
/// <typeparam name="TDomain">ドメインエンティティ</typeparam>
/// <typeparam name="TOther">他のモデル</typeparam>
public interface IAdapter<TDomain, TOther>
{
    /// <summary>
    /// ドメインエンティティを他のモデルに変換する
    /// </summary>
    /// <param name="source">ドメインエンティティ</param>
    /// <returns>他のモデル</returns>
    TOther Convert(TDomain source);

    /// <summary>
    /// 他のモデルをドメインエンティティに復元する
    /// </summary>
    /// <param name="source">他のモデル</param>
    /// <returns>ドメインエンティティ</returns>
    TDomain Restore(TOther source);
}
