using System.Data;

namespace Queryz.Models
{
    public class PreviewModel
    {
        public int ReportId { get; set; }

        public string ReportName { get; set; }

        public string DateFormat { get; set; }

        public DataTable Data { get; set; }
    }
}