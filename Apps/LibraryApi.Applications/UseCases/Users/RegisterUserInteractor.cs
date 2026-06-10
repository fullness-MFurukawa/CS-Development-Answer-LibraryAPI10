using LibraryApi.Applications.Authentications;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services.Users;
using LibraryApi.Applications.UseCases.UnitOfWorks; 
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.UseCases.Users;
/// <summary>
/// IRegisterUserUseCase の実装(Interactor)
///
/// ユーザー名の重複を確認し、パスワードをハッシュ化してユーザーを登録する。
/// ユーザー名が既に存在する場合は ConflictException をスローする(UC-01、409相当)。
/// </summary>
public class RegisterUserInteractor : IRegisterUserUseCase
{
    private readonly IUserService _userService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdapter<User, UserDto> _userDtoAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public RegisterUserInteractor(
        IUserService userService,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork,
        IAdapter<User, UserDto> userDtoAdapter)
    {
        _userService = userService;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _userDtoAdapter = userDtoAdapter;
    }

    /// <summary>
    /// 新しいユーザーを登録する
    /// </summary>
    /// <param name="input">ユーザー登録の入力情報</param>
    /// <returns>登録されたユーザーの情報</returns>
    /// <exception cref="ConflictException">ユーザー名が既に使用されている場合</exception>
    public async Task<UserDto> ExecuteAsync(RegisterUserDto input)
    {
        // ユーザー名の重複を確認する(トランザクション開始前の前提条件チェック)
        var existing = await _userService.FindByUsernameAsync(input.Username);
        if (existing is not null)
        {
            // 既に同名のユーザーが存在する → 競合として扱う(409相当)
            throw new ConflictException(
                "DuplicateUsername", "そのユーザー名は既に使用されています。");
        }

        // パスワードをハッシュ化する(平文は保存しない)
        var hashedPassword = _passwordService.Hash(input.Password);

        // ユーザーを構築する(ユーザー名の妥当性は User.Create 内のドメイン検証が担う)
        var user = User.Create(input.Username, hashedPassword);

        // ユーザーをトランザクション内で永続化する
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _userService.AddAsync(user);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        // 登録したユーザーを DTO に変換して返す(パスワードは含めない)
        return _userDtoAdapter.Convert(user);
    }
}