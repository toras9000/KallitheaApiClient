using System.Text.Json;
using System.Text.Json.Serialization;

namespace KallitheaApiClient.Converters;

/// <summary>
/// RepoPerm 型の値を変換する JsonConverter
/// </summary>
public class RepoPermJsonConverter : JsonConverter<RepoPerm>
{
    // 公開メソッド
    #region 変換処理
    /// <summary>文字列を RepoPerm 値に解釈する</summary>
    /// <param name="name">JSON文字列表現</param>
    /// <returns>解釈された RepoPerm 値</returns>
    public static RepoPerm Parse(string name)
    {
        // 固定のプレフィクスで始まっているかを判別
        if (name != null && name.StartsWith(PermPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            // 続く名称部分が定義に一致するかを判別
            var namePart = name.AsSpan(PermPrefix.Length);
            if (namePart.Equals(nameof(RepoPerm.none).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return RepoPerm.none;
            if (namePart.Equals(nameof(RepoPerm.read).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return RepoPerm.read;
            if (namePart.Equals(nameof(RepoPerm.write).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return RepoPerm.write;
            if (namePart.Equals(nameof(RepoPerm.admin).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return RepoPerm.admin;
        }

        // 変換できなかった場合はエラーとする
        throw new NotSupportedException($"Invalid repository permission '{name}'.");
    }
    #endregion

    #region JSONノード処理
    /// <inheritdoc />
    public override RepoPerm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Parse(reader.GetString() ?? "");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, RepoPerm value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{PermPrefix}{value}");
    }
    #endregion

    // 非公開フィールド
    #region 定数
    /// <summary>JSON表現のプレフィックス</summary>
    private const string PermPrefix = "repository.";
    #endregion
}
