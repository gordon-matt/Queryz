using Autofac;
using Autofac.Extensions.DependencyInjection;
using Extenso.AspNetCore.Mvc.ExtensoUI;
using Extenso.AspNetCore.Mvc.ExtensoUI.Providers;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.OData;
using Queryz.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add Autofac
builder.Host
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterType<ApplicationDbContextFactory>().As<IDbContextFactory>().SingleInstance();

        containerBuilder.RegisterGeneric(typeof(EntityFrameworkRepository<>))
            .As(typeof(IRepository<>))
            .InstancePerLifetimeScope();

        containerBuilder.RegisterType<QueryzRazorViewRenderService>().As<IRazorViewRenderService>().SingleInstance();
        containerBuilder.RegisterType<ODataRegistrar>().As<IODataRegistrar>().SingleInstance();

        containerBuilder.RegisterType<DataSourceService>().As<IDataSourceService>().InstancePerDependency();
        containerBuilder.RegisterType<EnumerationService>().As<IEnumerationService>().InstancePerDependency();
        containerBuilder.RegisterType<ReportService>().As<IReportService>().InstancePerDependency();
        containerBuilder.RegisterType<ReportTableService>().As<IReportTableService>().InstancePerDependency();
        containerBuilder.RegisterType<ReportTableColumnService>().As<IReportTableColumnService>().InstancePerDependency();
        containerBuilder.RegisterType<ReportGroupService>().As<IReportGroupService>().InstancePerDependency();
        containerBuilder.RegisterType<ReportGroupRoleService>().As<IReportGroupRoleService>().InstancePerDependency();
        containerBuilder.RegisterType<ReportSortingService>().As<IReportSortingService>().InstancePerDependency();
        containerBuilder.RegisterType<ReportUserBlacklistService>().As<IReportUserBlacklistService>().InstancePerDependency();

        // Transform Functions
        containerBuilder.RegisterType<BeautifyJsonFunction>().As<ITransformFunction>().SingleInstance();
        containerBuilder.RegisterType<ConvertTimeZoneFunction>().As<ITransformFunction>().SingleInstance();
        //containerBuilder.RegisterType<RoundUpFunction>().As<ITransformFunction>().SingleInstance();

        containerBuilder.RegisterType<ReportBuilderService>().As<IReportBuilderService>().InstancePerDependency();
    });

// Configure services
builder.Services.AddAutofac();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity setup
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services
    .AddControllersWithViews()
    .AddApplicationPart(typeof(Microsoft.AspNetCore.Identity.UI.LoggerEventIds).Assembly)
    .AddNewtonsoftJson()
    .AddOData((options, serviceProvider) =>
    {
        options.Select().Expand().Filter().OrderBy().SetMaxTop(null).Count();
        var registrars = serviceProvider.GetRequiredService<IEnumerable<IODataRegistrar>>();
        foreach (var registrar in registrars)
        {
            registrar.Register(options);
        }
    });

builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseExtensoUI<Bootstrap4UIProvider>();

app.UseAuthentication();
app.UseAuthorization();

// IMPORTANT: This session call MUST go before UseMvc()
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        await DbInitializer.InitializeAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();