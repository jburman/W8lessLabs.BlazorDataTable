#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace W8lessLabs.BlazorDataTable
{
    public class DataTableConfig<TEntity>
    {
        public string Height { get; set; } = string.Empty;

        public string Width { get; set; } = string.Empty;

        public ReadData<TEntity>? ReadData { get; set; }

        public int PageSize { get; set; }

        public int[] PageSizes { get; set; } = DataTable<TEntity>.DefaultPageSizes;

        public PaginatorStyle PaginatorStyle { get; set; }
    }

    public partial class DataTable<TEntity>
    {
        public static readonly int DefaultPageSize = 20;
        internal static readonly int[] DefaultPageSizes = new[] { 10, 20, 50 };

        private bool _rendered = false;

        public DataTable()
        {
            Config = new DataTableConfig<TEntity>();
            Columns = new List<ColumnRenderInfo<TEntity>>();
            ReadData = default;
            Page = 1;
            Process = new DataTableProcess<TEntity>();
        }

        [Inject]
        public ILogger<DataTable<TEntity>>? Logger { get; set; }

        public DataTableConfig<TEntity> Config { get; private set; }

        [Parameter] public string Height { get; set; } = string.Empty;

        [Parameter] public string Width { get; set; } = string.Empty;

        internal List<ColumnRenderInfo<TEntity>> Columns { get; set; }
        
        [Parameter] public ReadData<TEntity>? ReadData { get; set; }

        [Parameter] public int PageSize { get; set; } = DefaultPageSize;

        [Parameter] public int[] PageSizes { get; set; } = DefaultPageSizes;

        [Parameter] public PaginatorStyle PaginatorStyle { get; set; } = PaginatorStyle.Full;

        internal DataTableProcess<TEntity> Process { get; set; }

        internal DataTableSource<TEntity>? DataSource { get; set; }

        internal int Page { get; private set; }

        private delegate void CallbackHandler<T>(T value);

        private void OnReadDataCompleted(object arg)
        {
        }

        private void OnColumnSortChanged(IColumnDef col)
        {
            InvokeAsync(async () => await Process.OnReadDataAsync(Page, Config.PageSize));
        }

        private void OnPageChanged(int page)
        {
            Page = page;
            InvokeAsync(async () => await Process.OnReadDataAsync(Page, Config.PageSize));
        }

        private void OnPageSizeChanged(int pageSize)
        {
            Page = 1;
            Config.PageSize = pageSize;
            InvokeAsync(async () => await Process.OnReadDataAsync(Page, Config.PageSize));
        }

        private static readonly List<TEntity> _Empty = new List<TEntity>(0);
        private static Task<ReadDataResponse<TEntity>> _DefaultRead(ReadDataRequest<TEntity> request) =>
            Task.FromResult(request.CreateResponse(_Empty, 0));

        internal void AddColumn(ColumnRenderInfo<TEntity> col)
        {
            if (col != null)
                Columns.Add(col);
        }

        internal void RemoveColumn(ColumnRenderInfo<TEntity> col)
        {
            if (col != null)
                Columns.Remove(col);
        }

        internal async Task ColumnClickedAsync(ColumnRenderInfo<TEntity> col)
        {
            await Process.OnColumnClickedAsync(col);
        }

        protected override void OnInitialized()
        {
            Process.Logger = Logger;
            Process.ColumnSortChangedAsync = new EventCallback<IColumnDef>(this, new CallbackHandler<IColumnDef>(OnColumnSortChanged));
            Process.ReadDataCompletedAsync = new EventCallback(this, new CallbackHandler<object>(OnReadDataCompleted));
            Process.PageChanged = new EventCallback<int>(this, new CallbackHandler<int>(OnPageChanged));
            Process.PageSizeChanged = new EventCallback<int>(this, new CallbackHandler<int>(OnPageSizeChanged));

            Logger?.LogTrace("DataTable Initialized");
        }

        protected override void OnParametersSet()
        {
            // freeze the config once the data table has been rendered
            if (!_rendered)
            {
                Config.Height = Height;
                Config.Width = Width;
                Config.PageSize = PageSize;
                Config.PageSizes = PageSizes;
                Config.PaginatorStyle = PaginatorStyle;
                Config.ReadData = ReadData;

                Logger?.LogTrace("Set DataTableConfigs");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                Logger?.LogTrace("DataTable -> OnAfterRenderAsync");

                _rendered = true;
                if (DataSource is null)
                {
                    DataSource = new DataTableSource<TEntity>(Process, Config.ReadData ?? new ReadData<TEntity>(_DefaultRead), Columns);
                    await Process.OnReadDataAsync(Page, Config.PageSize);
                }
            }
        }
    }
}
