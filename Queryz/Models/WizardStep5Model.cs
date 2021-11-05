using System.Collections.Generic;
using Extenso.Data.QueryBuilder;

namespace Queryz.Models
{
    public class WizardStep5Model
    {
        public int ReportId { get; set; }

        public IEnumerable<string> AvailableColumns { get; set; }

        public IEnumerable<SortingModel> Sortings { get; set; }
    }

    public class SortingModel
    {
        public string ColumnName { get; set; }

        public SortDirection SortDirection { get; set; }
    }
}