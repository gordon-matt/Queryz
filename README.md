[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Logo.png" alt="Logo" width="250" />

## Intro

Queryz is a report building tool that allows you to quickly connect to databases and run queries against them, while giving you the option to download the results as an xlsx file or a delimited file, such as a CSV.

## Getting started

1. Add a reference to the Queryz NuGet package (coming soon - for now, just reference the project in this repo).

2. Ensure your DbContext inherits from `QueryzDbContext<ApplicationUser, ApplicationRole>`:

```csharp
public class ApplicationDbContext : QueryzDbContext<ApplicationUser, ApplicationRole>
{
    public ApplicationDbContext(DbContextOptions<QueryzDbContext<ApplicationUser, ApplicationRole>> options)
        : base(options)
    {
    }
}
```

3. Create an `IDbContextFactory`

```csharp
public class ApplicationDbContextFactory : IDbContextFactory
{
    // see Query.Demo project for example implementation
}
```

3. Setup your `Program.cs`:

```csharp
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.AddDistributedMemoryCache(); // Required for session state
builder.Services.AddSession();

builder.Services
    .AddControllersWithViews()
    .AddNewtonsoftJson() // Currently only working with this. The model binding wouldn't work otherwise. It's on the TODO list to fix.
    .AddOData((options, serviceProvider) =>
    {
        options.Select().Expand().Filter().OrderBy().SetMaxTop(null).Count();
        var registrars = serviceProvider.GetRequiredService<IEnumerable<IODataRegistrar>>();
        foreach (var registrar in registrars)
        {
            registrar.Register(options);
        }
    })
    .AddQueryz<ApplicationUser, ApplicationRole>(builder.Configuration, connectionString)
    .AddRazorRuntimeCompilation();

// Override the default IDbContextFactory registered when calling AddQueryz()
builder.Services.AddSingleton<IDbContextFactory, ApplicationDbContextFactory>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets(); // Important

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new CompositeFileProvider(
        new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
        new EmbeddedFileProvider(
            typeof(IQueryzAssemblyMarker).Assembly,
            baseNamespace: "Queryz.wwwroot"))
});

app.UseQueryz();

// If you are commercial, then set your own commercial license here.. or else change the name passed into SetNonCommercialPersonal().
ExcelPackage.License.SetNonCommercialPersonal("Queryz");

app.Run();
```

Queryz expects there to be 3 different roles available:

```csharp
public static class QueryzConstants
{
    public static class Roles
    {
        public const string Administrators = "Administrators";
        public const string ReportBuilderEditors = "Report Builder Editors";
        public const string ReportBuilderUsers = "Report Builder Users";
    }
}
```

Customization in appsettings.json:

```json
"Queryz": {
  "Layout": "~/Views/Shared/_QueryzLayout.cshtml", // Set your own layout or use null for the default layout from the library.
  "GridPageSize": 10,
  "AppName": "DemoApp", // Only used if Layout is set to null
  "FooterText": "&copy; 2025 - DemoApp" // Only used if Layout is set to null
}
```

See the Query.Demo project in this repo for a full working example, including how to seed your database with the aforementioned roles.

## Usage

The main screen with many Reports under each Report Group. You can make as many groups as you like - one for each of your clients and/or one for each internal department and so forth.

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/01 - Main.PNG" alt="Main Screen" />

Setting up a data source:

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/02 - Data Source.PNG" alt="Data Source" />

If you have enum values in your report and want them to be displayed as text, you can setup an enumeration:

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/03 - Enums.PNG" alt="Enum Setup" />

.. which will then allow you to select that enumeration when choosing columns:

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/04 - Enum column selection.png" alt="Enum column selection" />

.. and the end result will be as follows:

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/05 - Enum result.PNG" alt="Enum result" />

There are also transform functions available for making JSON columns easier to read or for converting the timezone of a date/time field:

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/06 - Other Functions.PNG" alt="Other Functions" />

The convert timezone function will work based on the timezone you've setup for your report group (if any):

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/09 - Report Group Setup.PNG" alt="Report Group Setup" />

You can create your own transforms by implementing `ITransformFunction` and registering it with dependency injection:

```csharp
public class ConvertTimeZoneFunction : ITransformFunction
{
    public string Name => "Convert Time Zone";

    public dynamic Transform(dynamic value, Report report)
    {
        if (value is not DateTime) return value;

        if (value == null) return value;

        if (report == null || string.IsNullOrEmpty(report.Group.TimeZoneId)) return value;

        var timeZone = TZConvert.GetTimeZoneInfo(report.Group.TimeZoneId);

        return TimeZoneInfo.ConvertTimeFromUtc(value, timeZone);
    }
}

services.AddSingleton<ITransformFunction, ConvertTimeZoneFunction>();
```

You can setup default query filters, including hidden filters:

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/07 - QueryBuilder - Setup.PNG" alt="QueryBuilder - Setup" />

Notice the open/closed eye icons. If you setup a filter here as being hidden, it will still be applied when the report is run, but not show for the user when they select other filters. This is useful when there is some data they should not see (other tenants, other users, confidential records, etc). The above example is in design mode and the screenshot below is in "run report" mode:

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Screenshots/08 - QueryBuilder - Run.PNG" alt="QueryBuilder - Run" />

## Donate
If you find this project helpful, consider buying me a cup of coffee.

[![PayPal](https://img.shields.io/badge/PayPal-003087?logo=paypal&logoColor=fff)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

| Crypto         | Wallet Address |
|----------------|----------------|
| ![Bitcoin](https://img.shields.io/badge/Bitcoin-FF9900?logo=bitcoin&logoColor=white) | 1EeDfbcqoEaz6bbcWsymwPbYv4uyEaZ3Lp |
| ![Ethereum](https://img.shields.io/badge/Ethereum-3C3C3D?logo=ethereum&logoColor=white) | 0x277552efd6ea9ca9052a249e781abf1719ea9414 |
| ![Litecoin](https://img.shields.io/badge/Litecoin-A6A9AA?logo=litecoin&logoColor=white) | LRUP8hukWGXRrcPK6Tm7iUp9vPvnNNt3uz |

<img src="https://komarev.com/ghpvc/?username=gordon-matt&label=GitHub%20Hits%20Since%202025-06-01%3A%20&color=ff0000&style=flat" alt="gordon-matt" />
