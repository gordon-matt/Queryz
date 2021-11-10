using System.Collections.Generic;

namespace Queryz.Models
{
    public class WizardStep3Model
    {
        //public WizardStep3Model()
        //{
        //    AvailableColumns = new List<ColumnModel>();
        //    SelectedColumns = new List<SelectedColumnModel>();
        //}

        public int ReportId { get; set; }

        public ColumnModel[] AvailableColumns { get; set; }

        public SelectedColumnModel[] SelectedColumns { get; set; }

        public EnumerationModel[] AvailableEnumerations { get; set; }

        public IEnumerable<string> AvailableTransformFunctions { get; set; }
    }

    public class EnumerationModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ColumnModel
    {
        public string ColumnName { get; set; }

        public string Type { get; set; }

        public bool IsForeignKey { get; set; }

        public IEnumerable<string> AvailableParentColumns { get; set; }
    }

    public class SelectedColumnModel : ColumnModel
    {
        public string Alias { get; set; }

        public bool IsLiteral { get; set; }

        /// <summary>
        ///  Used when column is foreign key
        /// </summary>
        public string DisplayColumn { get; set; }

        public int? EnumerationId { get; set; }

        public string TransformFunction { get; set; }

        public string Format { get; set; }

        public bool IsHidden { get; set; }
    }
}