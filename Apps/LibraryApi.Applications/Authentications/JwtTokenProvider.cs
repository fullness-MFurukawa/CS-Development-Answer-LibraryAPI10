using System.Security.Claims;
using System.Text;
using LibraryApi.Domains.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
namespace LibraryApi.Applications.Authentications;
/// <summary>
/// IJwtTokenProvider の実装
///
/// Microsoft.IdentityModel.JsonWebTokens の JsonWebTokenHandler を用いて、
/// ユーザーの識別Idを含む JWT アクセストークンを、有効期限・署名付きで発行する。
/// </summary>
public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly JwtSettings _settings;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="settings">JWT の設定値</param>
    public JwtTokenProvider(JwtSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// アクセストークンを発行し、JWT文字列を返す
    /// </summary>
    /// <param name="user">ユーザーのドメインオブジェクト</param>
    /// <param name="extraClaims">追加のクレーム(任意)</param>
    /// <returns>JWT文字列</returns>
    public string IssueAccessToken(User user, IEnumerable<Claim>? extraClaims = null)
    {
        // 署名鍵を生成する(対称鍵 HMAC-SHA256)
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // トークンに含めるクレームを組み立てる
        // Sub(Subject)にユーザーの識別Id(user_uuid)を格納する(UC-02 BR-02)
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.UserUuid,
        };

        // 追加クレームがあれば反映する(同名のクレームは後勝ちで上書き)
        if (extraClaims is not null)
        {
            foreach (var claim in extraClaims)
            {
                claims[claim.Type] = claim.Value;
            }
        }

        // トークンの記述子(クレーム・発行者・対象者・有効期限・署名)を組み立てる
        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes), // 有効期限(UC-02 BR-05)
            SigningCredentials = signingCredentials,
        };

        // トークンを生成して文字列として返す
        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }
}