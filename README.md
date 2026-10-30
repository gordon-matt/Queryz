[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

<img src="https://github.com/gordon-matt/Queryz/blob/main/_Misc/Logo.png" alt="Logo" width="350" />

Queryz is a Blazor Razor Class Library for building reports against the databases you connect. A report selects columns from one or more joined tables, stores its filters as [QueryKrafter](https://github.com/gordon-matt/QueryKrafter) rules, and turns those rules into a parameterized WHERE clause on the server. Running a report shows the filters first, then a preview of the first 10 rows. Download exports the full result as Excel (.xlsx) or a delimited file (comma, tab, vertical bar, or semicolon), still honoring a row limit saved on the report.

Groups are the folders on the main screen. Use one per client, department, or whatever else matches how you share reports. A group has a time zone, used by the Convert Time Zone transform, and a set of roles that decide who can run its reports.

Enumerations turn integer columns into text, a number, or both. The built-in transforms are Round Up, Beautify JSON, and Convert Time Zone.

In the filter step of the designer, each condition can be hidden. A hidden condition is still applied when the report runs and when it is downloaded, and the person running the report cannot see it or remove it. That is how an administrator keeps a predicate such as a tenant id in force. A column marked hidden is treated the same way.

`Queryz.Demo` is the sample host. Run it with:

```bash
dotnet run --project Queryz.Demo
```

The development seed signs in as `admin` / `Admin@123`. Change that password before sharing the instance. The demo database is `Queryz/queryz.db`.

## Hosting

```csharp
builder.Services.AddQueryz(builder.Configuration, options =>
{
    options.Authorization.Add(new AdministratorQueryzAuthorizationFilter());
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Reports).Assembly);

app.MapQueryz();
```

The router also needs Queryz's assembly:

```razor
<ForgeRouter AppAssembly="typeof(Program).Assembly"
             AdditionalAssemblies="@( [typeof(Reports).Assembly] )"
             NotFoundPage="typeof(NotFound)" />
```

`IQueryzAuthorizationFilter` is required. With an empty list, nobody can open Queryz. Every registered filter must allow the caller. `AdministratorQueryzAuthorizationFilter` lets any signed-in person run reports and lets the Administrator role set them up. Replace it, or register more than one filter, when the host uses different roles.

The previous app expected three role names: Administrators, Report Builder Editors, and Report Builder Users. Editors could build reports, and only administrators could manage groups, data sources, and enumerations. The library no longer looks for those names. The host filter answers two questions: who may open Queryz, and who may manage it. Anyone who may manage can set up data sources, groups, enumerations, and reports. Who may run one report is whichever roles are selected on its group, using the host's own role names, followed by that report's user blacklist. A group with no roles is visible only to people who can manage.

## Who can see a report

People who can manage can run every report, including disabled ones, and are not blocked by the blacklist. A client sees a report when one of the group's role ids is a role they have, the report is enabled, and they are not on its blacklist. The raw SQL is shown on the preview page only to people who can manage.

## Support

If you find this project useful, consider supporting its development.

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_Logo.png"
         alt="Buy Me a Coffee"
         height="40">
  </a>&nbsp;&nbsp;&nbsp;
  <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=gordon_matt%40live%2ecom&lc=AU&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/PayPal_Logo.png"
         alt="Donate with PayPal"
         height="40">
  </a>
</p>

<p align="center">
  Prefer using your phone? Scan the QR code:
</p>

<p align="center">
  <a href="https://buymeacoffee.com/vnmatt">
    <img src="https://github.com/gordon-matt/MyResume/blob/gh-pages/assets/images/BuyMeACoffee_QR.png"
         alt="Buy Me a Coffee QR Code"
         width="180">
  </a>
</p>

<img src="https://komarev.com/ghpvc/?username=gordon-matt&label=GitHub%20Hits%20Since%202025-06-01%3A%20&color=ff0000&style=flat" alt="gordon-matt" />