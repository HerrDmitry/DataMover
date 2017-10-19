using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.Configuration;

namespace Importer.Configuration
{
    public static class FilterExtensions
    {
        public static Func<IReadOnlyList<ISourceField>, bool> PrepareSourceFilter(this IRow row)
        {
            return row.PrepareFilterGeneric<IReadOnlyList<ISourceField>>((source, index, value) =>
                index < source.Count && source[index].Source == value);
        }

        public static Func<IDataRow, bool> PrepareTargetFilter(this IRow row)
        {
            if (row.Filter?.Count > 0)
            {
                return source =>
                {
                    return row.Filter.Any(f => source[f.Name].ToString() == f.Value);
                };
            }
            return source => true;
        }


        private static Func<T, bool> PrepareFilterGeneric<T>(this IRow row, Func<T, int, string, bool> comparer)
        {
            if (row.Filter?.Count > 0)
            {
                var filterValues=new List<Tuple<int, string>>();
                foreach (var f in row.Filter)
                {
                    for (var c = 0; c < row.Columns.Count; c++)
                    {
                        if (f.Name == row.Columns[c].Name)
                        {
                            filterValues.Add(new Tuple<int, string>(c,f.Value));
                            break;
                        }
                    }
                }
                return source =>
                {
                    return filterValues.All(flt => comparer(source,flt.Item1,flt.Item2));
                };
            }

            return source => true;
        }

    }
}