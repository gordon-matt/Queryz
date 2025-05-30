using Autofac.Extensions.DependencyInjection;

namespace Queryz.BlazorServer;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .UseServiceProviderFactory(new AutofacServiceProviderFactory());
}