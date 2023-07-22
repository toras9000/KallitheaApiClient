using KallitheaApiClient.Converters;

namespace KallitheaApiClient;

/// <summary>
/// KallitheaApiData 関連拡張メソッド
/// </summary>
public static class KallitheaApiDataExtensions
{
    /// <summary>リポジトリパーミッションをパーミッション名文字列に変換する</summary>
    /// <param name="self">リポジトリパーミッション</param>
    /// <returns>パーミッション名文字列</returns>
    public static string ToPermName(this RepoPerm self)
    {
        return RepoPermJsonConverter.PermPrefix + self.ToString();
    }

    /// <summary>リポジトリグループパーミッションをパーミッション名文字列に変換する</summary>
    /// <param name="self">リポジトリグループパーミッション</param>
    /// <returns>パーミッション名文字列</returns>
    public static string ToPermName(this RepoGroupPerm self)
    {
        return RepoGroupPermJsonConverter.PermPrefix + self.ToString();
    }

    /// <summary>ユーザグループパーミッションをパーミッション名文字列に変換する</summary>
    /// <param name="self">ユーザグループパーミッション</param>
    /// <returns>パーミッション名文字列</returns>
    public static string ToPermName(this UserGroupPerm self)
    {
        return UserGroupPermJsonConverter.PermPrefix + self.ToString();
    }

    /// <summary>API要求タスクを主要なレスポンス値のみを返すタスクにアンラップする</summary>
    /// <typeparam name="TResult">APIレスポンスの応答データ型</typeparam>
    /// <param name="self">API要求タスク</param>
    /// <returns>応答データを得るタスク</returns>
    public static async Task<TResult> UnwrapResponse<TResult>(this Task<ApiResponse<TResult>> self)
    {
        var response = await self.ConfigureAwait(false);
        return response.result;
    }

    /// <summary>API要求タスクを主要なレスポンス値からの変換結果のみを返すタスクにアンラップする</summary>
    /// <typeparam name="TResult">APIレスポンスの応答データ型</typeparam>
    /// <typeparam name="TConverted">変換結果のデータ型</typeparam>
    /// <param name="self">API要求タスク</param>
    /// <param name="converter">レスポンス値の変換デリゲート</param>
    /// <returns>応答データを得るタスク</returns>
    public static async Task<TConverted> ConvertResponse<TResult, TConverted>(this Task<ApiResponse<TResult>> self, Func<TResult, TConverted> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        var response = await self.ConfigureAwait(false);
        return converter(response.result);
    }

}
