using Microsoft.Extensions.DependencyInjection;

namespace Queryz.Extensions;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder AddQueryz<TUser, TRole>(this IMvcBuilder mvcBuilder, string connectionString)
        where TUser : IdentityUser
        where TRole : IdentityRole
    {
        mvcBuilder.AddApplicationPart(typeof(MvcBuilderExtensions).Assembly);
        mvcBuilder.Services.AddQueryz<TUser, TRole>(connectionString);
        return mvcBuilder;
    }
}