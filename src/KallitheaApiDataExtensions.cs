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
}
