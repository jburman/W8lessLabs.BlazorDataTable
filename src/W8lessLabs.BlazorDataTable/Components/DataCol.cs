#nullable enable
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Linq.Expressions;

namespace W8lessLabs.BlazorDataTable
{
    public class DataCol<TEntity, TProp> : ComponentBase, IDisposable, IColumnDef
    {
        private ColumnRenderInfo<TEntity, TProp> _colRenderInfo;
        private bool _rendered;

        public DataCol()
        {
            _colRenderInfo = new ColumnRenderInfo<TEntity, TProp>();
            _rendered = false;
        }

        [CascadingParameter] public DataTable<TEntity>? Table { get; set; }

        [Parameter] public Expression<Func<TEntity, TProp>>? Property { get; set; }

        [Parameter] public string? Title { get; set; }

        [Parameter] public bool Sortable { get; set; } = false;

        [Parameter] public SortDirection SortDirection { get; set; } = SortDirection.None;

        [Parameter] public int SortOrder { get; set; } = 0;

        [Parameter] public string Width { get; set; } = string.Empty;

        [Parameter] public bool RenderAsMarkup { get; set; } = false;

        [Parameter] public string CustomContentClass { get; set; } = string.Empty;

        [Parameter] public Func<TEntity, string>? CustomContentClassCallback { get; set; }

        [Parameter] public string CustomHeaderClass { get; set; } = string.Empty;

        [Parameter] public Func<TEntity, string>? Render { get; set; }

        public string? DataField { get => _colRenderInfo.DataField; }

        protected override void OnParametersSet()
        {
            if (!_rendered)
            {
                _colRenderInfo.IsCustom = false;
                _colRenderInfo.Title = Title;
                _colRenderInfo.Sortable = Sortable;
                _colRenderInfo.SortDirection = SortDirection;
                _colRenderInfo.SortOrder = SortOrder;
                _colRenderInfo.RenderAsMarkup = RenderAsMarkup;
                _colRenderInfo.CustomContentClass = CustomContentClass;
                _colRenderInfo.CustomContentClassCallback = CustomContentClassCallback;
                _colRenderInfo.CustomHeaderClass = CustomHeaderClass;
                _colRenderInfo.Render = Render;
                _colRenderInfo.Property = Property;
                _colRenderInfo.Width = Width;
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            _rendered = true;
        }

        protected override void OnInitialized()
        {
            Table?.AddColumn(_colRenderInfo);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_colRenderInfo != null)
                    Table?.RemoveColumn(_colRenderInfo);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    public partial class CustomCol<TEntity> : ComponentBase, IDisposable
    {
        private ColumnRenderInfo<TEntity> _colRenderInfo;
        private bool _rendered;

        public CustomCol()
        {
            _colRenderInfo = new ColumnRenderInfo<TEntity, TEntity>();
            _rendered = false;
        }

        [Parameter] public RenderFragment<TEntity>? ChildContent { get; set; }

        [CascadingParameter] public DataTable<TEntity>? Table { get; set; }

        [Parameter] public string? Title { get; set; }

        [Parameter] public string Width { get; set; } = string.Empty;

        [Parameter] public string CustomContentClass { get; set; } = string.Empty;

        [Parameter] public Func<TEntity, string>? CustomContentClassCallback { get; set; }

        [Parameter] public string CustomHeaderClass { get; set; } = string.Empty;


        protected override void OnParametersSet()
        {
            // freeze the configuration once the column is rendered
            if (!_rendered)
            {
                _colRenderInfo.IsCustom = true;
                _colRenderInfo.Title = Title;
                _colRenderInfo.Sortable = false;
                _colRenderInfo.SortDirection = SortDirection.None;
                _colRenderInfo.RenderAsMarkup = false;
                _colRenderInfo.CustomContentClass = CustomContentClass;
                _colRenderInfo.CustomContentClassCallback = CustomContentClassCallback;
                _colRenderInfo.CustomHeaderClass = CustomHeaderClass;
                _colRenderInfo.Width = Width;
                _colRenderInfo.ChildContent = ChildContent;
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            _rendered = true;
        }

        protected override void OnInitialized()
        {
            Table?.AddColumn(_colRenderInfo);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (_colRenderInfo != null)
                    Table?.RemoveColumn(_colRenderInfo);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
