namespace Queryz.Services.Reports;

public interface IEnumerationService
{
    Task<Result<IReadOnlyList<EnumerationDto>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<EnumerationDto>> SaveAsync(int? id, SaveEnumerationRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class EnumerationService(
    ILogger<EnumerationService> logger,
    IUserContextService userContext,
    IRepository<Enumeration> enumerations,
    IRepository<ReportColumn> columns) : IEnumerationService
{
    public async Task<Result<IReadOnlyList<EnumerationDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await enumerations.FindAsync(new SearchOptions<Enumeration>
        {
            OrderBy = query => query.OrderBy(item => item.Name),
            CancellationToken = cancellationToken,
        });

        return Result.Success<IReadOnlyList<EnumerationDto>>(rows.Select(ToDto).ToList());
    }

    public async Task<Result<EnumerationDto>> SaveAsync(
        int? id,
        SaveEnumerationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<EnumerationDto>.Invalid(new ValidationError("The name is required."));
        }

        var values = request.Values
            .Where(value => !string.IsNullOrWhiteSpace(value.Name))
            .Select(value => new EnumerationValueDto(value.Id, value.Name.Trim()))
            .ToList();

        if (values.GroupBy(value => value.Id).Any(group => group.Count() > 1))
        {
            return Result<EnumerationDto>.Invalid(new ValidationError("Each value needs its own number."));
        }

        string name = request.Name.Trim();
        var duplicate = await enumerations.FindOneAsync(new SearchOptions<Enumeration>
        {
            Query = item => item.Name == name && item.Id != (id ?? 0),
            CancellationToken = cancellationToken,
        });

        if (duplicate is not null)
        {
            return Result<EnumerationDto>.Invalid(new ValidationError("An enumeration with that name already exists."));
        }

        string json = EnumerationValues.Serialize(values);
        if (id is null)
        {
            var created = new Enumeration
            {
                Name = name,
                IsBitFlags = request.IsBitFlags,
                Values = json,
            };
            await enumerations.InsertAsync(created);
            logger.LogInformation("User {UserId} created enumeration {EnumerationId}", userContext.GetCurrentUserId(), created.Id);
            return Result.Success(ToDto(created));
        }

        var existing = await enumerations.FindOneAsync(id.Value);
        if (existing is null)
        {
            return Result<EnumerationDto>.NotFound();
        }

        existing.Name = name;
        existing.IsBitFlags = request.IsBitFlags;
        existing.Values = json;
        await enumerations.UpdateAsync(existing);
        logger.LogInformation("User {UserId} updated enumeration {EnumerationId}", userContext.GetCurrentUserId(), existing.Id);
        return Result.Success(ToDto(existing));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        int used = await columns.CountAsync(column => column.EnumerationId == id);
        if (used > 0)
        {
            return Result.Invalid(new ValidationError("A column still uses this enumeration."));
        }

        int deleted = await enumerations.DeleteAsync(item => item.Id == id);
        if (deleted == 0)
        {
            return Result.NotFound();
        }

        logger.LogInformation("User {UserId} deleted enumeration {EnumerationId}", userContext.GetCurrentUserId(), id);
        return Result.Success();
    }

    private static EnumerationDto ToDto(Enumeration item) =>
        new(item.Id, item.Name, item.IsBitFlags, EnumerationValues.Deserialize(item.Values));
}
