# Queryz

A Blazor Server app for saved SQL reports. Filters are built with [QueryKrafter](https://github.com/Heroicsoft/QueryKrafter) and turned into a parameterized WHERE clause on the server from the columns stored with the report.

The host follows the same shape as Snippetz: Interactive Server, NetForge shell, Identity (or Keycloak / none), Ardalis.Result services, and Extenso repositories. The app database is selected with `Database:Provider` (`Sqlite`, `SqlServer`, `Npgsql`, `MySql`). Report data sources are separate connections and can be any of those four as well.

## Run

```bash
dotnet run --project Queryz
```

The development seed signs in as `admin` / `Admin@123`. Change that password before sharing the instance.

## What a report stores

A report is one table, the columns to select, and QueryKrafter rules JSON. Running it reads the live schema, keeps only columns that still exist, and generates SQL from that configuration. The `field` values posted with the rules are not used.
