using System.Text.Json;
using System.Text.Json.Serialization;

namespace KallitheaApiClient.Converters;

/// <summary>
/// ModuleInfo 型の値を変換する JsonConverter
/// </summary>
public class ModuleInfoJsonConverter : JsonConverter<ModuleInfo>
{
    #region JSONノード処理
    /// <inheritdoc />
    public override ModuleInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ModuleInfo は2要素の文字列配列表現となっている。
        // 対応位置が配列開始であることを判定
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();

        // 1要素目の値を取得
        if (!reader.Read()) throw new JsonException();
        if (reader.TokenType != JsonTokenType.String) throw new JsonException();
        var name = reader.GetString() ?? throw new JsonException();

        // 2要素目の値を取得
        if (!reader.Read()) throw new JsonException();
        if (reader.TokenType != JsonTokenType.String) throw new JsonException();
        var version = reader.GetString() ?? throw new JsonException();

        // 配列の終わりまで読み飛ばす
        while (reader.TokenType != JsonTokenType.EndArray && reader.Read()) ;

        // 読み取り値を返却
        return new ModuleInfo(name, version);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ModuleInfo value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.name);
        writer.WriteStringValue(value.version);
        writer.WriteEndArray();
    }
    #endregion
}
