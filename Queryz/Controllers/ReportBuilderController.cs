namespace Queryz.Controllers;

[Authorize]
[Route("report-builder")]
public class ReportBuilderController : Controller
{
    #region Private Members

    private readonly IDbContextFactory dbContextFactory;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<ApplicationRole> roleManager;
    private readonly IDataSourceService dataSourceService;
    private readonly IEnumerationService enumerationService;
    private readonly IRazorViewRenderService razorViewRenderService;
    private readonly IReportBuilderService reportBuilderService;
    private readonly IReportTableColumnService reportColumnService;
    private readonly IReportGroupService reportGroupService;
    private readonly IReportService reportService;
    private readonly IReportSortingService reportSortingService;
    private readonly IReportTableService reportTableService;
    private readonly IReportUserBlacklistService reportUserBlacklistService;
    private readonly IEnumerable<ITransformFunction> transformFunctions;

    #endregion Private Members

    #region Constructor

    public ReportBuilderController(
        IDbContextFactory dbContextFactory,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IRazorViewRenderService razorViewRenderService,
        IDataSourceService dataSourceService,
        IEnumerationService enumerationService,
        IReportGroupService reportGroupService,
        IReportService reportService,
        IReportSortingService reportSortingService,
        IReportTableColumnService reportColumnService,
        IReportTableService reportTableService,
        IReportUserBlacklistService reportUserBlacklistService,
        IEnumerable<ITransformFunction> transformFunctions,
        IReportBuilderService reportBuilderService)
    {
        this.dbContextFactory = dbContextFactory;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.razorViewRenderService = razorViewRenderService;

        this.dataSourceService = dataSourceService;
        this.enumerationService = enumerationService;
        this.reportColumnService = reportColumnService;
        this.reportGroupService = reportGroupService;
        this.reportService = reportService;
        this.reportSortingService = reportSortingService;
        this.reportTableService = reportTableService;
        this.reportUserBlacklistService = reportUserBlacklistService;
        this.transformFunctions = transformFunctions;

        this.reportBuilderService = reportBuilderService;
    }

    #endregion Constructor

    private IDictionary<int, string> ReportFilters
    {
        get
        {
            var filters = HttpContext.Session.GetObjectFromJson<Dictionary<int, string>>("ReportFilters");
            return filters ?? [];
        }

        set => HttpContext.Session.SetObjectAsJson("ReportFilters", value);
    }

    #region Action Methods

    [HttpPost]
    [Route("download/{id}")]
    public async Task<FileResult> Download(int id, DownloadOptions options)
    {
        var report = await reportService.FindOneAsync(new SearchOptions<Report>
        {
            Query = x => x.Id == id,
            Include = query => query
                .Include(x => x.Group)
                .Include(x => x.DataSource)
                .Include(x => x.Tables)
                .Include(x => x.Columns)
                .Include(x => x.Sortings)
        });

        report.Filters = null;
        string filters = ReportFilters[report.Id];

        if (filters != null)
        {
            report.Filters = filters;
        }

        if (report.DataSource.DataProvider == DataProvider.MySql)
        {
            report.Filters = report.Filters.Replace("\"", "`");
        }

        var result = reportBuilderService.ExecuteReport(report);

        switch (options.FileFormat)
        {
            case DownloadFileFormat.Delimited:
                {
                    string separator = null;
                    string contentType = null;
                    string fileExtension = null;

                    switch (options.Delimiter)
                    {
                        case DownloadFileDelimiter.Tab:
                            separator = "\t";
                            contentType = "text/tab-separated-values";
                            fileExtension = "tsv";
                            break;

                        case DownloadFileDelimiter.VerticalBar:
                            separator = "|";
                            contentType = "text/plain";
                            fileExtension = "txt";
                            break;

                        case DownloadFileDelimiter.Semicolon:
                            separator = ";";
                            contentType = "text/plain";
                            fileExtension = "txt";
                            break;

                        case DownloadFileDelimiter.Comma:
                        default:
                            separator = ",";
                            contentType = "text/csv";
                            fileExtension = "csv";
                            break;
                    }

                    string delimited = result.ToDelimited(
                        delimiter: separator,
                        outputColumnNames: options.OutputColumnNames,
                        alwaysEnquote: options.AlwaysEnquote);

                    return File(Encoding.UTF8.GetBytes(delimited), contentType, $"{report.Name}_{DateTime.Now:yyyy-MM-dd HH_mm_ss}.{fileExtension}");
                }
            case DownloadFileFormat.XLSX:
                {
                    byte[] bytes = result.ToXlsx();

                    // DAS-95: Alex Ooi requested brackets be removed from exported files due to some limitation in excel where
                    //  pivot tables won't work when there are brackets in the workbook's name.
                    string reportName = report.Name
                        .Replace("[", "(")
                        .Replace("]", ")");

                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportName}_{DateTime.Now:yyyy-MM-dd HH_mm_ss}.xlsx");
                }
            default: throw new ArgumentOutOfRangeException();
        }
    }

    [Route("get-columns/{reportId}/{tableName}")]
    public async Task<JsonResult> GetColumns(int reportId, string tableName)
    {
        if (reportId == 0)
        {
            return Json(new
            {
                Success = false,
                Message = "Report ID missing."
            });
        }

        try
        {
            var report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == reportId,
                Include = query => query.Include(x => x.DataSource)
            });

            using var connection = report.DataSource.DataProvider.GetConnection(report.DataSource.ConnectionString);
            var columnData = DbConnectionHelpers.GetColumnData(connection, report.DataSource, tableName);
            return Json(new
            {
                Success = true,
                Columns = columnData.OrderBy(x => x.OrdinalPosition).Select(x => x.ColumnName)
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [Route("get-connection-details/{dataSourceId}")]
    public async Task<JsonResult> GetConnectionDetails(int dataSourceId)
    {
        try
        {
            var dataSource = await dataSourceService.FindOneAsync(dataSourceId);

            if (dataSource == null)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Could not find specified data source"
                });
            }

            string connectionDetails;
            switch (dataSource.DataProvider)
            {
                case DataProvider.SqlServer:
                    {
                        var connectionStringBuilder = new SqlConnectionStringBuilder(dataSource.ConnectionString);
                        connectionDetails = new SqlServerConnectionBuilderModel
                        {
                            Server = connectionStringBuilder.DataSource,
                            Database = connectionStringBuilder.InitialCatalog,
                            IntegratedSecurity = connectionStringBuilder.IntegratedSecurity,
                            UserId = connectionStringBuilder.UserID,
                            Password = connectionStringBuilder.Password
                        }.JsonSerialize();
                    }
                    break;

                case DataProvider.PostgreSql:
                    {
                        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(dataSource.ConnectionString);
                        connectionDetails = new PostgreSqlConnectionBuilderModel
                        {
                            Server = connectionStringBuilder.Host,
                            Port = connectionStringBuilder.Port,
                            Database = connectionStringBuilder.Database,
                            UserId = connectionStringBuilder.Username,
                            Password = connectionStringBuilder.Password
                        }.JsonSerialize();
                    }
                    break;

                case DataProvider.MySql:
                    {
                        var connectionStringBuilder = new MySqlConnectionStringBuilder(dataSource.ConnectionString);
                        connectionDetails = new MySqlConnectionBuilderModel
                        {
                            Server = connectionStringBuilder.Server,
                            Port = connectionStringBuilder.Port,
                            Database = connectionStringBuilder.Database,
                            UserId = connectionStringBuilder.UserID,
                            Password = connectionStringBuilder.Password
                        }.JsonSerialize();
                    }
                    break;

                default: connectionDetails = null; break;
            }

            return Json(new
            {
                Success = true,
                ConnectionDetails = connectionDetails
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [Route("get-connection-details-ui/{dataProvider}")]
    public async Task<JsonResult> GetConnectionDetailsUI(DataProvider dataProvider)
    {
        try
        {
            string content = null;
            content = dataProvider switch
            {
                DataProvider.SqlServer => await razorViewRenderService.RenderToStringAsync("_ConnectionBuilderSqlServer", routeData: RouteData, useActionContext: true),
                DataProvider.PostgreSql => await razorViewRenderService.RenderToStringAsync("_ConnectionBuilderPostgreSql", routeData: RouteData, useActionContext: true),
                DataProvider.MySql => await razorViewRenderService.RenderToStringAsync("_ConnectionBuilderMySql", routeData: RouteData, useActionContext: true),
                _ => throw new NotSupportedException(),
            };
            return Json(new
            {
                Success = true,
                Html = content
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [Route("get-custom-properties/{dataSourceId}")]
    public async Task<JsonResult> GetCustomProperties(int dataSourceId)
    {
        try
        {
            if (dataSourceId == 0)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Data Source ID missing."
                });
            }

            var dataSource = await dataSourceService.FindOneAsync(dataSourceId);

            if (dataSource == null)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Data Source could not be found."
                });
            }

            return Json(new
            {
                Success = true,
                CustomProperties = dataSource.CustomProperties.IsNullOrEmpty()
                    ? null
                    : dataSource.CustomProperties.JsonDeserialize<Dictionary<string, string>>()
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [HttpPost]
    [Route("get-databases")]
    public JsonResult GetDatabases([FromBody] dynamic request)
    {
        try
        {
            DataProvider provider = request.provider;
            string connectionDetails = request.connectionDetails;

            string connectionString = provider.GetConnectionString(connectionDetails);

            using var connection = provider.GetConnection(connectionString);
            if (!connection.Validate(3))
            {
                return Json(new
                {
                    Success = false,
                    Message = "Could not validate connection. Please check and try again."
                });
            }

            return Json(new
            {
                Success = true,
                Databases = DbConnectionHelpers.GetDatabaseNames(connection)
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    //[OutputCache(Duration = 86400, VaryByParam = "none")]
    [Route("get-translations")]
    public JsonResult GetTranslations() =>
        // TODO: Use some translation service...
        Json(new
        {
            Create = "Create",
            Delete = "Delete",
            DeleteRecordConfirm = "Are you sure that you want to delete this record?",
            DeleteRecordError = "There was an error when deleting the record.",
            DeleteRecordSuccess = "Successfully deleted record.",
            Edit = "Edit",
            GetRecordError = "There was an error when retrieving the record.",
            InsertRecordError = "There was an error when inserting the record.",
            InsertRecordSuccess = "Successfully inserted record.",
            UpdateRecordError = "There was an error when updating the record.",
            UpdateRecordSuccess = "Successfully updated record.",
            Columns = new
            {
                Name = "Name"
            }
        });

    [Route("")]
    public IActionResult Index() => View();

    [HttpPost]
    [Route("preview")]
    public async Task<JsonResult> Preview([FromBody] ReportQueryModel model)
    {
        try
        {
            var report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == model.ReportId,
                Include = query => query
                    .Include(x => x.Group)
                    .Include(x => x.DataSource)
                    .Include(x => x.Tables)
                    .Include(x => x.Columns)
                    .Include(x => x.Sortings)
            });

            report.Filters = model.Query;

            if (report.DataSource.DataProvider == DataProvider.MySql)
            {
                report.Filters = report.Filters.Replace("\"", "`");
            }

            #region Save filters into session variable for later use in Download() action

            var reportFilters = ReportFilters;
            if (reportFilters.ContainsKey(report.Id))
            {
                reportFilters[report.Id] = model.Query;
            }
            else
            {
                reportFilters.Add(report.Id, model.Query);
            }
            ReportFilters = reportFilters;

            #endregion Save filters into session variable for later use in Download() action

            // Limit the numbers of rows for previewing on screen
            if (report.RowLimit.IsNullOrDefault() || report.RowLimit.Value > 10)
            {
                report.RowLimit = 10;
            }

            var previewModel = new PreviewModel
            {
                ReportId = report.Id,
                ReportName = report.Name,
                Data = reportBuilderService.ExecuteReport(report)
            };

            return Json(new
            {
                Success = true,
                Html = await razorViewRenderService.RenderToStringAsync("Preview", previewModel, routeData: RouteData, useActionContext: true)
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [Route("run-report/{id}")]
    public async Task<IActionResult> RunReport(int id)
    {
        var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);
        bool canView = await CheckUserHasAccessToReportAsync(id, user.Id);
        if (!canView)
        {
            return Unauthorized();
        }

        var report = await reportService.FindOneAsync(new SearchOptions<Report>
        {
            Query = x => x.Id == id,
            Include = query => query
                .Include(x => x.Group)
                .Include(x => x.DataSource)
                .Include(x => x.Tables)
                .Include(x => x.Columns)
        });

        var queryBuilderFilters = GetQueryBuilderFilters(report);

        var model = new RunReportModel
        {
            ReportId = report.Id,
            ReportName = $"{report.Group.Name} - {report.Name}",
            JQQueryBuilderConfig = new JQQueryBuilderConfig
            {
                Plugins = new Dictionary<string, object>
                {
                    { "sortable", null },
                    { "unique-filter", null },
                    { "invert", null },
                    { "not-group", null },
                    { "sql-support", JObject.FromObject(new { boolean_as_integer = false }) }
                },
                Filters = queryBuilderFilters.Item2
            },
            JQQueryBuilderFieldIdMappings = queryBuilderFilters.Item1
        };

        string query = report.Filters;

        string[] tableNames = report.Columns
            .Where(x => !x.IsLiteral)
            .Select(x => x.Name.LeftOf('.'))
            .Distinct()
            .ToArray();

        foreach (string tableName in tableNames)
        {
            query = query.Replace($"\"{tableName}\".\"", $"\"{tableName}_");
        }

        model.Query = query == "null" ? null : query;

        ViewBag.Title = $"{report.Group.Name} - {report.Name}";

        return View(model);
    }

    //[HttpPost]
    //[Route("search-column-values")]
    //public async Task<JsonResult> SearchColumnValues(int reportId, string table, string idColumn, string nameColumn, string query, int pageIndex = 0)
    //{
    //    if (reportId == 0)
    //    {
    //        return Json(new
    //        {
    //            Success = false,
    //            Message = "Report ID missing."
    //        });
    //    }

    //    try
    //    {
    //        var report = await reportService.FindOneAsync(x => x.Id == reportId, include => include.DataSource);

    //        if (report == null)
    //        {
    //            return Json(new
    //            {
    //                Success = false,
    //                Message = "Report could not be found."
    //            });
    //        }

    //        var queryBuilder = report.DataSource.GetSelectQueryBuilder()
    //            .Select(table, idColumn, nameColumn)
    //            .From(table)
    //            .Where(table, nameColumn, ComparisonOperator.StartsWith, query)
    //            .OrderBy(table, nameColumn, SortDirection.Ascending)
    //            .Take(30); // TODO: Make this configurable?

    //        using (var connection = report.DataSource.DataProvider.GetConnection(report.DataSource.ConnectionString))
    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandTimeout = 300;
    //            command.CommandType = CommandType.Text;

    //            if (connection is SqlConnection)
    //            {
    //                // customer requested this transaction be set to prevent SQL implicit transaction, which blocks writes
    //                command.CommandText = string.Format(@"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED BEGIN TRANSACTION; {0} COMMIT TRANSACTION;", queryBuilder.BuildQuery());
    //            }
    //            else
    //            {
    //                command.CommandText = queryBuilder.BuildQuery();
    //            }

    //            var results = new TupleList<string, string>();

    //            await connection.OpenAsync();
    //            using (var reader = await command.ExecuteReaderAsync())
    //            {
    //                while (await reader.ReadAsync())
    //                {
    //                    results.Add(reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
    //                }
    //            }
    //            connection.Close();

    //            return Json(new
    //            {
    //                Success = true,
    //                Results = results.Select(x => new
    //                {
    //                    Id = x.Item1,
    //                    Name = x.Item2
    //                }),
    //                TotalCount = 30 //TODO: provide real count
    //            });
    //        }
    //    }
    //    catch (Exception x)
    //    {
    //        return Json(new
    //        {
    //            Success = false,
    //            Message = x.GetBaseException().Message
    //        });
    //    }
    //}

    #region Wizard

    [HttpPost]
    [Route("save-wizard-step-1")]
    public async Task<JsonResult> SaveWizardStep1([FromBody] WizardStep1Model model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new
            {
                Success = false,
                Message = ModelState.Values.First(x => x.Errors.Count > 0).Errors.First().ErrorMessage
            });
        }

        try
        {
            bool isNew = model.Id == 0;
            Report report;

            if (!isNew)
            {
                report = await reportService.FindOneAsync(new SearchOptions<Report>
                {
                    Query = x => x.Id == model.Id,
                    Include = query => query.Include(x => x.Tables)
                });

                report.Name = model.Name;
                report.GroupId = model.GroupId;
                report.DataSourceId = model.DataSourceId;
                report.LastModifiedUtc = DateTime.UtcNow;
                report.Enabled = model.Enabled;
                report.EmailEnabled = model.EmailEnabled;

                await reportService.UpdateAsync(report);
            }
            else
            {
                report = new Report
                {
                    Name = model.Name,
                    GroupId = model.GroupId,
                    DataSourceId = model.DataSourceId,
                    LastModifiedUtc = DateTime.UtcNow,
                    Enabled = model.Enabled,
                    EmailEnabled = model.EmailEnabled
                };

                await reportService.InsertAsync(report);
            }

            var dataSource = report.DataSource ?? await dataSourceService.FindOneAsync(report.DataSourceId);

            var tables = Enumerable.Empty<string>();
            using (var connection = dataSource.DataProvider.GetConnection(dataSource.ConnectionString))
            {
                tables = DbConnectionHelpers.GetTableNames(connection, dataSource, true);
            }

            return Json(new
            {
                Success = true,
                Model = new
                {
                    ReportId = report.Id,
                    AvailableTables = tables,
                    SelectedTables = report.Tables.Select(x => x.Name)
                }
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [HttpPost]
    [Route("save-wizard-step-2")]
    public async Task<JsonResult> SaveWizardStep2([FromBody] WizardStep2Model model)
    {
        if (model.ReportId == 0)
        {
            return Json(new
            {
                Success = false,
                Message = "Report ID missing."
            });
        }

        try
        {
            var report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == model.ReportId,
                Include = query => query
                    .Include(x => x.DataSource)
                    .Include(x => x.Tables)
                    .Include(x => x.Columns)
            });

            string[] existingTableNames = report.Tables.Select(x => x.Name).ToArray();

            var toInsert = model.SelectedTables.Where(x => !existingTableNames.Contains(x)).Select(x => new ReportTable
            {
                ReportId = report.Id,
                Name = x
            });

            await reportTableService.DeleteAsync(x => x.ReportId == report.Id && !model.SelectedTables.Contains(x.Name));
            await reportTableService.InsertAsync(toInsert);

            var columns = new List<ColumnModel>();
            using (var connection = report.DataSource.DataProvider.GetConnection(report.DataSource.ConnectionString))
            {
                foreach (string tableName in model.SelectedTables)
                {
                    var columnData = DbConnectionHelpers.GetColumnData(connection, report.DataSource, tableName);
                    var foreignKeyData = DbConnectionHelpers.GetForeignKeyData(connection, report.DataSource, tableName);

                    foreach (var columnInfo in columnData.OrderBy(x => x.OrdinalPosition))
                    {
                        var columnModel = new ColumnModel
                        {
                            ColumnName = $"{tableName}.{columnInfo.ColumnName}",
                            Type = DataTypeConvertor.GetSystemType(columnInfo.DataType).Name
                        };

                        if (columnInfo.KeyType == KeyType.ForeignKey)
                        {
                            columnModel.IsForeignKey = true;

                            var fkInfo = foreignKeyData.FirstOrDefault(x => x.ForeignKeyColumn == columnInfo.ColumnName);

                            if (fkInfo != null)
                            {
                                var parentColumns = DbConnectionHelpers.GetColumnData(connection, report.DataSource, fkInfo.PrimaryKeyTable);
                                columnModel.AvailableParentColumns = parentColumns.OrderBy(x => x.OrdinalPosition).Select(x => x.ColumnName);
                            }
                        }

                        columns.Add(columnModel);
                    }
                }
            }

            SelectedColumnModel[] selectedColumns = null;

            if (!report.Columns.IsNullOrEmpty())
            {
                selectedColumns = report.Columns.OrderBy(x => x.Ordinal).Select(x => new SelectedColumnModel
                {
                    ColumnName = x.Name,
                    Type = x.IsLiteral ? "String" : columns.First(y => y.ColumnName == x.Name).Type,
                    Alias = x.Alias,
                    IsLiteral = x.IsLiteral,
                    EnumerationId = x.EnumerationId,
                    TransformFunction = x.TransformFunction,
                    Format = x.Format,
                    IsHidden = x.IsHidden
                }).ToArray();
            }
            else
            {
                // If there are no columns selected (example: this is a new report), then we auto select ALL
                //  available columns by default to make it easier for the user.
                selectedColumns = columns
                    .Select(x => new SelectedColumnModel
                    {
                        ColumnName = x.ColumnName,
                        Type = x.Type,
                        Alias = x.ColumnName.Contains(" ")
                            ? x.ColumnName.RightOf('.').Trim().Replace("  ", " ")
                            : x.ColumnName.RightOf('.').SplitPascal().Trim()
                    })
                    .ToArray();
            }

            EnumerationModel[] availableEnumerations;
            using (var connection = enumerationService.OpenConnection())
            {
                availableEnumerations = connection.Query()
                    .Select(x => new EnumerationModel
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                    .OrderBy(x => x.Name)
                    .ToArray();
            }

            return Json(new
            {
                Success = true,
                Model = new WizardStep3Model
                {
                    ReportId = report.Id,
                    AvailableColumns = columns.OrderBy(x => x.ColumnName).ToArray(),
                    SelectedColumns = selectedColumns,
                    AvailableEnumerations = availableEnumerations,
                    AvailableTransformFunctions = transformFunctions.Select(y => y.Name)
                }
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [HttpPost]
    [Route("save-wizard-step-3")]
    public async Task<JsonResult> SaveWizardStep3([FromBody] WizardStep3Model model)
    {
        if (model.ReportId == 0)
        {
            return Json(new
            {
                Success = false,
                Message = "Report ID missing."
            });
        }

        try
        {
            var report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == model.ReportId,
                Include = query => query
                    .Include(x => x.DataSource)
                    .Include(x => x.Tables)
                    .Include(x => x.Columns)
            });

            var selectedColumns = model.SelectedColumns.Select((x, i) => new
            {
                Column = x,
                Ordinal = i
            });

            #region Non-Literals

            string[] existingColumnNames = report.Columns
                .Where(x => !x.IsLiteral)
                .Select(x => x.Name)
                .ToArray();

            var toInsert = selectedColumns
                .Where(x =>
                    !x.Column.IsLiteral &&
                    !existingColumnNames.Contains(x.Column.ColumnName))
                .Select(x => new ReportTableColumn
                {
                    ReportId = report.Id,
                    Name = x.Column.ColumnName,
                    Alias = string.IsNullOrWhiteSpace(x.Column.Alias) ? null : x.Column.Alias,
                    IsLiteral = false,
                    Ordinal = x.Ordinal,
                    IsForeignKey = x.Column.IsForeignKey,
                    DisplayColumn = x.Column.DisplayColumn,
                    EnumerationId = x.Column.EnumerationId,
                    TransformFunction = x.Column.TransformFunction,
                    Format = x.Column.Format,
                    IsHidden = x.Column.IsHidden
                });

            var selectedColumnNames = selectedColumns.Select(y => y.Column.ColumnName);
            await reportColumnService.DeleteAsync(x => x.ReportId == report.Id && !x.IsLiteral && !selectedColumnNames.Contains(x.Name));
            await reportColumnService.InsertAsync(toInsert);

            var toUpdate = report.Columns.Where(x => !x.IsLiteral && existingColumnNames.Contains(x.Name));

            foreach (var item in toUpdate)
            {
                var toUpdateWith = selectedColumns.First(x => x.Column.ColumnName == item.Name);

                item.Alias = toUpdateWith.Column.Alias;
                item.Ordinal = toUpdateWith.Ordinal;
                item.IsForeignKey = toUpdateWith.Column.IsForeignKey;
                item.DisplayColumn = toUpdateWith.Column.DisplayColumn;
                item.EnumerationId = toUpdateWith.Column.EnumerationId;
                item.TransformFunction = toUpdateWith.Column.TransformFunction;
                item.Format = toUpdateWith.Column.Format;
                item.IsHidden = toUpdateWith.Column.IsHidden;
            }
            await reportColumnService.UpdateAsync(toUpdate);

            #endregion Non-Literals

            #region Literals

            existingColumnNames = report.Columns
                .Where(x => x.IsLiteral)
                .Select(x => x.Alias)
                .ToArray();

            toInsert = selectedColumns
                .Where(x =>
                    x.Column.IsLiteral &&
                    !existingColumnNames.Contains(x.Column.Alias))
                .Select(x => new ReportTableColumn
                {
                    ReportId = report.Id,
                    Name = x.Column.ColumnName,
                    Alias = x.Column.Alias,
                    IsLiteral = true,
                    Ordinal = x.Ordinal,
                    IsForeignKey = false,
                    DisplayColumn = null,
                    EnumerationId = x.Column.EnumerationId,
                    TransformFunction = x.Column.TransformFunction,
                    Format = x.Column.Format,
                    IsHidden = x.Column.IsHidden
                });

            var selectedColumnAliases = selectedColumns.Select(y => y.Column.Alias);
            await reportColumnService.DeleteAsync(x => x.ReportId == report.Id && x.IsLiteral && !selectedColumnAliases.Contains(x.Alias));
            await reportColumnService.InsertAsync(toInsert);

            toUpdate = report.Columns.Where(x => x.IsLiteral && existingColumnNames.Contains(x.Alias));
            foreach (var item in toUpdate)
            {
                var toUpdateWith = selectedColumns.First(x => x.Column.Alias == item.Alias);

                item.Name = toUpdateWith.Column.ColumnName;
                item.Ordinal = toUpdateWith.Ordinal;
                item.EnumerationId = toUpdateWith.Column.EnumerationId;
                item.TransformFunction = toUpdateWith.Column.TransformFunction;
                item.Format = toUpdateWith.Column.Format;
                item.IsHidden = toUpdateWith.Column.IsHidden;
            }
            await reportColumnService.UpdateAsync(toUpdate);

            #endregion Literals

            // Refresh, so we have the columns to use for query builder.
            report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == model.ReportId,
                Include = query => query
                    .Include(x => x.DataSource)
                    .Include(x => x.Tables)
                    .Include(x => x.Columns)
            });

            var queryBuilderFilters = GetQueryBuilderFilters(report);

            var step4Model = new WizardStep4Model
            {
                ReportId = report.Id,
                JQQueryBuilderConfig = new JQQueryBuilderConfig
                {
                    Plugins = new Dictionary<string, object>
                    {
                        { "sortable", null },
                        { "unique-filter", null },
                        { "invert", null },
                        { "not-group", null },
                        { "sql-support", JObject.FromObject(new { boolean_as_integer = false }) }
                    },
                    Filters = queryBuilderFilters.Item2
                },
                JQQueryBuilderFieldIdMappings = queryBuilderFilters.Item1
            };

            string query = report.Filters;

            if (!string.IsNullOrEmpty(query))
            {
                string[] tableNames = report.Columns
                    .Where(x => !x.IsLiteral)
                    .Select(x => x.Name.LeftOf('.'))
                    .Distinct()
                    .ToArray();

                foreach (string tableName in tableNames)
                {
                    query = query.Replace($"\"{tableName}\".\"", $"\"{tableName}_");
                }

                step4Model.Query = query;
            }

            return Json(new
            {
                Success = true,
                Model = step4Model
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [HttpPost]
    [Route("save-wizard-step-4")]
    public async Task<JsonResult> SaveWizardStep4([FromBody] ReportQueryModel model)
    {
        if (model.ReportId == 0)
        {
            return Json(new
            {
                Success = false,
                Message = "Report ID missing."
            });
        }

        try
        {
            var report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == model.ReportId,
                Include = query => query
                    .Include(x => x.DataSource)
                    .Include(x => x.Tables)
                    .Include(x => x.Columns)
                    .Include(x => x.Sortings)
            });

            report.Filters = model.Query;

            await reportService.UpdateAsync(report);

            return Json(new
            {
                Success = true,
                Model = new WizardStep5Model
                {
                    ReportId = report.Id,
                    AvailableColumns = report.Columns.OrderBy(x => x.Name).Select(x => x.Name),
                    Sortings = report.Sortings
                    .OrderBy(x => x.Ordinal)
                        .Select(x => new SortingModel
                        {
                            ColumnName = x.ColumnName,
                            SortDirection = x.SortDirection
                        })
                }
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [HttpPost]
    [Route("save-wizard-step-5")]
    public async Task<JsonResult> SaveWizardStep5([FromBody] WizardStep5Model model)
    {
        if (model.ReportId == 0)
        {
            return Json(new
            {
                Success = false,
                Message = "Report ID missing."
            });
        }

        try
        {
            var report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == model.ReportId,
                Include = query => query
                    .Include(x => x.DataSource)
                    .Include(x => x.Tables)
                    .Include(x => x.Sortings)
            });

            await reportSortingService.DeleteAsync(x => x.ReportId == report.Id);

            if (!model.Sortings.IsNullOrEmpty())
            {
                var sortings = model.Sortings.Select((x, i) => new
                {
                    Sorting = x,
                    Ordinal = (byte)i
                });

                await reportSortingService.InsertAsync(sortings.Select(x => new ReportSorting
                {
                    ReportId = model.ReportId,
                    ColumnName = x.Sorting.ColumnName,
                    SortDirection = x.Sorting.SortDirection,
                    Ordinal = x.Ordinal
                }));
            }

            var relationships = Enumerable.Empty<RelationshipModel>();
            using (var connection = report.DataSource.DataProvider.GetConnection(report.DataSource.ConnectionString))
            {
                relationships = report.Tables.Select(x => new RelationshipModel
                {
                    TableName = x.Name,
                    ParentTable = x.ParentTable,
                    PrimaryKey = x.PrimaryKeyColumn,
                    ForeignKey = x.ForeignKeyColumn,
                    JoinType = x.JoinType,
                    AvailableColumns = DbConnectionHelpers.GetColumnData(connection, report.DataSource, x.Name).OrderBy(y => y.OrdinalPosition).Select(y => y.ColumnName)
                }).ToList();
            }

            var tableNames = report.Tables.Select(x => x.Name);
            foreach (var relationship in relationships)
            {
                relationship.AvailableParentTables = tableNames.Except(new[] { relationship.TableName });
            }

            return Json(new
            {
                Success = true,
                Model = new WizardStep6Model
                {
                    ReportId = report.Id,
                    Relationships = relationships
                }
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [HttpPost]
    [Route("save-wizard-step-6")]
    public async Task<JsonResult> SaveWizardStep6([FromBody] WizardStep6Model model)
    {
        if (model.ReportId == 0)
        {
            return Json(new
            {
                Success = false,
                Message = "Report ID missing."
            });
        }

        try
        {
            var report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == model.ReportId,
                Include = query => query.Include(x => x.Tables)
            });

            var toUpdate = new List<ReportTable>();
            foreach (var table in report.Tables)
            {
                var tableModel = model.Relationships.First(x => x.TableName == table.Name);
                table.ParentTable = tableModel.ParentTable;
                table.PrimaryKeyColumn = tableModel.PrimaryKey;
                table.ForeignKeyColumn = tableModel.ForeignKey;
                table.JoinType = tableModel.JoinType;
                toUpdate.Add(table);
            }
            await reportTableService.UpdateAsync(toUpdate);

            var deniedUserIds = (await reportUserBlacklistService
                .FindAsync(new SearchOptions<ReportUserBlacklistEntry>
                {
                    Query = x => x.ReportId == model.ReportId
                },
                projection => new
                {
                    projection.UserId
                }))
                .Select(x => x.UserId);

            return Json(new
            {
                Success = true,
                Model = new WizardStep7Model
                {
                    ReportId = report.Id,
                    IsDistinct = report.IsDistinct,
                    RowLimit = report.RowLimit.HasValue ? report.RowLimit.Value : 0,
                    EnumerationHandling = report.EnumerationHandling,
                    AvailableUsers = (await GetAvailableUsersAsync(report.GroupId)).ToArray(),
                    DeniedUserIds = deniedUserIds.ToArray()
                }
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [HttpPost]
    [Route("save-wizard-step-7")]
    public async Task<JsonResult> SaveWizardStep7([FromBody] WizardStep7Model model)
    {
        if (model.ReportId == 0)
        {
            return Json(new
            {
                Success = false,
                Message = "Report ID missing."
            });
        }

        try
        {
            var report = await reportService.FindOneAsync(model.ReportId);
            report.IsDistinct = model.IsDistinct;
            report.RowLimit = model.RowLimit;
            report.EnumerationHandling = model.EnumerationHandling;
            await reportService.UpdateAsync(report);

            if (!model.DeniedUserIds.IsNullOrEmpty())
            {
                var existingUserIds = (await reportUserBlacklistService
                    .FindAsync(new SearchOptions<ReportUserBlacklistEntry>
                    {
                        Query = x => x.ReportId == model.ReportId
                    },
                    projection => new
                    {
                        projection.UserId
                    }))
                    .Select(x => x.UserId);

                var toInsert = model.DeniedUserIds.Where(x => !existingUserIds.Contains(x));
                var toDelete = existingUserIds.Where(x => !model.DeniedUserIds.Contains(x));

                if (!toInsert.IsNullOrEmpty())
                {
                    await reportUserBlacklistService.InsertAsync(toInsert.Select(x => new ReportUserBlacklistEntry { ReportId = model.ReportId, UserId = x }));
                }

                if (!toDelete.IsNullOrEmpty())
                {
                    await reportUserBlacklistService.DeleteAsync(x => x.ReportId == model.ReportId && toDelete.Contains(x.UserId));
                }
            }

            return Json(new
            {
                Success = true
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    [Route("start-wizard/{reportId}")]
    public async Task<JsonResult> StartWizard(int reportId)
    {
        try
        {
            var report = await reportService.FindOneAsync(new SearchOptions<Report>
            {
                Query = x => x.Id == reportId,
                Include = query => query.Include(x => x.DataSource)
            });

            if (report == null)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Could not find specified report"
                });
            }

            return Json(new
            {
                Success = true,
                Model = new WizardStep1Model
                {
                    Id = report.Id,
                    Name = report.Name,
                    GroupId = report.GroupId,
                    DataSourceId = report.DataSourceId,
                    Enabled = report.Enabled,
                    EmailEnabled = report.EmailEnabled
                }
            });
        }
        catch (Exception x)
        {
            return Json(new
            {
                Success = false,
                Message = x.GetBaseException().Message
            });
        }
    }

    private async Task<IEnumerable<IdNamePair<string>>> GetAvailableUsersAsync(int reportGroupId)
    {
        using var context = (ApplicationDbContext)dbContextFactory.GetContext();
        string[] roleIds = context.ReportGroupRoles
            .Where(x => x.ReportGroupId == reportGroupId)
            .Select(x => x.RoleId)
            .ToArray();

        //var roles = context.Roles.Where(x => roleIds.Contains(x.Id)).ToList();

        var availableUsers = new List<IdNamePair<string>>();
        foreach (string roleId in roleIds)
        {
            var role = await roleManager.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == roleId);
            var userIds = role.Users.Select(x => x.Id).ToList();
            var users = await userManager.Users.Where(x => userIds.Contains(x.Id)).ToHashSetAsync();

            foreach (var user in users)
            {
                if (!availableUsers.Any(x => x.Id == user.Id))
                {
                    availableUsers.Add(new IdNamePair<string> { Id = user.Id, Name = user.UserName });
                }
            }
        }

        return availableUsers.OrderBy(x => x.Name);
    }

    #endregion Wizard

    #endregion Action Methods

    #region Non-Public Methods

    private async Task<bool> CheckUserHasAccessToReportAsync(int reportId, string userId) =>
        await reportUserBlacklistService.FindOneAsync(new SearchOptions<ReportUserBlacklistEntry>
        {
            Query = x => x.ReportId == reportId && x.UserId == userId
        }) == null;

    private Tuple<IDictionary<string, string>, IEnumerable<JQQueryBuilderFilter>> GetQueryBuilderFilters(Report report)
    {
        var result = new HashSet<JQQueryBuilderFilter>();

        string[] tableNames = report.Columns
            .Where(x => !x.IsLiteral)
            .Select(x => x.Name.LeftOf('.'))
            .Distinct()
            .ToArray();

        var tableData = new Dictionary<string, ColumnInfoCollection>();
        using (var connection = report.DataSource.DataProvider.GetConnection(report.DataSource.ConnectionString))
        {
            foreach (string tableName in tableNames)
            {
                tableData.Add(tableName, DbConnectionHelpers.GetColumnData(connection, report.DataSource, tableName));
            }
        }

        var fieldIdMappings = new Dictionary<string, string>();

        foreach (var column in report.Columns)
        {
            var filter = new JQQueryBuilderFilter();

            if (column.IsLiteral)
            {
                // TODO: What if more than 1 column has same alias? For now, user should not do that. In future, maybe show error on UI when alias used more than once.
                string id = $"{column.Alias.ToLowerInvariant()}";
                string field = $"{column.Alias.EnquoteDouble()}";

                filter.Id = id;
                filter.OptionGroup = "_Other";
                filter.Field = field;
                filter.Label = column.Alias;
                filter.Type = JQQueryBuilderFieldType.String;
                filter.Operators = JQQueryBuilderConfig.AllOperatorTypes.Value;
            }
            else
            {
                string tableName = column.Name.LeftOf('.');
                string columnName = column.Name.RightOf('.');

                string id = $"{tableName.ToLowerInvariant()}-{columnName.ToLowerInvariant()}";
                string field = $"{tableName.EnquoteDouble()}.{columnName.EnquoteDouble()}";

                fieldIdMappings.Add($"{tableName}_{columnName}", id);

                filter.Id = id;
                filter.OptionGroup = tableName;
                filter.Field = field;
                filter.Label = string.IsNullOrEmpty(column.Alias) ? columnName : column.Alias;

                var columnData = tableData[tableName];
                var columnInfo = columnData.First(x => x.ColumnName == columnName);
                string type = DataTypeConvertor.GetSystemType(columnInfo.DataType).Name;

                if (type == "String")
                {
                    filter.Type = JQQueryBuilderFieldType.String;
                    filter.ValueSeparator = ";"; // TODO: Test and also make configurable

                    filter.Operators = columnInfo.MaximumLength < 256
                        ? JQQueryBuilderConfig.ShortTextOperatorTypes.Value
                        : JQQueryBuilderConfig.LongTextOperatorTypes.Value;
                }
                else
                {
                    switch (type)
                    {
                        case "Boolean":
                            {
                                filter.Type = JQQueryBuilderFieldType.Boolean;
                                filter.Operators = JQQueryBuilderConfig.BooleanOperatorTypes.Value;
                            }
                            break;

                        case "DateTime":
                            {
                                filter.Type = JQQueryBuilderFieldType.Date;
                                filter.Operators = JQQueryBuilderConfig.DateTimeOperatorTypes.Value;
                                filter.Plugin = "datepicker";
                                filter.PluginConfig = new
                                {
                                    format = "yyyy-mm-dd",
                                    todayBtn = "linked",
                                    todayHighlight = true,
                                    autoclose = true
                                };
                            }
                            break;

                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "UInt16":
                        case "UInt32":
                        case "UInt64":
                        case "Byte":
                        case "SByte":
                            {
                                filter.Type = JQQueryBuilderFieldType.Integer;

                                if (column.EnumerationId.HasValue)
                                {
                                    var enumerations = enumerationService.FindOne(column.EnumerationId);
                                    var availableEnumerations = enumerations.Values.JsonDeserialize<EnumerationModel[]>();

                                    filter.Input = JQQueryBuilderInputType.Checkbox;
                                    filter.Values = availableEnumerations.ToDictionary(k => k.Id, v => v.Name);
                                    filter.Operators = JQQueryBuilderConfig.EnumOperatorTypes.Value;
                                }
                                else
                                {
                                    filter.Operators = JQQueryBuilderConfig.NumericOperatorTypes.Value;
                                }
                            }
                            break;

                        case "Single":
                        case "Double":
                        case "Decimal":
                            {
                                filter.Type = JQQueryBuilderFieldType.Double;
                                filter.Operators = JQQueryBuilderConfig.NumericOperatorTypes.Value;
                            }
                            break;

                        default:
                            {
                                filter.Type = JQQueryBuilderFieldType.String;
                                filter.Operators = JQQueryBuilderConfig.AllOperatorTypes.Value;
                            }
                            break;
                    }
                }
            }

            result.Add(filter);
        }

        return new Tuple<IDictionary<string, string>, IEnumerable<JQQueryBuilderFilter>>(fieldIdMappings, result.OrderBy(x => x.Label));
    }

    #endregion Non-Public Methods
}