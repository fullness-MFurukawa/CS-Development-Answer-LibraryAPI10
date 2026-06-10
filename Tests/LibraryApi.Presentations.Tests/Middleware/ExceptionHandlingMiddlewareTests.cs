using System.Text.Json;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Domains.Exceptions; // DomainException(実際の名前空間に合わせる)
using LibraryApi.Presentations.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
namespace LibraryApi.Presentations.Tests.Middlewares;
/// <summary>
/// ExceptionHandlingMiddleware の単体テスト
///
/// 後続処理で各種例外が投げられたとき、例外の型に応じた
/// HTTP ステータスとエラー形式(error/message)で応答することを検証する。
/// </summary>
[TestClass]
[TestCategory("Middlewares")]
public class ExceptionHandlingMiddlewareTests
{
    // 指定した例外を投げるミドルウェアを実行し、ステータスとボディを返すヘルパ
    private static async Task<(int statusCode, string error, string message)> InvokeWithExceptionAsync(
        Exception exception)
    {
        // 後続処理として、指定された例外を投げるデリゲートを用意する
        RequestDelegate next = _ => throw exception;

        var middleware = new ExceptionHandlingMiddleware(
            next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // 実行
        await middleware.InvokeAsync(context);

        // レスポンスボディを読む
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();

        // JSON をパースして error/message を取り出す
        using var doc = JsonDocument.Parse(bodyText);
        var error = doc.RootElement.GetProperty("error").GetString() ?? "";
        var message = doc.RootElement.GetProperty("message").GetString() ?? "";

        return (context.Response.StatusCode, error, message);
    }

    [TestMethod(DisplayName = "NotFoundExceptionを404とエラーコードに変換する")]
    public async Task InvokeAsync_TestCase01()
    {
        var (status, error, message) = await InvokeWithExceptionAsync(
            new NotFoundException("BookNotFound", "指定された図書が存在しません。"));

        Assert.AreEqual(404, status);
        Assert.AreEqual("BookNotFound", error);
        Assert.AreEqual("指定された図書が存在しません。", message);
    }

    [TestMethod(DisplayName = "InvalidInputExceptionを400とエラーコードに変換する")]
    public async Task InvokeAsync_TestCase02()
    {
        var (status, error, _) = await InvokeWithExceptionAsync(
            new InvalidInputException("CategoryNotFound", "指定された分類が存在しません。"));

        Assert.AreEqual(400, status);
        Assert.AreEqual("CategoryNotFound", error);
    }

    [TestMethod("ConflictExceptionを409とエラーコードに変換する")]
    public async Task InvokeAsync_TestCase03()
    {
        var (status, error, _) = await InvokeWithExceptionAsync(
            new ConflictException("DuplicateUsername", "そのユーザー名は既に使用されています。"));

        Assert.AreEqual(409, status);
        Assert.AreEqual("DuplicateUsername", error);
    }

    [TestMethod(DisplayName = "DomainExceptionを400とValidationErrorに変換する")]
    public async Task InvokeAsync_TestCase04()
    {
        var (status, error, _) = await InvokeWithExceptionAsync(
            new DomainException("書名は1~50文字で入力してください。")); // コンストラクタは実際に合わせる

        Assert.AreEqual(400, status);
        Assert.AreEqual("ValidationError", error);
    }

    [TestMethod(DisplayName = "想定外の例外を500とInternalServerErrorに変換する")]
    public async Task InvokeAsync_TestCase05()
    {
        var (status, error, _) = await InvokeWithExceptionAsync(
            new Exception("想定外のエラー"));

        Assert.AreEqual(500, status);
        Assert.AreEqual("InternalServerError", error);
    }
}