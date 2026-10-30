# Queryz

Queryz is a Blazor Razor Class Library for saved SQL reports. Filters are built with [QueryKrafter](https://github.com/Heroicsoft/QueryKrafter) and turned into a parameterized WHERE clause on the server from the columns stored with the report.

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

`IQueryzAuthorizationFilter` is required. With an empty list, nobody can open Queryz. `AdministratorQueryzAuthorizationFilter` lets any signed-in person run reports and lets the Administrator role set them up. Replace it when the host uses different roles.

## Who can see a report

Administrators set up data sources, groups, enumerations, and reports, and they can run every report. A client sees a report when the group's roles include one of theirs. A group with no roles is visible only to administrators. The report blacklist still excludes a person who would otherwise be allowed.

## What a report stores

A report selects columns from one or more joined tables and stores QueryKrafter rules JSON. Running it shows the first 10 rows. Download exports the full result, still honoring a row limit saved on the report.
