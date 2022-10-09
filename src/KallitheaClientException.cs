namespace KallitheaApiClient;

/// <summary>
/// KallitheaClient で利用する例外の基本クラス
/// </summary>
public class KallitheaClientException : Exception
{
    // 構築
    #region コンストラクタ
    /// <summary>デフォルトコンストラクタ</summary>
    public KallitheaClientException() : base() { }

    /// <summary>メッセージを指定するコンストラクタ</summary>
    /// <param name="message">例外メッセージ</param>
    public KallitheaClientException(string? message) : base(message) { }

    /// <summary>例外メッセージと内部例外を指定するコンストラクタ</summary>
    /// <param name="message">例外メッセージ</param>
    /// <param name="innerException">内部例外</param>
    public KallitheaClientException(string? message, Exception? innerException) : base(message, innerException) { }
    #endregion
}

/// <summary>
/// API応答のエラーを示す例外クラス
/// </summary>
public class ResponseException : KallitheaClientException
{
    // 構築
    #region コンストラクタ
    /// <summary>要求識別子と例外メッセージを指定するコンストラクタ</summary>
    /// <param name="id">要求識別子</param>
    /// <param name="message">例外メッセージ</param>
    public ResponseException(string id, string message) : base(message) { this.Id = id; }
    #endregion

    // 公開プロパティ
    #region コンテキスト情報
    /// <summary>要求識別子</summary>
    public string Id { get; }
    #endregion
}

/// <summary>
/// API要求に対する予期しない応答を表す例外クラス
/// </summary>
public class UnexpectedResponseException : ResponseException
{
    // 構築
    #region コンストラクタ
    /// <summary>要求識別子と例外メッセージを指定するコンストラクタ</summary>
    /// <param name="id">要求識別子</param>
    /// <param name="message">例外メッセージ</param>
    public UnexpectedResponseException(string id, string message) : base(id, message) { }
    #endregion
}

/// <summary>
/// API要求に対するエラー応答を表す例外クラス
/// </summary>
public class ErrorResponseException : ResponseException
{
    // 構築
    #region コンストラクタ
    /// <summary>要求識別子とエラー応答メッセージを指定するコンストラクタ</summary>
    /// <param name="id">要求識別子</param>
    /// <param name="error">応答に含まれたエラーメッセージ</param>
    public ErrorResponseException(string id, string error) : base(id, error) { }
    #endregion
}

/// <summary>
/// API要求の結果値デコードエラーを表す例外クラス
/// </summary>
public class UnexpectedResultException : ResponseException
{
    // 構築
    #region コンストラクタ
    /// <summary>要求識別子とデコードを試みたデータの識別名を指定するコンストラクタ</summary>
    /// <param name="id">要求識別子</param>
    /// <param name="name">例外メッセージ</param>
    public UnexpectedResultException(string id, string name) : base(id, $"Cannot decode '{name}'")
    {
        this.Name = name;
    }
    #endregion

    // 公開プロパティ
    #region コンテキスト情報
    /// <summary>データの識別名</summary>
    public string Name { get; }
    #endregion
}

