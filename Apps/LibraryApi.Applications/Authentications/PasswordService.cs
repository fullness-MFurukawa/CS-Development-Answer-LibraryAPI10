using Microsoft.AspNetCore.Identity;
namespace LibraryApi.Applications.Authentications;
/// <summary>
/// IPasswordService の実装
///
/// ASP.NET Core Identity の PasswordHasher を用いて、パスワードのハッシュ化・照合を行う。
/// PasswordHasher は PBKDF2 によるハッシュ化(V3形式)を行い、ソルトを内包する。
/// 型引数(TUser)は実装上使用されないため、objectを指定する。
/// </summary>
public class PasswordService : IPasswordService
{
    // PasswordHasher は状態を持たず、スレッドセーフに使える
    private readonly PasswordHasher<object> _passwordHasher = new();

    // VerifyHashedPassword に渡すダミーのユーザー(実装上使用されない)
    private static readonly object DummyUser = new();

    /// <summary>
    /// 平文パスワードをハッシュ化する
    /// </summary>
    /// <param name="password">平文パスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    public string Hash(string password)
    {
        // 第1引数(user)は実装上使用されないため、ダミーを渡す
        return _passwordHasher.HashPassword(DummyUser, password);
    }

    /// <summary>
    /// 平文パスワードと、保存済みのハッシュ化パスワードを照合する
    /// </summary>
    /// <param name="hashedPassword">保存済みのハッシュ化パスワード</param>
    /// <param name="password">照合する平文パスワード</param>
    /// <returns>一致する場合は true、しない場合は false</returns>
    public bool Verify(string hashedPassword, string password)
    {
        // 照合結果を取得する
        var result = _passwordHasher.VerifyHashedPassword(DummyUser, hashedPassword, password);

        // Success(完全一致)と、SuccessRehashNeeded(一致するが再ハッシュ推奨)を、一致とみなす
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}