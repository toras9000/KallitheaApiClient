using System.Text.Json;
using System.Text.Json.Serialization;

namespace KallitheaApiClient.Converters;

/// <summary>
/// IpRange 型の値を変換する JsonConverter
/// </summary>
public class IpRangeJsonConverter : JsonConverter<IpRange>
{
    #region JSONノード処理
    /// <inheritdoc />
    public override IpRange? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // IpRangeは2要素の文字列配列表現となっている。
        // 対応位置が配列開始であることを判定
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException();

        // 1要素目の値を取得
        if (!reader.Read()) throw new JsonException();
        if (reader.TokenType != JsonTokenType.String) throw new JsonException();
        var start = reader.GetString() ?? throw new JsonException();

        // 2要素目の値を取得
        if (!reader.Read()) throw new JsonException();
        if (reader.TokenType != JsonTokenType.String) throw new JsonException();
        var end = reader.GetString() ?? throw new JsonException();

        // 配列の終わりまで読み飛ばす
        while (reader.TokenType != JsonTokenType.EndArray && reader.Read()) ;

        // 読み取り値を返却
        return new IpRange(start, end);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IpRange value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.start_ip);
        writer.WriteStringValue(value.end_ip);
        writer.WriteEndArray();
    }
    #endregion
}
