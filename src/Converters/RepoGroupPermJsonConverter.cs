using System.Text.Json;
using System.Text.Json.Serialization;

namespace KallitheaApiClient.Converters;

/// <summary>
/// RepoGroupPerm 型の値を変換する JsonConverter
/// </summary>
public class RepoGroupPermJsonConverter : JsonConverter<RepoGroupPerm>
{
    // 公開メソッド
    #region 変換処理
    /// <summary>文字列を RepoGroupPerm 値に解釈する</summary>
    /// <param name="name">JSON文字列表現</param>
    /// <returns>解釈された RepoGroupPerm 値</returns>
    public static RepoGroupPerm Parse(string name)
    {
        // 固定のプレフィクスで始まっているかを判別
        if (name != null && name.StartsWith(PermPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            // 続く名称部分が定義に一致するかを判別
            var namePart = name.AsSpan(PermPrefix.Length);
            if (namePart.Equals(nameof(RepoGroupPerm.none).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return RepoGroupPerm.none;
            if (namePart.Equals(nameof(RepoGroupPerm.read).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return RepoGroupPerm.read;
            if (namePart.Equals(nameof(RepoGroupPerm.write).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return RepoGroupPerm.write;
            if (namePart.Equals(nameof(RepoGroupPerm.admin).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return RepoGroupPerm.admin;
        }

        // 変換できなかった場合はエラーとする
        throw new NotSupportedException($"Invalid repository permission '{name}'.");
    }
    #endregion

    #region JSONノード処理
    /// <inheritdoc />
    public override RepoGroupPerm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Parse(reader.GetString() ?? "");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, RepoGroupPerm value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{PermPrefix}{value}");
    }
    #endregion

    // 非公開フィールド
    #region 定数
    /// <summary>JSON表現のプレフィックス</summary>
    private const string PermPrefix = "group.";
    #endregion
}
