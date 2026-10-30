namespace Queryz;

public static class Constants
{
    public const string ClientThemeLocalStorageKey = "queryz.theme";

    public static class MaxLengths
    {
        public const int Name = 128;
        public const int ConnectionString = 1024;
        public const int ColumnName = 255;
        public const int Format = 128;
        public const int TimeZoneId = 128;
        public const int UserId = 450;
    }

    public static class Schemas
    {
        public const string App = "app";
    }

    public static class ServerSettingDefaults
    {
        public const string SiteName = "Queryz";
    }

    /// <summary>The on-screen run shows this many rows. Download is not capped here.</summary>
    public const int PreviewRows = 10;

    /// <summary>A saved row limit cannot ask for more than this.</summary>
    public const int MaxReportRows = 5000;
}
