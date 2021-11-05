using Queryz.Data.Domain;

namespace Queryz.Models
{
    public class WizardStep7Model
    {
        public int ReportId { get; set; }

        public bool IsDistinct { get; set; }

        public int RowLimit { get; set; }

        public EnumerationHandling EnumerationHandling { get; set; }

        public IdNamePair<string>[] AvailableUsers { get; set; }

        public string[] DeniedUserIds { get; set; }
    }
}