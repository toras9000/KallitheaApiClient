using System.Text.Json;
using System.Text.Json.Serialization;

namespace KallitheaApiClient.Converters;

/// <summary>
/// PropertySet{TValue} 型の値を変換する JsonConverter
/// </summary>
public class PropertySetJsonConverter<TValue> : JsonConverter<PropertySet<TValue>>
{
    #region コンストラクタ
    /// <summary>デフォルトコンストラクタ</summary>
    public PropertySetJsonConverter()
    {
    }

    /// <summary>オプションを受け取るコンストラクタ</summary>
    /// <param name="converter">値のコンバータ</param>
    public PropertySetJsonConverter(JsonConverter<TValue> converter)
    {
        this.valueConverter = converter;
    }
    #endregion

    #region JSONノード処理
    /// <inheritdoc />
    public override PropertySet<TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Key/Value コレクションのオブジェクト開始位置であるはず
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

        // 要素位置へ移動
        if (!reader.Read()) throw new JsonException();

        // プロパティと値を収集
        var list = new PropertySet<TValue>();
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            // プロパティ名を読み取り
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            var key = reader.GetString() ?? throw new JsonException();
            if (!reader.Read()) throw new JsonException();

            // 値の読み取り
            var converter = (options.GetConverter(typeof(TValue)) as JsonConverter<TValue>) ?? this.valueConverter;
            var value = converter == null ? JsonSerializer.Deserialize<TValue>(ref reader)
                      : converter.Read(ref reader, typeof(TValue), options);
            if (value == null) throw new JsonException();
            if (!reader.Read()) throw new JsonException();

            // 保持
            list.Add(new(key, value));
        }

        // 読み取り値を返却
        return list;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PropertySet<TValue> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var item in value)
        {
            writer.WritePropertyName(item.name);
            JsonSerializer.Serialize(writer, item.value);
        }
        writer.WriteEndObject();
    }
    #endregion

    // 非公開フィールド
    #region オプション
    /// <summary>値のコンバータ</summary>
    private JsonConverter<TValue>? valueConverter;
    #endregion
}
