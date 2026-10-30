using LogVue;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Queryz.Components;
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
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddNetForgeComponents(ui =>
{
    ui.ThemeStorageKey = Queryz.Constants.ClientThemeLocalStorageKey;
    ui.DefaultColorScheme = "system";
    ui.AccentColor = "#0f766e";
    ui.AccentColorDark = "#2dd4bf";
});

builder.Services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "Queryz";
    shell.SearchPlaceholder = "Find a report";
});

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var authProvider = builder.Services.AddQueryz(builder.Configuration, options =>
{
    options.Authorization.Add(new AdministratorQueryzAuthorizationFilter());
});

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
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Reports).Assembly);

app.MapControllers();

if (authProvider == AuthProvider.Identity)
{
    app.MapRazorPages();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapQueryz();

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
    "Queryz demo started with the {DatabaseProvider} database provider and {AuthProvider} authentication",
    builder.Configuration["Database:Provider"],
    authProvider);

app.Run();
