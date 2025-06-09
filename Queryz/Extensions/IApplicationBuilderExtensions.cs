using Extenso.AspNetCore.Mvc.ExtensoUI;
using Extenso.AspNetCore.Mvc.ExtensoUI.Providers;
using Microsoft.AspNetCore.Builder;

namespace Queryz.Extensions;

public static class IApplicationBuilderExtensions
{
    public static IApplicationBuilder UseQueryz(this IApplicationBuilder app)
    {
        app.UseExtensoUI<Bootstrap5UIProvider>();
        return app;
    }
}