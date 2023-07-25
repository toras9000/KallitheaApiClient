using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KallitheaApiClient.Converters;

/// <summary>
/// PropertySet{TValue} 型の値を変換する JsonConverter を生成するファクトリ
/// </summary>
public class PropertySetJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(PropertySet<>);
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(PropertySetJsonConverter<>).MakeGenericType(valueType);
        var valueConverter = options.GetConverter(valueType);

        var converter = (JsonConverter?)Activator.CreateInstance(
            converterType,
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: new object[] { valueConverter },
            culture: null);

        return converter;
    }
}
