using System.Text.Json;
using System.Text.Json.Serialization;

namespace KallitheaApiClient.Converters;

/// <summary>
/// UserGroupPerm 型の値を変換する JsonConverter
/// </summary>
public class UserGroupPermJsonConverter : JsonConverter<UserGroupPerm>
{
    // 公開フィールド
    #region 定数
    /// <summary>JSON表現のプレフィックス</summary>
    public const string PermPrefix = "usergroup.";
    #endregion

    // 公開メソッド
    #region 変換処理
    /// <summary>文字列を UserGroupPerm 値に解釈する</summary>
    /// <param name="name">JSON文字列表現</param>
    /// <returns>解釈された UserGroupPerm 値</returns>
    public static UserGroupPerm Parse(string name)
    {
        // 固定のプレフィクスで始まっているかを判別
        if (name != null && name.StartsWith(PermPrefix, StringComparison.InvariantCultureIgnoreCase))
        {
            // 続く名称部分が定義に一致するかを判別
            var namePart = name.AsSpan(PermPrefix.Length);
            if (namePart.Equals(nameof(UserGroupPerm.none).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return UserGroupPerm.none;
            if (namePart.Equals(nameof(UserGroupPerm.read).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return UserGroupPerm.read;
            if (namePart.Equals(nameof(UserGroupPerm.write).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return UserGroupPerm.write;
            if (namePart.Equals(nameof(UserGroupPerm.admin).AsSpan(), StringComparison.InvariantCultureIgnoreCase)) return UserGroupPerm.admin;
        }

        // 変換できなかった場合はエラーとする
        throw new NotSupportedException($"Invalid repository permission '{name}'.");
    }
    #endregion

    #region JSONノード処理
    /// <inheritdoc />
    public override UserGroupPerm Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Parse(reader.GetString() ?? "");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, UserGroupPerm value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{PermPrefix}{value}");
    }
    #endregion
}
