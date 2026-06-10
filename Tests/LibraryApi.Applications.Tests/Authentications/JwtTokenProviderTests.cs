using LibraryApi.Applications.Authentications;
using LibraryApi.Domains.Models;
using Microsoft.IdentityModel.JsonWebTokens;
namespace LibraryApi.Applications.Tests.Authentications;
/// <summary>
/// JwtTokenProvider の単体テスト
///
/// 発行した JWT トークンに、識別Id・発行者・対象者・有効期限が
/// 正しく含まれることを検証する。発行ロジックそのものが対象のため、モックは用いない。
/// </summary>
[TestClass]
[TestCategory("Authentications")]
public class JwtTokenProviderTests
{
    /// <summary>
    /// テスト用の JwtSettings を生成する
    /// </summary>
    private static JwtSettings CreateSettings()
    {
        return new JwtSettings
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            // 署名鍵は HMAC-SHA256 に十分な長さ(32バイト以上)が必要
            SecretKey = "test-secret-key-1234567890-abcdefghij",
            ExpiresInMinutes = 60,
        };
    }

    [TestMethod(DisplayName = "発行したトークンに識別Idが含まれる")]
    public void IssueAccessToken_TestCase01()
    {
        // Arrange
        var settings = CreateSettings();
        var provider = new JwtTokenProvider(settings);
        var user = User.Restore("user-uuid-1", "yamada_taro", "hashed-password");

        // Act:トークンを発行し、デコードして中身を確認する
        var token = provider.IssueAccessToken(user);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);

        // Assert:Sub クレームに識別Id が含まれること(UC-02 BR-02)
        var sub = jwt.GetClaim(JwtRegisteredClaimNames.Sub).Value;
        Assert.AreEqual("user-uuid-1", sub);
    }

    [TestMethod(DisplayName = "発行したトークンに発行者・対象者が設定される")]
    public void IssueAccessToken_TestCase02()
    {
        // Arrange
        var settings = CreateSettings();
        var provider = new JwtTokenProvider(settings);
        var user = User.Restore("user-uuid-1", "yamada_taro", "hashed-password");

        // Act
        var token = provider.IssueAccessToken(user);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);

        // Assert:Issuer と Audience が設定どおりであること
        Assert.AreEqual("test-issuer", jwt.Issuer);
        Assert.IsTrue(jwt.Audiences.Contains("test-audience"));
    }

    [TestMethod(DisplayName = "発行したトークンに有効期限が設定される")]
    public void IssueAccessToken_TestCase03()
    {
        // Arrange
        var settings = CreateSettings();
        var provider = new JwtTokenProvider(settings);
        var user = User.Restore("user-uuid-1", "yamada_taro", "hashed-password");

        // Act
        var beforeIssue = DateTime.UtcNow;
        var token = provider.IssueAccessToken(user);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(token);

        // Assert:有効期限が、発行時刻より未来に設定されていること(UC-02 BR-05)
        Assert.IsTrue(jwt.ValidTo > beforeIssue);
        // おおむね設定どおり(60分後)の範囲にあること(多少の誤差を許容)
        var expectedExpiry = beforeIssue.AddMinutes(60);
        Assert.IsTrue(jwt.ValidTo <= expectedExpiry.AddMinutes(1));
    }
}