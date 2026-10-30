using LogVue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Queryz.Components;
using QueryKrafter;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogVue(options =>
    builder.Configuration.GetSection(LogVueOptions.SectionName).Bind(options));

builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.LogVue(services));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddNetForgeComponents(ui =>
{
    ui.ThemeStorageKey = global::Queryz.Constants.ClientThemeLocalStorageKey;
    ui.DefaultColorScheme = "system";
    ui.AccentColor = "#0f766e";
    ui.AccentColorDark = "#2dd4bf";
});

builder.Services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "Queryz";
    shell.SearchPlaceholder = "Find a report";
});
builder.Services.AddScoped<IShellNavigation, QueryzNavigation>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddQueryKrafter();

string databaseProvider = DatabaseProviders.Resolve(
    builder.Configuration[DatabaseProviders.ConfigurationKey],
    DatabaseProviders.Sqlite,
    [.. DatabaseProviders.All]);

builder.Services.AddQueryzDatabase(builder.Configuration);
builder.Services.AddQueryzRepositories();

var authProvider = builder.Services.AddQueryzAuthentication(builder.Configuration);
builder.Services.AddQueryzServices();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeaders.KnownIPNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapLogVue().RequireAuthorization(new AuthorizeAttribute
{
    Roles = WellKnownRoles.Administrator
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

if (authProvider == AuthProvider.Identity)
{
    app.MapRazorPages();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapReportExport();

await using (var scope = app.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        await DbInitializer.InitializeAsync(scope.ServiceProvider, authProvider, app.Configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed");
        throw;
    }
}

Log.Information(
    "Queryz started with the {DatabaseProvider} database provider and {AuthProvider} authentication",
    databaseProvider,
    authProvider);

app.Run();
