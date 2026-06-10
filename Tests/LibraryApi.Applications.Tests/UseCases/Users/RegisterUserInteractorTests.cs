using LibraryApi.Applications.Adapters;
using LibraryApi.Applications.Authentications;
using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.Services.Users;
using LibraryApi.Applications.UseCases.Users;
using LibraryApi.Applications.UseCases.UnitOfWorks;
using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using Moq;
namespace LibraryApi.Applications.Tests.UseCases.Users;
/// <summary>
/// RegisterUserInteractor の単体テスト
///
/// ユーザー登録のロジックを検証する。
/// ・ユーザー名が重複していれば ConflictException(トランザクションは開始しない)
/// ・正常時はハッシュ化・構築・保存し、トランザクションをコミットする
/// ・保存失敗時はロールバックして例外を再スローする
/// </summary>
[TestClass]
[TestCategory("UseCases")]
public class RegisterUserInteractorTests
{
    private static IAdapter<User, UserDto> CreateUserDtoAdapter()
    {
        return new UserDtoAdapter();
    }

    [TestMethod(DisplayName = "正常時:ハッシュ化・構築・保存しコミットしてDTOを返す")]
    public async Task ExecuteAsync_TestCase01()
    {
        // Arrange:ユーザー名が重複していない
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.FindByUsernameAsync("yamada_taro"))
            .ReturnsAsync((User?)null);
        userServiceMock
            .Setup(s => s.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var passwordServiceMock = new Mock<IPasswordService>();
        passwordServiceMock
            .Setup(p => p.Hash("plain-password"))
            .Returns("hashed-password");

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var input = new RegisterUserDto { Username = "yamada_taro", Password = "plain-password" };

        var interactor = new RegisterUserInteractor(
            userServiceMock.Object,
            passwordServiceMock.Object,
            unitOfWorkMock.Object,
            CreateUserDtoAdapter());

        // Act
        var result = await interactor.ExecuteAsync(input);

        // Assert:DTO が返り、ユーザー名が反映されていること
        Assert.AreEqual("yamada_taro", result.Username);
        Assert.IsFalse(string.IsNullOrEmpty(result.UserId)); // UUID が採番されている

        // パスワードのハッシュ化が呼ばれたこと
        passwordServiceMock.Verify(p => p.Hash("plain-password"), Times.Once);
        // 保存とトランザクション制御の検証
        userServiceMock.Verify(s => s.AddAsync(It.IsAny<User>()), Times.Once);
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Never);
    }

    [TestMethod(DisplayName = "ユーザー名が重複していればConflictExceptionをスローする")]
    public async Task ExecuteAsync_TestCase02()
    {
        // Arrange:同名のユーザーが既に存在する
        var existingUser = User.Restore("existing-uuid", "yamada_taro", "hashed-password");

        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.FindByUsernameAsync("yamada_taro"))
            .ReturnsAsync(existingUser);

        var passwordServiceMock = new Mock<IPasswordService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var input = new RegisterUserDto { Username = "yamada_taro", Password = "plain-password" };

        var interactor = new RegisterUserInteractor(
            userServiceMock.Object,
            passwordServiceMock.Object,
            unitOfWorkMock.Object,
            CreateUserDtoAdapter());

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ConflictException>(
            () => interactor.ExecuteAsync(input));

        // トランザクションは開始されず、保存も行われないこと
        unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
        userServiceMock.Verify(s => s.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [TestMethod(DisplayName = "保存失敗時:ロールバックして例外を再スローする")]
    public async Task ExecuteAsync_TestCase03()
    {
        // Arrange:重複なし、しかし保存で例外が発生する
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.FindByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        userServiceMock
            .Setup(s => s.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("DB エラー"));

        var passwordServiceMock = new Mock<IPasswordService>();
        passwordServiceMock.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        var input = new RegisterUserDto { Username = "yamada_taro", Password = "plain-password" };

        var interactor = new RegisterUserInteractor(
            userServiceMock.Object,
            passwordServiceMock.Object,
            unitOfWorkMock.Object,
            CreateUserDtoAdapter());

        // Act & Assert
        await Assert.ThrowsExactlyAsync<Exception>(
            () => interactor.ExecuteAsync(input));

        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
    }
}