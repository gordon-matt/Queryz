using Microsoft.AspNetCore.Mvc;

namespace Queryz.Infrastructure;

public static class ReportExportEndpoint
{
    public static WebApplication MapReportExport(this WebApplication app)
    {
        // Auth cookies are SameSite, so a cross-site page cannot post here. The interactive
        // circuit's antiforgery token is issued for a different identity and would be rejected.
        app.MapPost("/reports/{id:int}/export", ExportAsync).RequireAuthorization().DisableAntiforgery();
        return app;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> ExportAsync(
        int id,
        [FromForm] ReportDownloadRequest request,
        IReportService reports,
        CancellationToken cancellationToken)
    {
        var ran = await reports.RunAsync(id, preview: false, cancellationToken);
        if (!ran.IsSuccess)
        {
            if (ran.Status == ResultStatus.NotFound)
            {
                return Results.NotFound();
            }

            if (ran.Status == ResultStatus.Forbidden)
            {
                return Results.Forbid();
            }

            return Results.BadRequest(ran.GetDisplayMessage());
        }

        var file = ReportExport.Create(ran.Value.Name, ran.Value, request);
        return Results.File(file.Bytes, file.ContentType, file.FileName);
    }
}
