using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.UseCases.Users;
using LibraryApi.Domains.Adapters;
using LibraryApi.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace LibraryApi.Presentations.Controllers;
/// <summary>
/// ユーザーに関する API を提供する
/// </summary>
[ApiController]
[Route("library/api/users")]
[Tags("ユーザー")]
public class UsersController : ControllerBase
{
    private readonly IRegisterUserUseCase _registerUserUseCase;
    private readonly IAdapter<RegisterUserDto, RegisterUserRequest> _registerUserRequestAdapter;
    private readonly IAdapter<UserDto, UserResponse> _userResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public UsersController(
        IRegisterUserUseCase registerUserUseCase,
        IAdapter<RegisterUserDto, RegisterUserRequest> registerUserRequestAdapter,
        IAdapter<UserDto, UserResponse> userResponseAdapter)
    {
        _registerUserUseCase = registerUserUseCase;
        _registerUserRequestAdapter = registerUserRequestAdapter;
        _userResponseAdapter = userResponseAdapter;
    }

    /// <summary>
    /// 新しいユーザーを登録する
    /// POST /library/api/users
    /// </summary>
    /// <param name="request">ユーザー登録リクエスト</param>
    /// <returns>登録されたユーザー(201 Created)。ユーザー名が重複する場合は 409。</returns>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> RegisterUser(
        [FromBody] RegisterUserRequest request)
    {
        // リクエスト(ViewModel)を入力用 DTO に変換する(ViewModel → DTO)
        var input = _registerUserRequestAdapter.Restore(request);

        // ユーザーを登録する(ユーザー名が重複する場合、UseCase が ConflictException を投げ、
        //  ミドルウェアが 409 に変換する)
        var dto = await _registerUserUseCase.ExecuteAsync(input);

        // 登録結果(DTO)をレスポンス(ViewModel)に変換する(DTO → ViewModel)
        var response = _userResponseAdapter.Convert(dto);

        // 201 Created で返す
        return StatusCode(StatusCodes.Status201Created, response);
    }
}