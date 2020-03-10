#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace W8lessLabs.BlazorDataTable
{
    public enum SortDirection { Ascending, Descending, None }

    public delegate Task<ReadDataResponse<TEntity>> ReadData<TEntity>(ReadDataRequest<TEntity> dataRequest);

    public class DataTableSource<TEntity>
    {
        public ReadDataResponse<TEntity>? Result { get; private set; }

        private readonly DataTableProcess<TEntity> _process;

        private readonly ReadData<TEntity> _read;

        internal readonly IReadOnlyList<ColumnRenderInfo<TEntity>> Cols;

        internal readonly QuerySort<TEntity> Sorter;

        internal DataTableSource(DataTableProcess<TEntity> process, ReadData<TEntity> read, IReadOnlyList<ColumnRenderInfo<TEntity>> cols)
        {
            _process = process;
            _read = read;
            Cols = cols;
            Sorter = new QuerySort<TEntity>();
            foreach (var col in cols)
                col.AddToSorter(Sorter);

            _process.ReadDataAsync = new ReadDataAsync(
                async (int page, int pageSize) =>
                {
                    Result = await _read(new ReadDataRequest<TEntity>(this, page, pageSize));
                });
        }
    }

    public class ReadDataRequest<TEntity>
    {
        private DataTableSource<TEntity> _source;

        public ReadDataRequest(DataTableSource<TEntity> source, int page, int pageSize)
        {
            _source = source;
            Page = page;
            PageSize = pageSize;
        }

        public IReadOnlyList<IColumnDef> Columns { get => _source.Cols; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public IEnumerable<TEntity> ApplySorts(IEnumerable<TEntity> data)
        {
            var sorts = _source.Cols.Where(col => col.Sortable && col.SortDirection != SortDirection.None && col.DataField != null)
                .OrderBy(col => col.SortOrder)
                .Select(col => (col.DataField ?? string.Empty, col.SortDirection == SortDirection.Descending));
            return _source.Sorter.ApplySorts(data, sorts);
        }

        public IQueryable<TEntity> ApplySorts(IQueryable<TEntity> data)
        {
            var sorts = _source.Cols.Where(col => col.Sortable && col.SortDirection != SortDirection.None && col.DataField != null)
                .OrderBy(col => col.SortOrder)
                .Select(col => (col.DataField ?? string.Empty, col.SortDirection == SortDirection.Descending));
            return _source.Sorter.ApplySorts(data, sorts);
        }

        public IEnumerable<TEntity> ApplyPaging(IEnumerable<TEntity> data) =>
            data.Skip((Page - 1) * PageSize).Take(PageSize);

        public IEnumerable<TEntity> ApplyPaging(IQueryable<TEntity> data) =>
            data.Skip((Page - 1) * PageSize).Take(PageSize);

        public ReadDataResponse<TEntity> CreateResponse(IReadOnlyList<TEntity> data, int totalRecords) =>
            new ReadDataResponse<TEntity>(data.ToList(), totalRecords, Page, PageSize);
    }

    public class ReadDataResponse<TEntity>
    {
        public ReadDataResponse(IReadOnlyList<TEntity> data, int totalRecords, int page, int pageSize)
        {
            Data = data;
            TotalRecords = totalRecords;
            Page = page;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }

        public readonly IReadOnlyList<TEntity> Data;
        public readonly int TotalRecords;
        public readonly int Page;
        public readonly int PageSize;
        public readonly int TotalPages;

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;
    }
}
