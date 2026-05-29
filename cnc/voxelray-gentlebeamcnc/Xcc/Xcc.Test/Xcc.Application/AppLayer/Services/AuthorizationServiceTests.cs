using Moq;
using System.Security;
using Xcc.Application.AppLayer.Service;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;

namespace Xcc.Test.Xcc.Application.AppLayer.Services;

internal class AuthorizationServiceTests
{
    public AuthorizationServiceTests() { }

    private Mock<IUserRepository> mockUserRepository;
    private const string RegisteredRoleName = "Administrator";
    private const string SomeUnknownRole = "SomeUnknownRole";
    [SetUp]
    public void SetUp()
    {
        // Mock dependencies
        mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository.Setup(repo => repo.FetchAllUserRolesAsync())
            .Returns(Task.FromResult<ICollection<UserRole>>([
                new UserRole(RegisteredRoleName) { Permissions = new UserPermissions { ClinicalData = true } }
                ]));
    }

    [Test]
    public void AuthorizeUserAsyncTest_Positive()
    {
        var initialUserRole = new UserRole(RegisteredRoleName);

        Assert.That(initialUserRole.Permissions.ClinicalData, Is.False);

        IUser user = new User()
        {
            FirstName = "TestName",
            LastName = "LastName",
            Role = initialUserRole
        };

        // 
        var service = new AuthorizationService(null!, null!, mockUserRepository.Object);
        Assert.DoesNotThrowAsync(async () => user = await service.AuthorizeUserAsync(user));
        Assert.That(user.Role.Permissions.ClinicalData, Is.True);
    }

    [Test]
    public void AuthorizeUserAsyncTest_Negative()
    {
        var initialUserRole = new UserRole(SomeUnknownRole);
        IUser user = new User()
        {
            FirstName = "TestName",
            LastName = "LastName",
            Role = initialUserRole
        };

        // 
        var service = new AuthorizationService(null!, null!, mockUserRepository.Object);
        Assert.ThrowsAsync<SecurityException>(async () => user = await service.AuthorizeUserAsync(user));
    }
}
