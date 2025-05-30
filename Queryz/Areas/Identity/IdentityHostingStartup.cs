[assembly: HostingStartup(typeof(Queryz.Areas.Identity.IdentityHostingStartup))]

namespace Queryz.Areas.Identity;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder.ConfigureServices((context, services) =>
                                                               {
                                                               });
}