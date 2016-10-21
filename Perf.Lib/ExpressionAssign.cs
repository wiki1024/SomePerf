using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Perf.Lib
{
    /// <summary>
    ///  Based on Expression Tree, 5-10x faster than Reflection.
    /// </summary>
    public static class ExpressionAssign
    {
        /// <summary>
        /// All null properties will be overrided.
        /// </summary>
        /// <typeparam name="T"> All properties should be reference type or nullable, since value types have a default value.</typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns>revised target</returns>
        public static T Assign<T>(T target, T source) where T : class
        {
            foreach (var fastProp in PropertyCache<T>.Cache)
            {
                var tProp = fastProp.Get(target);
                if (tProp == null)
                {
                    fastProp.Set(target, fastProp.Get(source));
                }
            }
            return target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="overridedProperties">Properties to be overrided.</param>
        /// <returns></returns>
        public static T Assign<T>(T target, T source, string[] overridedProperties) where T : class
        {
            var keyCache = PropertyCache<T>.KeyCache;
            for (int i = 0; i < overridedProperties.Length; i++)
            {
                int index = keyCache.IndexOf(overridedProperties[i]);
                if (index != -1)
                {
                    var fastprop = PropertyCache<T>.Cache[index];
                    fastprop.Set(target, fastprop.Get(source));
                }
            }
            return target;
        }

        /// <summary>
        /// reverse version of overridedProperties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="skippedProperties">Override every property except these.</param>
        /// <returns></returns>
        public static T SkipAssign<T>(T target, T source, string[] skippedProperties) where T : class
        {
            var keyCache = PropertyCache<T>.KeyCache;
            for (int i = 0; i < keyCache.Count; i++)
            {
                int index = Array.IndexOf<string>(skippedProperties, keyCache[i]);
                if (index == -1)
                {
                    var fastprop = PropertyCache<T>.Cache[i];
                    fastprop.Set(target, fastprop.Get(source));
                }
            }
            return target;
        }

        /// <summary>
        /// For testing purpose.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <param name="fastProp"></param>
        /// <returns></returns>
        public static T Assign<T>(T target, T source, FastProperty fastProp) where T : class
        {
            fastProp.Set(target, fastProp.Get(source));
            return target;
        }
    }

    /// <summary>
    /// Source http://geekswithblogs.net/Madman/archive/2008/06/27/faster-reflection-using-expression-trees.aspx
    /// </summary>
    public class FastProperty
    {
        public readonly string Key;
        public readonly Func<object, object> Get;
        public readonly Action<object, object> Set;

        public FastProperty(string key, Func<object, object> get, Action<object, object> set)
        {
            Key = key;
            Get = get;
            Set = set;
        }
    }
    public static class PropertyCache<T> where T : class
    {
        public static List<FastProperty> Cache { get; private set; }
        public static List<string> KeyCache { get; private set; }
        static PropertyCache()
        {
            Cache = typeof(T).GetProperties().Select(p => new FastProperty(p.Name, InitializeHelper.InitializeGet(p), InitializeHelper.InitializeSet(p))).ToList();
            KeyCache = Cache.Select(p => p.Key).ToList();
        }
    }

    /// <summary>
    /// Source http://geekswithblogs.net/Madman/archive/2008/06/27/faster-reflection-using-expression-trees.aspx
    /// </summary>
    public static class InitializeHelper
    {
        public static Action<object, object> InitializeSet(PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            UnaryExpression instanceCast = (!property.DeclaringType.IsValueType) ? Expression.TypeAs(instance, property.DeclaringType) : Expression.Convert(instance, property.DeclaringType);
            UnaryExpression valueCast = (!property.PropertyType.IsValueType) ? Expression.TypeAs(value, property.PropertyType) : Expression.Convert(value, property.PropertyType);
            return Expression.Lambda<Action<object, object>>(Expression.Call(instanceCast, property.GetSetMethod(), valueCast), new ParameterExpression[] { instance, value }).Compile();
        }

        public static Func<object, object> InitializeGet(PropertyInfo property)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = (!property.DeclaringType.IsValueType) ? Expression.TypeAs(instance, property.DeclaringType) : Expression.Convert(instance, property.DeclaringType);
            return Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast, property.GetGetMethod()), typeof(object)), instance).Compile();
        }
    }
}
