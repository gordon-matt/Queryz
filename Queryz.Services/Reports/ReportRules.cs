using QueryKrafter.Models;
using QueryKrafter.Serialization;

namespace Queryz.Services.Reports;

/// <summary>
/// Keeps designer-only filters in the query. A runner can change the conditions they can see.
/// Hidden rules, and any rule on a column marked hidden, are taken from the saved report and
/// applied again so a posted query cannot drop them.
/// </summary>
internal static class ReportRules
{
    public static string Enforce(string? savedJson, string? submittedJson, IReadOnlySet<string> adminFields)
    {
        var saved = Parse(savedJson);
        var submitted = submittedJson is null ? saved.Clone() : Parse(submittedJson);

        var locked = saved.AllRules()
            .Where(rule => IsAdminRule(rule, adminFields))
            .Select(rule =>
            {
                var copy = rule.Clone();
                copy.Hidden = true;
                return (QueryNode)copy;
            })
            .ToList();

        RemoveAdminRules(submitted, adminFields);
        if (locked.Count == 0)
        {
            return QueryJson.Serialize(submitted);
        }

        if (submitted.Rules.Count == 0)
        {
            return QueryJson.Serialize(new QueryGroup
            {
                Condition = QueryKrafter.Constants.Conditions.And,
                Rules = locked,
            });
        }

        return QueryJson.Serialize(new QueryGroup
        {
            Condition = QueryKrafter.Constants.Conditions.And,
            Rules = [submitted, .. locked],
        });
    }

    /// <summary>The rules the runner edits. Conditions on hidden columns are marked hidden.</summary>
    public static QueryGroup ForRunner(string? rulesJson, IEnumerable<ReportColumnDto> columns)
    {
        var rules = Parse(rulesJson);
        var adminFields = AdminFields(columns);
        foreach (var rule in rules.AllRules())
        {
            if (rule.Id is not null && adminFields.Contains(rule.Id))
            {
                rule.Hidden = true;
            }
        }

        return rules;
    }

    /// <summary>Drops designer-only fields from the list the runner can add a condition on.</summary>
    public static QueryBuilderConfig Picker(QueryBuilderConfig config, QueryGroup rules, IEnumerable<ReportColumnDto> columns)
    {
        var adminFields = AdminFields(columns);
        var shown = rules.AllRules()
            .Where(rule => !rule.Hidden && !string.IsNullOrEmpty(rule.Id))
            .Select(rule => rule.Id!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filters = config.Filters.Select(filter =>
        {
            bool adminColumn = adminFields.Contains(filter.Id);
            bool onlyHidden = !shown.Contains(filter.Id)
                && rules.AllRules().Any(rule => rule.Hidden && string.Equals(rule.Id, filter.Id, StringComparison.OrdinalIgnoreCase));
            return adminColumn || onlyHidden ? filter with { HideFromPicker = true } : filter;
        }).ToList();

        return config with { Filters = filters };
    }

    public static HashSet<string> AdminFields(IEnumerable<ReportColumnDto> columns) =>
        columns
            .Where(column => column.IsHidden && !column.IsLiteral)
            .Select(column => column.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static QueryGroup Parse(string? json)
    {
        var parsed = QueryJson.TryParse(json);
        return parsed.IsSuccess ? parsed.Value : new QueryGroup();
    }

    private static bool IsAdminRule(QueryRule rule, IReadOnlySet<string> adminFields) =>
        rule.Hidden || (rule.Id is not null && adminFields.Contains(rule.Id));

    private static void RemoveAdminRules(QueryGroup group, IReadOnlySet<string> adminFields)
    {
        group.Rules.RemoveAll(node => node is QueryRule rule && IsAdminRule(rule, adminFields));
        foreach (var child in group.Rules.OfType<QueryGroup>().ToList())
        {
            RemoveAdminRules(child, adminFields);
            if (child.Rules.Count == 0)
            {
                group.Rules.Remove(child);
            }
        }
    }
}
