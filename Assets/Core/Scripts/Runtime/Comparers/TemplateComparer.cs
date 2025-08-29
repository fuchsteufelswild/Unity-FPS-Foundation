using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Nexora
{
    /// <summary>
    /// Comparer that safely handles both Unity objects and C# objects.
    /// - Unity objects uses Unity's overloaded equality operators.
    /// - C# objects uses standard reference equality.
    /// </summary>
    public sealed class UniversalTemplateComparer :
        IEqualityComparer<object>
    {
        public static readonly UniversalTemplateComparer Instance = new();

        public new bool Equals(object x, object y)
        {
            if (x is UnityEngine.Object ux && y is UnityEngine.Object uy)
            {
                return ux == uy;
            }

            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            if (obj is UnityEngine.Object uObj)
            {
                return uObj.GetHashCode();
            }

            return RuntimeHelpers.GetHashCode(obj);
        }
    }

    /// <summary>
    /// Optimized comparer for mixed Unity/C# object dictionaries.
    /// Uses pre-compiled delegates and type caching to minimize overhead.
    /// </summary>
    public sealed class OptimizedTemplateComparer :
        IEqualityComparer<object>
    {
        public static readonly OptimizedTemplateComparer Instance = new();

        private static readonly Func<object, int> _unityHashGetter;
        private static readonly Func<object, object, bool> _unityEquality;

        /// <summary>
        /// Caching methods to avoid reflection repetition
        /// </summary>
        static OptimizedTemplateComparer()
        {
            var param = Expression.Parameter(typeof(object));
            var unityObjParam = Expression.Convert(param, typeof(UnityEngine.Object));

            _unityHashGetter = Expression.Lambda<Func<object, int>>(
                Expression.Call(unityObjParam, typeof(UnityEngine.Object).GetMethod("GetHashCode")),
                param).Compile();

            _unityEquality = Expression.Lambda<Func<object, object, bool>>(
                Expression.Equal(
                    Expression.Convert(param, typeof(UnityEngine.Object)),
                    Expression.Convert(Expression.Parameter(typeof(object)), typeof(UnityEngine.Object))),
                param, Expression.Parameter(typeof(object))).Compile();
        }

        public new bool Equals(object x, object y)
        {
            if (x == null) return y == null;
            if (y == null) return false;

            if(x.GetType().IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return _unityEquality(x, y);
            }

            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            if(obj == null)
            {
                return 0;
            }

            if(obj.GetType().IsSubclassOf(typeof(UnityEngine.Object)))
            {
                return _unityHashGetter(obj);
            }

            return RuntimeHelpers.GetHashCode(obj);
        }
    }


    /// <summary>
    /// Static helper class for comparison of enum flags. Can use <see cref="HasFlag{T}(T, T)"/>
    /// method to check if first argument has the flag.
    /// </summary>
    public static class EnumFlagsComparer
    {
        private static readonly ConcurrentDictionary<Type, Delegate> _cachedFunctions = new();

        /// <remarks>
        /// Provides a type-safe enum flag check.
        /// </remarks>
        /// <returns>
        /// If <paramref name="self"/> has <paramref name="flag"/>. 
        /// </returns>
        public static bool HasFlag<T>(T self, T flag)
            where T : Enum
        {
            var function = (Func<T, T, bool>)_cachedFunctions.GetOrAdd(typeof(T), CreateHasFlagsFunction<T>());
            return function(self, flag);
        }

        /// <returns>
        ///     (self & flag) == flag)
        /// </returns>
        private static Func<T, T, bool> CreateHasFlagsFunction<T>()
            where T : Enum
        {
            var param1 = Expression.Parameter(typeof(T), "self");
            var param2 = Expression.Parameter(typeof(T), "flag");

            var underlyingType = Enum.GetUnderlyingType(typeof(T));
            var convertedParam1 = Expression.Convert(param1, underlyingType);
            var convertedParam2 = Expression.Convert(param2, underlyingType);

            var andExpression = Expression.And(convertedParam1, convertedParam2);
            var equalExpression = Expression.Equal(andExpression, convertedParam2);

            return Expression.Lambda<Func<T, T, bool>>(equalExpression, param1, param2).Compile();
        }
    }

    public static class InterfaceComparer
    {
        public static bool AreSameInstanceAndCompatibleInterface<T1, T2>(T1 obj1, T2 obj2)
            where T1 : class
            where T2 : class
        {
            if(typeof(T1) !=  typeof(T2))
            {
                throw new ArgumentException($"Cannot compare interfaces of different types" +
                    $" '{typeof(T1).Name}' '{typeof(T2).Name}'");
            }

            return ReferenceEquals(obj1, obj2);
        }
    }
}