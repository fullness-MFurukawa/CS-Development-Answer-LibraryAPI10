using System.Security.Claims;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Authentications;
/// <summary>
/// JWTの発行インターフェイス
///
/// ユーザーの識別情報を含む JWT アクセストークンを発行する(UC-02 BR-02、BR-05)。
/// トークンの検証はプレゼンテーション層の認証ミドルウェアが担うため、本インターフェイスは
/// 発行のみを責務とする。生成したトークンを HttpOnly Cookie にセットするのもプレゼンテーション層の責務。
/// </summary>
public interface IJwtTokenProvider
{
    /// <summary>
    /// アクセストークンを発行し、JWT文字列を返す
    /// </summary>
    /// <param name="user">
    ///     ユーザーのドメインオブジェクト(UserUuid などの識別情報を利用)
    /// </param>
    /// <param name="extraClaims">追加のクレーム(任意)</param>
    /// <returns>JWT文字列("header.payload.signature")</returns>
    string IssueAccessToken(User user, IEnumerable<Claim>? extraClaims = null);
}