using System.Collections.Generic;
using Extenso.Data.QueryBuilder;

namespace Queryz.Models
{
    public class WizardStep6Model
    {
        public int ReportId { get; set; }

        public IEnumerable<RelationshipModel> Relationships { get; set; }
    }

    public class RelationshipModel
    {
        public string TableName { get; set; }

        public string ParentTable { get; set; }

        public string PrimaryKey { get; set; }

        public string ForeignKey { get; set; }

        public JoinType JoinType { get; set; }

        public IEnumerable<string> AvailableColumns { get; set; }

        public IEnumerable<string> AvailableParentTables { get; set; }

        public bool IsEmpty
        {
            get
            {
                return
                    string.IsNullOrEmpty(ParentTable) &&
                    string.IsNullOrEmpty(PrimaryKey) &&
                    string.IsNullOrEmpty(ForeignKey);
            }
        }

        public override string ToString() =>
            $"{JoinType} {TableName}.{ForeignKey} ON {ParentTable}.{PrimaryKey}";
    }
}