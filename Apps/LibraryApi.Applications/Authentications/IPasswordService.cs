namespace LibraryApi.Applications.Authentications;
/// <summary>
/// パスワードのハッシュ化・照合を行うコンポーネントのインターフェイス
///
/// ・Hash   : 平文パスワードをハッシュ化する(ユーザー登録時)
/// ・Verify : 平文パスワードと、保存済みのハッシュ化パスワードを照合する(ログイン時)
///
/// ハッシュ化は ASP.NET Core Identity の PasswordHasher(PBKDF2)を用いて実装する。
/// (平文の比較は行わない:UC-02 BR-01)
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// 平文パスワードをハッシュ化する
    /// </summary>
    /// <param name="password">平文パスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    string Hash(string password);

    /// <summary>
    /// 平文パスワードと、保存済みのハッシュ化パスワードを照合する
    /// </summary>
    /// <param name="hashedPassword">保存済みのハッシュ化パスワード</param>
    /// <param name="password">照合する平文パスワード</param>
    /// <returns>一致する場合は true、しない場合は false</returns>
    bool Verify(string hashedPassword, string password);
}