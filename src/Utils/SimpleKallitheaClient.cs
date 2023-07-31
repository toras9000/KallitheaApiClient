using KallitheaApiClient;

namespace KallitheaApiClient.Utils;

/// <summary>
/// データ部のみを取得対象とする kallithea API クライアント
/// </summary>
public class SimpleKallitheaClient : IDisposable
{
    // 構築
    #region コンストラクタ
    /// <summary>コンストラクタ</summary>
    /// <param name="apiEntry">APIエントリポイント</param>
    /// <param name="apiKey">APIキー。後からの設定可能。</param>
    /// <param name="clientFactory">HttpClient生成デリゲート。IHttpClientFactory による生成を仲介することを推奨。指定しない場合は新しくインスタンスを生成する。</param>
    /// <exception cref="ArgumentNullException"></exception>
    public SimpleKallitheaClient(Uri apiEntry, string? apiKey = null, Func<HttpClient>? clientFactory = null)
    {
        this.client = new KallitheaClient(apiEntry, apiKey, clientFactory);
    }
    #endregion

    // 公開プロパティ
    #region APIアクセス情報
    /// <summary>APIエントリポイント</summary>
    public Uri ApiEntry => this.client.ApiEntry;

    /// <summary>APIキー</summary>
    public string? ApiKey
    {
        get => this.client.ApiKey;
        set => this.client.ApiKey = value;
    }
    #endregion

    // 公開イベント
    #region ログ
    /// <summary>API要求ログ</summary>
    public event Action<ApiLog>? Logging
    {
        add => this.client.Logging += value;
        remove => this.client.Logging -= value;
    }
    #endregion

    // 公開メソッド
    #region API呼び出し
    /// <summary>リモートからのpullをトリガーする。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<PullResult> PullAsync(PullArgs args, CancellationToken cancelToken = default)
        => this.client.PullAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリの再スキャンをトリガーする。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RescanReposResult> RescanReposAsync(RescanReposArgs args, CancellationToken cancelToken = default)
        => this.client.RescanReposAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリのキャッシュを無効化する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<InvalidateCacheResult> InvalidateCacheAsync(RepoArgs args, CancellationToken cancelToken = default)
        => this.client.InvalidateCacheAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザIPホワイトリストとサーバから見たIPを取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetIpResult> GetIpAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => this.client.GetIpAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>サーバ情報を取得する。</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetServerInfoResult> GetServerInfoAsync(CancellationToken cancelToken = default)
        => this.client.GetServerInfoAsync(takeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザ情報を取得する。</summary>
    /// <remarks>管理者権限を持たない場合、自身の情報のみ取得可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetUserResult> GetUserAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => this.client.GetUserAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザ情報を取得する。</summary>
    /// <remarks>管理者権限を持たない場合、自身の情報のみ取得可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserInfo> GetUserInfoAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => this.client.GetUserAsync(args, takeId(), cancelToken).ConvertResponse(r => r.user);

    /// <summary>ユーザの一覧を取得する</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserInfo[]> GetUsersAsync(CancellationToken cancelToken = default)
        => this.client.GetUsersAsync(takeId(), cancelToken).ConvertResponse(r => r.users);

    /// <summary>ユーザを作成する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserInfo> CreateUserAsync(CreateUserArgs args, CancellationToken cancelToken = default)
        => this.client.CreateUserAsync(args, takeId(), cancelToken).ConvertResponse(r => r.user);

    /// <summary>ユーザ情報を更新する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserInfo> UpdateUserAsync(UpdateUserArgs args, CancellationToken cancelToken = default)
        => this.client.UpdateUserAsync(args, takeId(), cancelToken).ConvertResponse(r => r.user);

    /// <summary>ユーザを削除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task DeleteUserAsync(UserArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteUserAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザグループ情報を取得する。</summary>
    /// <remarks>ユーザグループに対する読み取り以上の権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo> GetUserGroupAsync(UserGroupArgs args, CancellationToken cancelToken = default)
        => this.client.GetUserGroupAsync(args, takeId(), cancelToken).ConvertResponse(r => r.user_group);

    /// <summary>ユーザグループ一覧を取得する。</summary>
    /// <remarks>ユーザグループに対する読み取り以上の権限を持つキーで実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo[]> GetUserGroupsAsync(CancellationToken cancelToken = default)
        => this.client.GetUserGroupsAsync(takeId(), cancelToken).ConvertResponse(r => r.user_groups);

    /// <summary>ユーザグループを作成する。</summary>
    /// <remarks>ユーザグループの作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo> CreateUserGroupAsync(CreateUserGroupArgs args, CancellationToken cancelToken = default)
        => this.client.CreateUserGroupAsync(args, takeId(), cancelToken).ConvertResponse(r => r.user_group);

    /// <summary>ユーザグループを更新する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo> UpdateUserGroupAsync(UpdateUserGroupArgs args, CancellationToken cancelToken = default)
        => this.client.UpdateUserGroupAsync(args, takeId(), cancelToken).ConvertResponse(r => r.user_group);

    /// <summary>ユーザグループを削除する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo> DeleteUserGroupAsync(UserGroupArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteUserGroupAsync(args, takeId(), cancelToken).ConvertResponse(r => r.user_group);

    /// <summary>ユーザグループにユーザを追加する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<AddUserToUserGroupResult> AddUserToUserGroupAsync(UserWithUserGroupArgs args, CancellationToken cancelToken = default)
        => this.client.AddUserToUserGroupAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザグループからユーザを削除する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RemoveUserToUserGroupResult> RemoveUserFromUserGroupAsync(UserWithUserGroupArgs args, CancellationToken cancelToken = default)
        => this.client.RemoveUserFromUserGroupAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリの情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetRepoResult> GetRepoAsync(GetRepoArgs args, CancellationToken cancelToken = default)
        => this.client.GetRepoAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリの情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoInfoEx> GetRepoInfoAsync(GetRepoArgs args, CancellationToken cancelToken = default)
        => this.client.GetRepoAsync(args, takeId(), cancelToken).ConvertResponse(r => r.repo);

    /// <summary>リポジトリの一覧を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoInfoEx[]> GetReposAsync(CancellationToken cancelToken = default)
        => this.client.GetReposAsync(takeId(), cancelToken).ConvertResponse(r => r.repos);

    /// <summary>リポジトリのリビジョンに含まれるノード情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoNode[]> GetRepoNodesAsync(GetRepoNodesArgs args, CancellationToken cancelToken = default)
        => this.client.GetRepoNodesAsync(args, takeId(), cancelToken).ConvertResponse(r => r.nodes);

    /// <summary>リポジトリを作成する。</summary>
    /// <remarks>リポジトリの作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<CreateRepoResult> CreateRepoAsync(CreateRepoArgs args, CancellationToken cancelToken = default)
        => this.client.CreateRepoAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリを更新する。</summary>
    /// <remarks>リポジトリの書き込み権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoInfoEx> UpdateRepoAsync(UpdateRepoArgs args, CancellationToken cancelToken = default)
        => this.client.UpdateRepoAsync(args, takeId(), cancelToken).ConvertResponse(r => r.repository);

    /// <summary>リポジトリをフォークする。</summary>
    /// <remarks>フォーク元リポジトリの読み取り権限とリポジトリ作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<ForkRepoResult> ForkRepoAsync(ForkRepoArgs args, CancellationToken cancelToken = default)
        => this.client.ForkRepoAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリを削除する。</summary>
    /// <remarks>リポジトリの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<DeleteRepoResult> DeleteRepoAsync(DeleteRepoArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteRepoAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリにユーザの権限を設定(追加または更新)する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GrantUserPermToRepoResult> GrantUserPermToRepoAsync(GrantUserPermToRepoArgs args, CancellationToken cancelToken = default)
        => this.client.GrantUserPermToRepoAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリからユーザの権限を解除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RevokeUserPermFromRepoResult> RevokeUserPermFromRepoAsync(RevokeUserPermFromRepoArgs args, CancellationToken cancelToken = default)
        => this.client.RevokeUserPermFromRepoAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリにユーザグループの権限を設定(追加または更新)する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GrantUserGroupPermToRepoResult> GrantUserGroupPermToRepoAsync(GrantUserGroupPermToRepoArgs args, CancellationToken cancelToken = default)
        => this.client.GrantUserGroupPermToRepoAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリからユーザグループの権限を解除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RevokeUserGroupPermFromRepoResult> RevokeUserGroupPermFromRepoAsync(RevokeUserGroupPermFromRepoArgs args, CancellationToken cancelToken = default)
        => this.client.RevokeUserGroupPermFromRepoAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループの情報を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetRepoGroupResult> GetRepoGroupAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.GetRepoGroupAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループの情報を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoGroupInfo> GetRepoGroupInfoAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.GetRepoGroupAsync(args, takeId(), cancelToken).ConvertResponse(r => r.repogroup);

    /// <summary>リポジトリグループの一覧を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoGroupInfo[]> GetRepoGroupsAsync(CancellationToken cancelToken = default)
        => this.client.GetRepoGroupsAsync(takeId(), cancelToken).ConvertResponse(r => r.repogroups);

    /// <summary>リポジトリグループを作成する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoGroupInfo> CreateRepoGroupAsync(CreateRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.CreateRepoGroupAsync(args, takeId(), cancelToken).ConvertResponse(r => r.repo_group);

    /// <summary>リポジトリグループを更新する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoGroupInfo> UpdateRepoGroupAsync(UpdateRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.UpdateRepoGroupAsync(args, takeId(), cancelToken).ConvertResponse(r => r.repo_group);

    /// <summary>リポジトリグループを削除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<DeleteRepoGroupResult> DeleteRepoGroupAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteRepoGroupAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループにユーザの権限を設定(追加または更新)する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GrantUserPermToRepoGroupResult> GrantUserPermToRepoGroupAsync(GrantUserPermToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.GrantUserPermToRepoGroupAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループからユーザの権限を解除する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RevokeUserPermFromRepoGroupResult> RevokeUserPermFromRepoGroupAsync(RevokeUserPermFromRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.RevokeUserPermFromRepoGroupAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループにユーザグループの権限を設定(追加または更新)する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GrantUserGroupPermToRepoGroupResult> GrantUserGroupPermToRepoGroupAsync(GrantUserGroupPermToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.GrantUserGroupPermToRepoGroupAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループからユーザグループの権限を解除する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RevokeUserGroupPermFromRepoGroupResult> RevokeUserGroupPermFromRepoGroupAsync(RevokeUserGroupPermFromToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.RevokeUserGroupPermFromRepoGroupAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>Gist情報を取得する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GistInfo> GetGistAsync(GistArgs args, CancellationToken cancelToken = default)
        => this.client.GetGistAsync(args, takeId(), cancelToken).ConvertResponse(r => r.gist);

    /// <summary>Gist一覧を取得する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GistInfo[]> GetGistsAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => this.client.GetGistsAsync(args, takeId(), cancelToken).ConvertResponse(r => r.gists);

    /// <summary>Gistを作成する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GistInfo> CreateGistAsync(CreateGistArgs args, CancellationToken cancelToken = default)
        => this.client.CreateGistAsync(args, takeId(), cancelToken).ConvertResponse(r => r.gist);

    /// <summary>Gistを削除する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<DeleteGistResult> DeleteGistAsync(GistArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteGistAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリのチェンジセット一覧を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<Changeset[]> GetChangesetsAsync(GetChangesetsArgs args, CancellationToken cancelToken = default)
        => this.client.GetChangesetsAsync(args, takeId(), cancelToken).ConvertResponse(r => r.changesets);

    /// <summary>リポジトリのチェンジセット情報を取得する</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetChangesetResult> GetChangesetAsync(GetChangesetArgs args, CancellationToken cancelToken = default)
        => this.client.GetChangesetAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>プルリクエスト情報を取得する</summary>
    /// <remarks>プルリクエスト元リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<PullRequest> GetPullRequestAsync(PullRequestArgs args, CancellationToken cancelToken = default)
        => this.client.GetPullRequestAsync(args, takeId(), cancelToken).ConvertResponse(r => r.pullrequest);

    /// <summary>プルリクエストにコメント追加/状態更新する</summary>
    /// <remarks>プルリクエスト元リポジトリの読み取り権限とレビュア権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<CommentPullRequestResult> CommentPullRequestAsync(CommentPullRequestArgs args, CancellationToken cancelToken = default)
        => this.client.CommentPullRequestAsync(args, takeId(), cancelToken).UnwrapResponse();

    /// <summary>プルリクエストのレビュアを更新する</summary>
    /// <remarks>プルリクエスト元リポジトリの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<EditPullRequestReviewersResult> EditPullRequestReviewersAsync(EditPullRequestReviewersArgs args, CancellationToken cancelToken = default)
        => this.client.EditPullRequestReviewersAsync(args, takeId(), cancelToken).UnwrapResponse();
    #endregion

    #region 破棄
    /// <summary>リソース破棄</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    // 保護メソッド
    #region 破棄
    /// <summary>リソース破棄</summary>
    /// <param name="disposing">マネージ破棄過程であるか否か</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            // マネージ破棄過程であればマネージオブジェクトを破棄する
            if (disposing)
            {
                this.client?.Dispose();
            }

            // 破棄済みマーク
            this.isDisposed = true;
        }
    }
    #endregion

    // 非公開フィールド
    #region リソース
    /// <summary>APIクライアント</summary>
    private readonly KallitheaClient client;
    #endregion

    #region 状態フラグ
    /// <summary>要求ID</summary>
    private uint id;

    /// <summary>破棄済みフラグ</summary>
    private bool isDisposed;
    #endregion

    // 非公開メソッド
    #region リソース
    /// <summary>要求IDを取得する</summary>
    /// <returns>要求ID</returns>
    private string takeId()
    {
        var value = Interlocked.Increment(ref this.id);
        return value.ToString();
    }
    #endregion
}
