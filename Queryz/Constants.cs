using Extenso.AspNetCore.Mvc.ExtensoUI;

namespace Queryz
{
    public static class Constants
    {
        public const State DefaultExtensoUIState = State.Inverse;

        public static class Roles
        {
            public const string Administrators = "Administrators";
            public const string ReportBuilderEditors = "Report Builder Editors";
            public const string ReportBuilderUsers = "Report Builder Users";
        }
    }
}