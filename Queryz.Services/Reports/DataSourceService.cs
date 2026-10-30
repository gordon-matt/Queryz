namespace Queryz.Services.Reports;

public interface IDataSourceService
{
    Task<Result<IReadOnlyList<DataSourceDto>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<DataSourceDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<DataSourceDetailsDto>> GetDetailsAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<string>>> ListDatabasesAsync(int? id, SaveDataSourceRequest request, CancellationToken cancellationToken = default);

    Task<Result<DataSourceDto>> SaveAsync(int? id, SaveDataSourceRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<Result> TestAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class DataSourceService(
    ILogger<DataSourceService> logger,
    IUserContextService userContext,
    IRepository<DataSource> dataSources,
    IRepository<Report> reports) : IDataSourceService
{
    public async Task<Result<IReadOnlyList<DataSourceDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dataSources.FindAsync(new SearchOptions<DataSource>
        {
            OrderBy = query => query.OrderBy(source => source.Name),
            CancellationToken = cancellationToken,
        });

        return Result.Success<IReadOnlyList<DataSourceDto>>(rows.Select(ToDto).ToList());
    }

    public async Task<Result<DataSourceDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var source = await dataSources.FindOneAsync(id);
        return source is null
            ? Result<DataSourceDto>.NotFound()
            : Result.Success(ToDto(source));
    }

    public async Task<Result<DataSourceDetailsDto>> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var source = await dataSources.FindOneAsync(id);
        if (source is null)
        {
            return Result<DataSourceDetailsDto>.NotFound();
        }

        return Result.Success(new DataSourceDetailsDto(
            source.Id,
            source.Name,
            source.Provider,
            DataSourceConnections.Parse(source.Provider, source.ConnectionString),
            DataSourceConnections.HasPassword(source.Provider, source.ConnectionString)));
    }

    public async Task<Result<IReadOnlyList<string>>> ListDatabasesAsync(
        int? id,
        SaveDataSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Provider == ReportProvider.Sqlite)
        {
            return Result.Success<IReadOnlyList<string>>([]);
        }

        var prepared = await PrepareConnectionAsync(id, request, requireDatabase: false, cancellationToken);
        if (!prepared.IsSuccess)
        {
            return Result<IReadOnlyList<string>>.Invalid(prepared.ValidationErrors);
        }

        try
        {
            string catalogConnection = DataSourceConnections.ForCatalog(request.Provider, prepared.Value);
            using var connection = ReportSql.Open(request.Provider, catalogConnection);
            using var command = connection.CreateCommand();
            command.CommandText = request.Provider switch
            {
                ReportProvider.SqlServer => "SELECT name FROM sys.databases ORDER BY name",
                ReportProvider.PostgreSql => "SELECT datname FROM pg_database WHERE datallowconn AND NOT datistemplate ORDER BY datname",
                ReportProvider.MySql => "SHOW DATABASES",
                _ => throw new InvalidOperationException($"Unsupported report provider '{request.Provider}'."),
            };

            var names = new List<string>();
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                names.Add(reader.GetString(0));
            }

            return Result.Success<IReadOnlyList<string>>(names);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Databases could not be listed for provider {Provider}", request.Provider);
            return Result<IReadOnlyList<string>>.Error(ex.Message);
        }
    }

    public async Task<Result<DataSourceDto>> SaveAsync(
        int? id,
        SaveDataSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<DataSourceDto>.Invalid(new ValidationError("The name is required."));
        }

        var prepared = await PrepareConnectionAsync(id, request, requireDatabase: true, cancellationToken);
        if (!prepared.IsSuccess)
        {
            return Result<DataSourceDto>.Invalid(prepared.ValidationErrors);
        }

        string connectionString = DataSourceConnections.Build(request.Provider, prepared.Value);
        string name = request.Name.Trim();
        var duplicate = await dataSources.FindOneAsync(new SearchOptions<DataSource>
        {
            Query = source => source.Name == name && source.Id != (id ?? 0),
            CancellationToken = cancellationToken,
        });

        if (duplicate is not null)
        {
            return Result<DataSourceDto>.Invalid(new ValidationError("A data source with that name already exists."));
        }

        if (id is null)
        {
            var created = new DataSource
            {
                Name = name,
                Provider = request.Provider,
                ConnectionString = connectionString,
            };

            await dataSources.InsertAsync(created);
            logger.LogInformation("User {UserId} created data source {DataSourceId}", userContext.GetCurrentUserId(), created.Id);
            return Result.Success(ToDto(created));
        }

        var existing = await dataSources.FindOneAsync(id.Value);
        if (existing is null)
        {
            return Result<DataSourceDto>.NotFound();
        }

        existing.Name = name;
        existing.Provider = request.Provider;
        existing.ConnectionString = connectionString;
        await dataSources.UpdateAsync(existing);
        logger.LogInformation("User {UserId} updated data source {DataSourceId}", userContext.GetCurrentUserId(), existing.Id);
        return Result.Success(ToDto(existing));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        int used = await reports.CountAsync(report => report.DataSourceId == id);
        if (used > 0)
        {
            return Result.Invalid(new ValidationError("Reports still use this data source."));
        }

        int deleted = await dataSources.DeleteAsync(source => source.Id == id);
        if (deleted == 0)
        {
            return Result.NotFound();
        }

        logger.LogInformation("User {UserId} deleted data source {DataSourceId}", userContext.GetCurrentUserId(), id);
        return Result.Success();
    }

    public async Task<Result> TestAsync(int id, CancellationToken cancellationToken = default)
    {
        var source = await dataSources.FindOneAsync(id);
        if (source is null)
        {
            return Result.NotFound();
        }

        try
        {
            using var connection = ReportSql.Open(source.Provider, source.ConnectionString);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Data source {DataSourceId} could not be opened", id);
            return Result.Error(ex.Message);
        }
    }

    private async Task<Result<DataSourceConnectionRequest>> PrepareConnectionAsync(
        int? id,
        SaveDataSourceRequest request,
        bool requireDatabase,
        CancellationToken cancellationToken)
    {
        var connection = request.Connection;
        if (string.IsNullOrEmpty(connection.Password) && id is int existingId)
        {
            var existing = await dataSources.FindOneAsync(existingId);
            if (existing is not null)
            {
                connection.Password = DataSourceConnections.ReadPassword(existing.Provider, existing.ConnectionString) ?? string.Empty;
            }
        }

        if (request.Provider == ReportProvider.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(connection.FilePath))
            {
                return Result<DataSourceConnectionRequest>.Invalid(new ValidationError("The database file is required."));
            }

            return Result.Success(connection);
        }

        if (string.IsNullOrWhiteSpace(connection.Server))
        {
            return Result<DataSourceConnectionRequest>.Invalid(new ValidationError("The server is required."));
        }

        if (requireDatabase && string.IsNullOrWhiteSpace(connection.Database))
        {
            return Result<DataSourceConnectionRequest>.Invalid(new ValidationError("The database is required."));
        }

        if (request.Provider != ReportProvider.SqlServer || !connection.IntegratedSecurity)
        {
            if (string.IsNullOrWhiteSpace(connection.UserId) || string.IsNullOrEmpty(connection.Password))
            {
                return Result<DataSourceConnectionRequest>.Invalid(new ValidationError("The user and password are required."));
            }
        }

        return Result.Success(connection);
    }

    private static DataSourceDto ToDto(DataSource source) => new(source.Id, source.Name, source.Provider);
}
