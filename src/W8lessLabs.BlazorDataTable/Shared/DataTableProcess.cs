#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace W8lessLabs.BlazorDataTable
{
    public delegate Task ReadDataAsync(int page, int pageSize);

    public class DataTableProcess<TEntity>
    {
        public DataTableProcess() 
        {
        }

        internal ILogger? Logger { get; set; }

        public ReadDataAsync? ReadDataAsync;

        public EventCallback ReadDataCompletedAsync;

        public EventCallback<IColumnDef> ColumnSortChangedAsync;

        public EventCallback<int> PageChanged;

        public EventCallback<int> PageSizeChanged;

        internal bool InProgress;

        private int _sortOrder;

        public async Task OnReadDataAsync(int page, int pageSize)
        {
            
            if (ReadDataAsync != null)
            {
                InProgress = true;
                try
                {
                    await ReadDataAsync(page, pageSize);
                }
                finally
                {
                    InProgress = false;

                    await ReadDataCompletedAsync.InvokeAsync(null);
                }
            }
        }
        
        internal async Task OnColumnClickedAsync(ColumnRenderInfo<TEntity> col)
        {
            Logger?.LogTrace("OnColumnClickedAsync - [{name}] - [{sortDirection}]", col.Title, col.SortDirection);

            switch (col.SortDirection)
            {
                case SortDirection.None:
                    _sortOrder++;
                    col.SortDirection = SortDirection.Ascending;
                    col.SortOrder = _sortOrder;
                    break;
                case SortDirection.Ascending:
                    col.SortDirection = SortDirection.Descending;
                    break;
                default:
                    col.SortDirection = SortDirection.None;
                    break;
            }
            await ColumnSortChangedAsync.InvokeAsync(col).ConfigureAwait(false);
        }

        public async Task OnPageSizeChanged(int newPageSize)
        {
            await PageSizeChanged.InvokeAsync(newPageSize);
        }
    }
}
