namespace Queryz.Services;

public interface IRoleService
{
    Task<IEnumerable<IdentityRole>> GetRoles();

    Task<IEnumerable<IdentityRole>> GetRolesByNameAsync(IEnumerable<string> roleNames);

    Task<IEnumerable<IdentityRole>> GetRolesByIdAsync(IEnumerable<string> roleIds);
}

public class RoleService<TRole> : IRoleService
    where TRole : IdentityRole
{
    private readonly RoleManager<TRole> roleManager;

    public RoleService(RoleManager<TRole> roleManager)
    {
        this.roleManager = roleManager;
    }

    public async Task<IEnumerable<IdentityRole>> GetRoles() =>
        await roleManager.Roles.ToListAsync();

    public async Task<IEnumerable<IdentityRole>> GetRolesByNameAsync(IEnumerable<string> roleNames) =>
        await roleManager.Roles.Where(role => roleNames.Contains(role.Name)).ToListAsync();

    public async Task<IEnumerable<IdentityRole>> GetRolesByIdAsync(IEnumerable<string> roleIds) =>
        await roleManager.Roles.Where(role => roleIds.Contains(role.Id)).ToListAsync();
}