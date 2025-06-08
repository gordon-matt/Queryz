using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Queryz.Extensions;

public static class MvcBuilderExtensions
{
    public static IMvcBuilder AddQueryz<TUser, TRole>(this IMvcBuilder mvcBuilder, IConfiguration configuration, string connectionString)
        where TUser : IdentityUser
        where TRole : IdentityRole
    {
        mvcBuilder.AddApplicationPart(typeof(MvcBuilderExtensions).Assembly);
        mvcBuilder.Services.AddQueryz<TUser, TRole>(configuration, connectionString);
        return mvcBuilder;
    }
}