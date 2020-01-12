using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;
using System.Collections;

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
            => action?.GetType().Name.ToUpperInvariant() ?? null;


        sealed class JsonNonStringKeyDictionaryConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>>
        {
            public override IDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var convertedType = typeof(Dictionary<,>)
                    .MakeGenericType(typeof(string), typeToConvert.GenericTypeArguments[1]);
                var value = JsonSerializer.Deserialize(ref reader, convertedType, options);
                var instance = (Dictionary<TKey, TValue>)Activator.CreateInstance(
                    typeToConvert,
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    null,
                    CultureInfo.CurrentCulture);
                var enumerator = (IEnumerator)convertedType.GetMethod("GetEnumerator")!.Invoke(value, null);
                var parse = typeof(TKey).GetMethod("Parse", 0, BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, new[] { typeof(string) }, null);
                if (parse == null) throw new NotSupportedException($"{typeof(TKey)} as TKey in IDictionary<TKey, TValue> is not supported.");
                while (enumerator.MoveNext())
                {
                    var element = (KeyValuePair<string?, TValue>)enumerator.Current;
                    instance.Add((TKey)parse.Invoke(null, new[] { element.Key }), element.Value);
                }
                return instance;
            }

            public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue> value, JsonSerializerOptions options)
            {
                var convertedDictionary = new Dictionary<string?, TValue>(value.Count);
                foreach (var (k, v) in value) convertedDictionary[k?.ToString()] = v;
                JsonSerializer.Serialize(writer, convertedDictionary, options);
                convertedDictionary.Clear();
            }
        }

        sealed class JsonNonStringKeyDictionaryConverterFactory : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert)
            {
                if (!typeToConvert.IsGenericType) return false;
                if (typeToConvert.GenericTypeArguments[0] == typeof(string)) return false;
                return typeToConvert.GetInterface("IDictionary") != null;
            }

            public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                var converterType = typeof(JsonNonStringKeyDictionaryConverter<,>)
                    .MakeGenericType(typeToConvert.GenericTypeArguments[0], typeToConvert.GenericTypeArguments[1]);
                var converter = (JsonConverter)Activator.CreateInstance(
                    converterType,
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    null,
                    CultureInfo.CurrentCulture);
                return converter;
            }
        }

        public static string ToJson<TState,TAction>(this IStore<TState,TAction> store)
              where TState : new()
            where TAction : IAction
        {
            HistoryContent histo = new HistoryContent();
            
            store.History.ForEach(h =>
            {
                histo.Add(h.Mutation);
            });
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonNonStringKeyDictionaryConverterFactory());
            return JsonSerializer.Serialize(histo,typeof(HistoryContent));
        }

        public static HistoryContent GetHistoryContent(this string json)
        {
            return JsonSerializer.Deserialize<HistoryContent>(json);
        }
    }

    public class HistoryContent
    {

        public List<string> Actions { get; set; } = new List<string>();
        

        public void Add(string action)
        {
            Actions.Add(action);
        }

    }

    public class ActionById
    {
       // [JsonPropertyName("action")]
        public ActionType ActionType { get; set; } = new ActionType();
      //  [JsonPropertyName("timestamp")]
        public int TimeStamp { get; set; }
        public string Type { get; set; } = "PERFORM_ACTION";
    }

    public class ActionType
    {
       //[JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
