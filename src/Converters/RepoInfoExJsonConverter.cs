using System.Text.Json;
using System.Text.Json.Serialization;

namespace KallitheaApiClient.Converters;

/// <summary>
/// RepoInfoEx 型の値を変換する JsonConverter
/// </summary>
public class RepoInfoExJsonConverter : JsonConverter<RepoInfoEx>
{
    // 公開メソッド
    #region JSONノード処理
    /// <inheritdoc />
    public override RepoInfoEx? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 対応位置がオブジェクト開始であることを判定
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

        // リーダー構造体のコピーで拡張フィールドの情報を読み取る。
        // 他の情報を読み取る前の状態をコピーしてオブジェクト内のプロパティを参照する必要がある。
        var fields = collectExtraFields(reader);

        // オブジェクトのリポジトリ情報を読みとり
        var repo = JsonSerializer.Deserialize<RepoInfo>(ref reader) ?? throw new JsonException();

        // リポジトリ情報と拡張フィールド情報をコンポジット型で返却
        return new RepoInfoEx(repo, fields);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, RepoInfoEx value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
    #endregion

    // 非公開フィールド
    #region 定数
    /// <summary>拡張フィールドの情報に付与されるプレフィックス</summary>
    private const string ExtraFieldPrefix = "ex_";
    #endregion

    // 非公開メソッド
    #region 拡張フィールド
    /// <summary>リポジトリ情報JSONから拡張フィールド情報を読み取る</summary>
    /// <param name="reader">JSONリーダ</param>
    /// <returns>読み取った拡張フィールド情報。無かった場合は null</returns>
    private ExtraField[]? collectExtraFields(Utf8JsonReader reader)
    {
        // 対応位置がオブジェクト開始であることを判定
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

        // オブジェクト内に読み進める
        if (!reader.Read()) throw new JsonException();

        // オブジェクト内の拡張フィールドを読み取る
        var fields = default(List<ExtraField>);
        while (reader.TokenType != JsonTokenType.EndObject)
        {
            // このコンテキストはプロパティ名であるはず
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();

            // プロパティ名を読み取り
            var key = reader.GetString() ?? throw new JsonException();

            // プロパティ値へ読み進める
            if (!reader.Read()) throw new JsonException();

            // プロパティが拡張フィールドであるかを判定
            if (key.StartsWith(ExtraFieldPrefix) && reader.TokenType == JsonTokenType.String)
            {
                // 拡張フィールドであればプロパティ値を取得
                var value = reader.GetString();
                if (value != null)
                {
                    // 有効な拡張フィールドであれば収集
                    (fields ??= new()).Add(new(key[ExtraFieldPrefix.Length..], value));
                }
            }

            // オブジェクトまたは配列の場合は全体をスキップ(それぞれのEndまで進む)
            if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray) reader.Skip();

            // 次のプロパティに読み進める
            if (!reader.Read()) throw new JsonException();
        }

        return fields?.ToArray();
    }
    #endregion
}
