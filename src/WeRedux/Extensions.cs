using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace WeRedux
{
    public class ReducerTypes
    {
        public Type Type { get; internal set; }
        public ReducerAttribute Attribute { get; internal set; }
    }
    public static class Extensions
    {
        public static List<ReducerTypes> GetReducedClass<TState, TAction>(this Assembly[] assemblies)
            where TState : new()
            where TAction : IAction
        {
            var typesWithMyAttribute =
                from a in assemblies
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(ReducerAttribute), true)
                where attributes != null && attributes.Length > 0 && typeof(Reducer<TState, TAction>).IsAssignableFrom(t)
                select new ReducerTypes { Type = t, Attribute = attributes.First() as ReducerAttribute };
            return typesWithMyAttribute.ToList();
        }
        public static List<ReducerTypes> GetReducedClass<TState, TAction>()
        where TState : new()
        where TAction : IAction
        {
            return GetReducedClass<TState, TAction>(AppDomain.CurrentDomain.GetAssemblies());
        }

        internal static string GetName<TAction>(this TAction action)
            where TAction : IAction
            => action.GetType().Name.ToUpperInvariant();
    }
}
