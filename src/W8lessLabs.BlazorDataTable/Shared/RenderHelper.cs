namespace W8lessLabs.BlazorDataTable
{
    public static class RenderHelper
    {
        public static string SortAscIcon = "oi oi-arrow-top";
        public static string SortDescIcon = "oi oi-arrow-bottom";
        public static string PageLeftIcon = "oi oi-chevron-left";
        public static string PageRightIcon = "oi oi-chevron-right";

        public static string TableSizeStyle<TEntity>(this DataTable<TEntity> table) =>
            (table.Config.Width, table.Config.Height) switch
            {
                ("", "") => string.Empty,
                ("", string height) => $"height: {height};",
                (string width, "") => $"width: {width};",
                (string width, string height) => $"width: {width}; height: {height};",
                _ => string.Empty
            };

        public static string HeaderClass(this IColumnDef col) =>
            (col.Sortable, col.SortDirection) switch
            {
                (true, SortDirection.Ascending) => "w8less-sa sortable",
                (true, SortDirection.Descending) => "w8less-sd sortable",
                (true, SortDirection.None) => "w8less-ns sortable",
                _ => string.Empty
            };

        public static string WidthStyle(this IColumnDef col) =>
            col.Width switch
            {
                string s when string.IsNullOrEmpty(s) => string.Empty,
                _ => " width: " + col.Width + ";"
            };

        public static string CellClass(this IColumnDef col) =>
            (col.Sortable, col.SortDirection) switch
            {
                (true, SortDirection.Ascending) => "w8less-sa",
                (true, SortDirection.Descending) => "w8less-sd",
                _ => string.Empty
            };

        public static string IconClass(this SortDirection dir) =>
            dir switch
            {
                SortDirection.Descending => "w8less-icon " + SortDescIcon,
                _ => "w8less-icon " + SortAscIcon
            };

        public static string PageClass(bool isAscending = false) =>
            (isAscending) ?
                PageRightIcon : PageLeftIcon;
    }
}
