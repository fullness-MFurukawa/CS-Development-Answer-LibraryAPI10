using System.Net;
using System.Text.Json;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Domains.Exceptions; 
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Middlewares;
/// <summary>
/// 例外を捕捉し、種類に応じた HTTP ステータスと統一的なエラーレスポンスに変換するミドルウェア
///
/// ・NotFoundException   → 404(例外が持つ ErrorCode を使用)
/// ・InvalidInputException → 400(例外が持つ ErrorCode を使用)
/// ・DomainException      → 400(エラーコードは ValidationError 固定)
/// ・その他(想定外)       → 500(エラーコードは InternalServerError 固定)
///
/// 例外の「種類の識別(ErrorCode)」はアプリケーション層の例外が持ち、
/// 「HTTP ステータスへの変換」は本ミドルウェア(プレゼンテーション層)が担う。
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// リクエストを処理し、例外が発生した場合はエラーレスポンスに変換する
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
         catch (AuthenticationException ex)
        {
            // 認証失敗 → 401
            await WriteErrorResponseAsync(context, HttpStatusCode.Unauthorized, ex.ErrorCode, ex.Message);
        }
        catch (NotFoundException ex)
        {
            // リソースが見つからない → 404
            await WriteErrorResponseAsync(context, HttpStatusCode.NotFound, ex.ErrorCode, ex.Message);
        }
        catch (ConflictException ex)   
        {
            // リソースの競合(ユーザー名重複など) → 409
            await WriteErrorResponseAsync(context, HttpStatusCode.Conflict, ex.ErrorCode, ex.Message);
        }
        catch (InvalidInputException ex)
        {
            // 入力が業務的に不正 → 400
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.ErrorCode, ex.Message);
        }
        catch (DomainException ex)
        {
            // ドメインの不変条件違反 → 400(エラーコードは固定)
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, "ValidationError", ex.Message);
        }
        catch (Exception ex)
        {
            // 想定外の例外 → 500(詳細はクライアントに返さず、サーバー側でログに記録する)
            _logger.LogError(ex, "未処理の例外が発生しました。");
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                "InternalServerError",
                "サーバー内部でエラーが発生しました。");
        }
    }

    /// <summary>
    /// 指定されたステータスとエラー内容で、JSON のエラーレスポンスを書き込む
    /// </summary>
    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string error,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var body = new ErrorResponse { Error = error, Message = message };

        // レスポンスの JSON も、API 全体と同じ camelCase で出力する
        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        await context.Response.WriteAsync(json);
    }
}