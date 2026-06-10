using LibraryApi.Applications.Dtos;
using LibraryApi.Applications.Exceptions;
using LibraryApi.Applications.UseCases.Users;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.Controllers;
using LibraryApi.Presentations.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace LibraryApi.Presentations.Tests.Controllers;
/// <summary>
/// UsersController の単体テスト
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class UsersControllerTests
{
    private static UsersController CreateController(Mock<IRegisterUserUseCase> useCaseMock)
    {
        return new UsersController(
            useCaseMock.Object,
            new RegisterUserRequestAdapter(),
            new UserResponseAdapter());
    }

    [TestMethod(DisplayName = "ユーザー登録:201 Createdで返す")]
    public async Task RegisterUser_TestCase01()
    {
        var useCaseMock = new Mock<IRegisterUserUseCase>();
        useCaseMock
            .Setup(u => u.ExecuteAsync(It.IsAny<RegisterUserDto>()))
            .ReturnsAsync(new UserDto { UserId = "user-1", Username = "test_user" });

        var controller = CreateController(useCaseMock);
        var request = new RegisterUserRequest { Username = "test_user", Password = "password123" };

        var actionResult = await controller.RegisterUser(request);

        // StatusCode(201, ...) を使った場合、戻り値は ObjectResult
        var objectResult = actionResult.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(201, objectResult.StatusCode);
        var response = objectResult.Value as UserResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("user-1", response.UserId);
        Assert.AreEqual("test_user", response.Username);
    }

    [TestMethod(DisplayName = "ユーザー登録:UseCaseのConflictExceptionを素通しする")]
    public async Task RegisterUser_TestCase02()
    {
        var useCaseMock = new Mock<IRegisterUserUseCase>();
        useCaseMock
            .Setup(u => u.ExecuteAsync(It.IsAny<RegisterUserDto>()))
            .ThrowsAsync(new ConflictException("DuplicateUsername", "そのユーザー名は既に使用されています。"));

        var controller = CreateController(useCaseMock);
        var request = new RegisterUserRequest { Username = "test_user", Password = "password123" };

        await Assert.ThrowsExactlyAsync<ConflictException>(
            () => controller.RegisterUser(request));
    }
}