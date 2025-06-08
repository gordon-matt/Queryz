namespace Queryz.Services;

public interface IUserService
{
    Task<IdentityUser> FindByNameAsync(string userName);

    Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName);

    Task<IList<string>> GetRolesAsync(IdentityUser user);

    Task<IList<IdentityUser>> GetUsersByIds(IEnumerable<string> userIds);
}

public class UserService<TUser> : IUserService
    where TUser : IdentityUser
{
    private readonly UserManager<TUser> userManager;

    public UserService(UserManager<TUser> userManager)
    {
        this.userManager = userManager;
    }

    public async Task<IdentityUser> FindByNameAsync(string userName) =>
        await userManager.FindByNameAsync(userName);

    public async Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName) =>
        (await userManager.GetUsersInRoleAsync(roleName)).Select(x => x as IdentityUser).ToList();

    public async Task<IList<string>> GetRolesAsync(IdentityUser user) =>
        await userManager.GetRolesAsync(user as TUser);

    public async Task<IList<IdentityUser>> GetUsersByIds(IEnumerable<string> userIds) =>
        (await userManager.Users.Where(x => userIds.Contains(x.Id)).ToListAsync()).Select(x => x as IdentityUser).ToList();
}