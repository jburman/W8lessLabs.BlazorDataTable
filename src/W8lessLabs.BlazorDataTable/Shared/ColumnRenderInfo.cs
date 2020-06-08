#nullable enable

using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace W8lessLabs.BlazorDataTable
{
    public interface IColumnDef
    {
        string? DataField { get; }
        bool Sortable { get; }
        SortDirection SortDirection { get; }
        int SortOrder { get; }
        string Width { get; }
    }

    internal abstract class ColumnRenderInfo<TEntity> : IColumnDef
    {
        public string? DataField { get; protected set; }

        public bool IsCustom { get; set; } = false;

        public string? Title { get; set; }

        public bool Sortable { get; set; }

        public SortDirection SortDirection { get; set; } = SortDirection.None;

        public int SortOrder { get; set; } = 0;

        public string Width { get; set; } = string.Empty;

        public bool RenderAsMarkup { get; set; }

        public string CustomContentClass { get; set; } = string.Empty;

        public Func<TEntity, string?>? CustomContentClassCallback { get; set; }

        public string CustomHeaderClass { get; set; } = string.Empty;

        public Func<TEntity, string?>? Render { get; set; }

        public abstract void AddToSorter(QuerySort<TEntity> sorter);

        public bool IsSorted => Sortable && SortDirection != SortDirection.None;

        public RenderFragment<TEntity>? ChildContent { get; set; }
    }

    internal class ColumnRenderInfo<TEntity, TProp> : ColumnRenderInfo<TEntity>
    {
        public ColumnRenderInfo()
        {
        }

        internal Expression<Func<TEntity, TProp>>? PropExpr;
        internal Func<TEntity, TProp>? PropFunc;
        internal int ValueIndex;

        public IComparer<TProp>? Comparer { get; set; } = null;

        public Expression<Func<TEntity, TProp>>? Property
        {
            get => PropExpr;
            set
            {
                PropExpr = value;
                PropFunc = value?.Compile();
                var memberExpression = ((MemberExpression?)value?.Body);
                DataField = memberExpression?.Member.Name;
                if (Render is null)
                    Render = e => _DefaultRender(this, e);
            }
        }

        public int Index 
        {
            get => ValueIndex; 
            set 
            {
                ValueIndex = value;
                DataField = ValueIndex.ToString();
                if (Render is null)
                    Render = e => _DefaultRender(this, e);
            }
        }

        public override void AddToSorter(QuerySort<TEntity> sorter)
        {
            if (DataField != null)
            {
                if (PropExpr != null)
                    sorter.AddProperty<TProp>(PropExpr, Comparer);
                else if (sorter is QuerySort<TProp[]>)
                    sorter.AddIndex<TProp>(ValueIndex, Comparer);
                else
                    throw new ArgumentException("Unable to determine TEntity access pattern. Expected either PropertyExpression or Array indexer.");
            }
        }

        private static string _DefaultRender(ColumnRenderInfo<TEntity, TProp> colDef, TEntity entity) =>
            (colDef, entity, colDef?.PropFunc) switch
            {
                (ColumnRenderInfo<TEntity, TProp> col, TProp[] record, null) 
                    => record[col.ValueIndex]?.ToString() ?? string.Empty,
                (ColumnRenderInfo<TEntity, TProp> col, TEntity e, Func<TEntity, TProp> propFunc) 
                    => propFunc(entity)?.ToString() ?? string.Empty,
                _ 
                    => string.Empty
            };
    }
}
