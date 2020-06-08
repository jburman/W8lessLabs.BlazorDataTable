#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace W8lessLabs.BlazorDataTable
{
    public class QuerySort<T>
    {
        private Dictionary<string,
            (Func<IQueryable<T>, bool, IOrderedQueryable<T>> applyToQueryable,
            Func<IOrderedQueryable<T>, bool, IOrderedQueryable<T>> applyToOrderedQueryable)> _sortQueryFuncs;

        private Dictionary<string,
            (Func<IEnumerable<T>, bool, IOrderedEnumerable<T>> applyToEnumerable,
            Func<IOrderedEnumerable<T>, bool, IOrderedEnumerable<T>> applyToOrderedEnumerable)> _sortEnumFuncs;

        public QuerySort()
        {
            _sortQueryFuncs = new Dictionary<string,
                (Func<IQueryable<T>, bool, IOrderedQueryable<T>> applyToQueryable,
                Func<IOrderedQueryable<T>, bool, IOrderedQueryable<T>> applyToOrderedQueryable)>(StringComparer.OrdinalIgnoreCase);

            _sortEnumFuncs = new Dictionary<string, (Func<IEnumerable<T>, bool, IOrderedEnumerable<T>> applyToEnumerable,
                Func<IOrderedEnumerable<T>, bool, IOrderedEnumerable<T>> applyToOrderedEnumerable)>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddProperty<TProp>(string name, IComparer<TProp>? comparer = null)
        {
            var paramExpression = Expression.Parameter(typeof(T), "e");
            var memberExpression = Expression.Property(paramExpression, name);
            var applyToQueryable = new Func<IQueryable<T>, bool, IOrderedQueryable<T>>((query, descending) =>
            {
                if (descending)
                    return query.OrderByDescending(Expression.Lambda<Func<T, TProp>>(memberExpression, paramExpression), comparer);
                else
                    return query.OrderBy(Expression.Lambda<Func<T, TProp>>(memberExpression, paramExpression), comparer);
            });

            var applyToOrderedQueryable = new Func<IOrderedQueryable<T>, bool, IOrderedQueryable<T>>((query, descending) =>
            {
                if (descending)
                    return query.ThenByDescending(Expression.Lambda<Func<T, TProp>>(memberExpression, paramExpression), comparer);
                else
                    return query.ThenBy(Expression.Lambda<Func<T, TProp>>(memberExpression, paramExpression), comparer);
            });

            _sortQueryFuncs[name] = (applyToQueryable, applyToOrderedQueryable);


            var applyToEnumerable = new Func<IEnumerable<T>, bool, IOrderedEnumerable<T>>((enumerable, descending) =>
            {
                if (descending)
                    return enumerable.OrderByDescending(Expression.Lambda<Func<T, TProp>>(memberExpression, paramExpression).Compile(), comparer);
                else
                    return enumerable.OrderBy(Expression.Lambda<Func<T, TProp>>(memberExpression, paramExpression).Compile(), comparer);
            });

            var applyToOrderedEnumerable = new Func<IOrderedEnumerable<T>, bool, IOrderedEnumerable<T>>((enumerable, descending) =>
            {
                if (descending)
                    return enumerable.ThenByDescending(Expression.Lambda<Func<T, TProp>>(memberExpression, paramExpression).Compile(), comparer);
                else
                    return enumerable.ThenBy(Expression.Lambda<Func<T, TProp>>(memberExpression, paramExpression).Compile(), comparer);
            });

            _sortEnumFuncs[name] = (applyToEnumerable, applyToOrderedEnumerable);
        }

        public void AddProperty<TProp>(Expression<Func<T, TProp>> expr, IComparer<TProp>? comparer = null)
        {
            var memberExpression = ((MemberExpression)expr.Body);

            var applyToQueryable = new Func<IQueryable<T>, bool, IOrderedQueryable<T>>((query, descending) =>
            {
                if (descending)
                    return query.OrderByDescending(expr, comparer);
                else
                    return query.OrderBy(expr, comparer);
            });

            var applyToOrderedQueryable = new Func<IOrderedQueryable<T>, bool, IOrderedQueryable<T>>((query, descending) =>
            {
                if (descending)
                    return query.ThenByDescending(expr, comparer);
                else
                    return query.ThenBy(expr, comparer);
            });

            _sortQueryFuncs[memberExpression.Member.Name] = (applyToQueryable, applyToOrderedQueryable);


            var applyToEnumerable = new Func<IEnumerable<T>, bool, IOrderedEnumerable<T>>((enumerable, descending) =>
            {
                if (descending)
                    return enumerable.OrderByDescending(expr.Compile(), comparer);
                else
                    return enumerable.OrderBy(expr.Compile(), comparer);
            });

            var applyToOrderedEnumerable = new Func<IOrderedEnumerable<T>, bool, IOrderedEnumerable<T>>((enumerable, descending) =>
            {
                if (descending)
                    return enumerable.ThenByDescending(expr.Compile(), comparer);
                else
                    return enumerable.ThenBy(expr.Compile(), comparer);
            });

            _sortEnumFuncs[memberExpression.Member.Name] = (applyToEnumerable, applyToOrderedEnumerable);
        }

        public void AddIndex<TProp>(int index, IComparer<TProp>? comparer = null)
        {
            var applyToQueryable = 
                (Func<IQueryable<T>, bool, IOrderedQueryable<T>>)
                new Func<IQueryable<TProp[]>, bool, IOrderedQueryable<TProp[]>>((query, descending) =>
                {
                    if (descending)
                        return query.OrderByDescending(e => e[index], comparer);
                    else
                        return query.OrderBy(e => e[index], comparer);
                });

            var applyToOrderedQueryable = 
                (Func<IOrderedQueryable<T>, bool, IOrderedQueryable<T>>)
                new Func<IOrderedQueryable<TProp[]>, bool, IOrderedQueryable<TProp[]>>((query, descending) =>
                {
                    if (descending)
                        return query.ThenByDescending(e => e[index], comparer);
                    else
                        return query.ThenBy(e => e[index], comparer);
                });
            _sortQueryFuncs[index.ToString()] = (applyToQueryable, applyToOrderedQueryable);

            var applyToEnumerable =
                (Func<IEnumerable<T>, bool, IOrderedEnumerable<T>>)
                new Func<IEnumerable<TProp[]>, bool, IOrderedEnumerable<TProp[]>>((enumerable, descending) =>
                {
                    if (descending)
                        return enumerable.OrderByDescending(e => e[index], comparer);
                    else
                        return enumerable.OrderBy(e => e[index], comparer);
                });

            var applyToOrderedEnumerable = 
                (Func<IOrderedEnumerable<T>, bool, IOrderedEnumerable<T>>)
                new Func<IOrderedEnumerable<TProp[]>, bool, IOrderedEnumerable<TProp[]>>((enumerable, descending) =>
                {
                    if (descending)
                        return enumerable.ThenByDescending(e => e[index], comparer);
                    else
                        return enumerable.ThenBy(e => e[index], comparer);
                });

            _sortEnumFuncs[index.ToString()] = (applyToEnumerable, applyToOrderedEnumerable);
        }

        public IQueryable<T> ApplySorts(IQueryable<T> query, IEnumerable<(string name, bool descending)> sorts)
        {
            IOrderedQueryable<T>? ordered = null;

            foreach (var sort in sorts)
            {
                var orderApply = _sortQueryFuncs[sort.name];
                if (ordered == null)
                    ordered = orderApply.applyToQueryable(query, sort.descending);
                else
                    ordered = orderApply.applyToOrderedQueryable(ordered, sort.descending);
            }
            return ordered ?? query;
        }

        public IEnumerable<T> ApplySorts(IEnumerable<T> query, IEnumerable<(string name, bool descending)> sorts)
        {
            IOrderedEnumerable<T>? ordered = null;

            foreach (var sort in sorts)
            {
                var orderApply = _sortEnumFuncs[sort.name];
                if (ordered == null)
                    ordered = orderApply.applyToEnumerable(query, sort.descending);
                else
                    ordered = orderApply.applyToOrderedEnumerable(ordered, sort.descending);
            }
            return ordered ?? query;
        }
    }
}
