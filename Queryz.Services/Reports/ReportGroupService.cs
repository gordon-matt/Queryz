namespace Queryz.Services.Reports;

public interface IReportGroupService
{
    Task<Result<IReadOnlyList<ReportGroupDto>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<ReportGroupDto>> SaveAsync(int? id, SaveReportGroupRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public sealed class ReportGroupService(
    ILogger<ReportGroupService> logger,
    IUserContextService userContext,
    IRepository<ReportGroup> groups,
    IRepository<ReportGroupRole> roles,
    IRepository<Report> reports) : IReportGroupService
{
    public async Task<Result<IReadOnlyList<ReportGroupDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var rows = await groups.FindAsync(new SearchOptions<ReportGroup>
        {
            OrderBy = query => query.OrderBy(group => group.Name),
            CancellationToken = cancellationToken,
        });

        var assignments = await roles.FindAsync(new SearchOptions<ReportGroupRole>
        {
            CancellationToken = cancellationToken,
        });

        var byGroup = assignments
            .GroupBy(role => role.ReportGroupId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<string>)group.Select(role => role.RoleId).ToList());

        return Result.Success<IReadOnlyList<ReportGroupDto>>(rows
            .Select(group => new ReportGroupDto(
                group.Id,
                group.Name,
                group.TimeZoneId,
                byGroup.GetValueOrDefault(group.Id) ?? []))
            .ToList());
    }

    public async Task<Result<ReportGroupDto>> SaveAsync(
        int? id,
        SaveReportGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<ReportGroupDto>.Invalid(new ValidationError("The name is required."));
        }

        string name = request.Name.Trim();
        var duplicate = await groups.FindOneAsync(new SearchOptions<ReportGroup>
        {
            Query = group => group.Name == name && group.Id != (id ?? 0),
            CancellationToken = cancellationToken,
        });

        if (duplicate is not null)
        {
            return Result<ReportGroupDto>.Invalid(new ValidationError("A group with that name already exists."));
        }

        string timeZoneId = request.TimeZoneId.Trim();
        if (id is null)
        {
            var created = new ReportGroup { Name = name, TimeZoneId = timeZoneId };
            await groups.InsertAsync(created);
            await ReplaceRolesAsync(created.Id, request.RoleIds);
            logger.LogInformation("User {UserId} created report group {ReportGroupId}", userContext.GetCurrentUserId(), created.Id);
            return Result.Success(new ReportGroupDto(created.Id, created.Name, created.TimeZoneId, DistinctRoles(request.RoleIds)));
        }

        var existing = await groups.FindOneAsync(id.Value);
        if (existing is null)
        {
            return Result<ReportGroupDto>.NotFound();
        }

        existing.Name = name;
        existing.TimeZoneId = timeZoneId;
        await groups.UpdateAsync(existing);
        await ReplaceRolesAsync(existing.Id, request.RoleIds);
        logger.LogInformation("User {UserId} updated report group {ReportGroupId}", userContext.GetCurrentUserId(), existing.Id);
        return Result.Success(new ReportGroupDto(existing.Id, existing.Name, existing.TimeZoneId, DistinctRoles(request.RoleIds)));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        int used = await reports.CountAsync(report => report.GroupId == id);
        if (used > 0)
        {
            return Result.Invalid(new ValidationError("Reports still use this group."));
        }

        await roles.DeleteAsync(role => role.ReportGroupId == id);
        int deleted = await groups.DeleteAsync(group => group.Id == id);
        if (deleted == 0)
        {
            return Result.NotFound();
        }

        logger.LogInformation("User {UserId} deleted report group {ReportGroupId}", userContext.GetCurrentUserId(), id);
        return Result.Success();
    }

    private async Task ReplaceRolesAsync(int groupId, IEnumerable<string> roleIds)
    {
        await roles.DeleteAsync(role => role.ReportGroupId == groupId);
        var rows = DistinctRoles(roleIds)
            .Select(roleId => new ReportGroupRole { ReportGroupId = groupId, RoleId = roleId })
            .ToList();

        if (rows.Count > 0)
        {
            await roles.InsertAsync(rows);
        }
    }

    private static IReadOnlyList<string> DistinctRoles(IEnumerable<string> roleIds) =>
        roleIds
            .Where(roleId => !string.IsNullOrWhiteSpace(roleId))
            .Select(roleId => roleId.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
}
