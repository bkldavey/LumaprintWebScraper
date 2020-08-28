using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumaprintsTask.Helper
{
    public static class ListHelper
    {
        public static List<TSource> ToListOrEmpty<TSource>(this IEnumerable<TSource> source)
        {
            return source == null ? new List<TSource>() : source.ToList();
        }
        //public static IEquatable<T> FirstOrDefaultOrEmpty<T>(this T source)
        //{
        //    if (EqualityComparer<T>.Default.Equals(source, default(T)))
        //    {
        //        return IEquatable<source>;
        //    }
        //    else
        //        return null;
        //    return source == default<T> ? source : null;
        //}
    }
}
