namespace KallitheaApiClient.Utils;

/// <summary>
/// データ部のみを取得対象とする kallithea API クライアント
/// </summary>
public class ShuckedKallitheaClient : IDisposable
{
    // 構築
    #region コンストラクタ
    /// <summary>コンストラクタ</summary>
    /// <param name="apiEntry">APIエントリポイント</param>
    /// <param name="apiKey">APIキー。後からの設定可能。</param>
    /// <param name="clientFactory">HttpClient生成デリゲート。IHttpClientFactory による生成を仲介することを推奨。指定しない場合は新しくインスタンスを生成する。</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ShuckedKallitheaClient(Uri apiEntry, string? apiKey = null, Func<HttpClient>? clientFactory = null)
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
    public Task PullAsync(PullArgs args, CancellationToken cancelToken = default)
        => this.client.PullAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリの再スキャンをトリガーする。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RescanReposResult> RescanReposAsync(RescanReposArgs args, CancellationToken cancelToken = default)
        => (await this.client.RescanReposAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result;

    /// <summary>リポジトリのキャッシュを無効化する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task InvalidateCacheAsync(RepoArgs args, CancellationToken cancelToken = default)
        => this.client.InvalidateCacheAsync(args, takeId(), cancelToken);

    /// <summary>ユーザIPホワイトリストとサーバから見たIPを取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GetIpResult> GetIpAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => (await this.client.GetIpAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result;

    /// <summary>サーバ情報を取得する。</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GetServerInfoResult> GetServerInfoAsync(CancellationToken cancelToken = default)
        => (await this.client.GetServerInfoAsync(takeId(), cancelToken).ConfigureAwait(false)).result;

    /// <summary>ユーザ情報を取得する。</summary>
    /// <remarks>管理者権限を持たない場合、自身の情報のみ取得可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GetUserResult> GetUserAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => (await this.client.GetUserAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result;

    /// <summary>ユーザ情報を取得する。</summary>
    /// <remarks>管理者権限を持たない場合、自身の情報のみ取得可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserInfo> GetUserInfoAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => (await this.client.GetUserAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.user;

    /// <summary>ユーザの一覧を取得する</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserInfo[]> GetUsersAsync(CancellationToken cancelToken = default)
        => (await this.client.GetUsersAsync(takeId(), cancelToken).ConfigureAwait(false)).result.users;

    /// <summary>ユーザを作成する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserInfo> CreateUserAsync(CreateUserArgs args, CancellationToken cancelToken = default)
        => (await this.client.CreateUserAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.user;

    /// <summary>ユーザ情報を更新する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserInfo> UpdateUserAsync(UpdateUserArgs args, CancellationToken cancelToken = default)
        => (await this.client.UpdateUserAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.user;

    /// <summary>ユーザを削除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task DeleteUserAsync(UserArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteUserAsync(args, takeId(), cancelToken);

    /// <summary>ユーザグループ情報を取得する。</summary>
    /// <remarks>ユーザグループに対する読み取り以上の権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserGroupInfo> GetUserGroupAsync(UserGroupArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetUserGroupAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.user_group;

    /// <summary>ユーザグループ一覧を取得する。</summary>
    /// <remarks>ユーザグループに対する読み取り以上の権限を持つキーで実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserGroupInfo[]> GetUserGroupsAsync(CancellationToken cancelToken = default)
        => (await this.client.GetUserGroupsAsync(takeId(), cancelToken).ConfigureAwait(false)).result.user_groups;

    /// <summary>ユーザグループを作成する。</summary>
    /// <remarks>ユーザグループの作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserGroupInfo> CreateUserGroupAsync(CreateUserGroupArgs args, CancellationToken cancelToken = default)
        => (await this.client.CreateUserGroupAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.user_group;

    /// <summary>ユーザグループを更新する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserGroupInfo> UpdateUserGroupAsync(UpdateUserGroupArgs args, CancellationToken cancelToken = default)
        => (await this.client.UpdateUserGroupAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.user_group;

    /// <summary>ユーザグループを削除する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<UserGroupInfo> DeleteUserGroupAsync(UserGroupArgs args, CancellationToken cancelToken = default)
        => (await this.client.DeleteUserGroupAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.user_group;

    /// <summary>ユーザグループにユーザを追加する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task AddUserToUserGroupAsync(UserWithUserGroupArgs args, CancellationToken cancelToken = default)
        => this.client.AddUserToUserGroupAsync(args, takeId(), cancelToken);

    /// <summary>ユーザグループからユーザを削除する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task DeleteUserFromUserGroupAsync(UserWithUserGroupArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteUserFromUserGroupAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリの情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GetRepoResult> GetRepoAsync(GetRepoArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetRepoAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result;

    /// <summary>リポジトリの情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RepoInfo> GetRepoInfoAsync(GetRepoArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetRepoAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.repo;

    /// <summary>リポジトリの一覧を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RepoInfo[]> GetReposAsync(CancellationToken cancelToken = default)
        => (await this.client.GetReposAsync(takeId(), cancelToken).ConfigureAwait(false)).result.repos;

    /// <summary>リポジトリのリビジョンに含まれるノード情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RepoNode[]> GetRepoNodesAsync(GetRepoNodesArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetRepoNodesAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.nodes;

    /// <summary>リポジトリを作成する。</summary>
    /// <remarks>リポジトリの作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task CreateRepoAsync(CreateRepoArgs args, CancellationToken cancelToken = default)
        => this.client.CreateRepoAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリを更新する。</summary>
    /// <remarks>リポジトリの書き込み権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RepoInfo> UpdateRepoAsync(UpdateRepoArgs args, CancellationToken cancelToken = default)
        => (await this.client.UpdateRepoAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.repository;

    /// <summary>リポジトリをフォークする。</summary>
    /// <remarks>フォーク元リポジトリの読み取り権限とリポジトリ作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task ForkRepoAsync(ForkRepoArgs args, CancellationToken cancelToken = default)
        => this.client.ForkRepoAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリを削除する。</summary>
    /// <remarks>リポジトリの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task DeleteRepoAsync(DeleteRepoArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteRepoAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリにユーザの権限を設定(追加または更新)する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task GrantUserPermToRepoAsync(GrantUserPermToRepoArgs args, CancellationToken cancelToken = default)
        => this.client.GrantUserPermToRepoAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリからユーザの権限を解除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task RevokeUserPermFromRepoAsync(RevokeUserPermFromRepoArgs args, CancellationToken cancelToken = default)
        => this.client.RevokeUserPermFromRepoAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリにユーザグループの権限を設定(追加または更新)する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task GrantUserGroupPermToRepoAsync(GrantUserGroupPermToRepoArgs args, CancellationToken cancelToken = default)
        => this.client.GrantUserGroupPermToRepoAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリからユーザグループの権限を解除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task RevokeUserGroupPermFromRepoAsync(RevokeUserGroupPermFromRepoArgs args, CancellationToken cancelToken = default)
        => this.client.RevokeUserGroupPermFromRepoAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリグループの情報を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GetRepoGroupResult> GetRepoGroupAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetRepoGroupAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result;

    /// <summary>リポジトリグループの情報を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RepoGroupInfo> GetRepoGroupInfoAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetRepoGroupAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.repogroup;

    /// <summary>リポジトリグループの一覧を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RepoGroupInfo[]> GetRepoGroupsAsync(CancellationToken cancelToken = default)
        => (await this.client.GetRepoGroupsAsync(takeId(), cancelToken).ConfigureAwait(false)).result.repogroups;

    /// <summary>リポジトリグループを作成する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RepoGroupInfo> CreateRepoGroupAsync(CreateRepoGroupArgs args, CancellationToken cancelToken = default)
        => (await this.client.CreateRepoGroupAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.repo_group;

    /// <summary>リポジトリグループを更新する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<RepoGroupInfo> UpdateRepoGroupAsync(UpdateRepoGroupArgs args, CancellationToken cancelToken = default)
        => (await this.client.UpdateRepoGroupAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.repo_group;

    /// <summary>リポジトリグループを削除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task DeleteRepoGroupAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteRepoGroupAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリグループにユーザの権限を設定(追加または更新)する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task GrantUserPermToRepoGroupAsync(GrantUserPermToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.GrantUserPermToRepoGroupAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリグループからユーザの権限を解除する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task RevokeUserPermFromRepoGroupAsync(RevokeUserPermFromRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.RevokeUserPermFromRepoGroupAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリグループにユーザグループの権限を設定(追加または更新)する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task GrantUserGroupPermToRepoGroupAsync(GrantUserGroupPermToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.GrantUserGroupPermToRepoGroupAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリグループからユーザグループの権限を解除する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task RevokeUserGroupPermFromRepoGroupAsync(RevokeUserGroupPermFromToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.client.RevokeUserGroupPermFromRepoGroupAsync(args, takeId(), cancelToken);

    /// <summary>Gist情報を取得する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GistInfo> GetGistAsync(GistArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetGistAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.gist;

    /// <summary>Gist一覧を取得する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GistInfo[]> GetGistsAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => (await this.client.GetGistsAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.gists;

    /// <summary>Gistを作成する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GistInfo> CreateGistAsync(CreateGistArgs args, CancellationToken cancelToken = default)
        => (await this.client.CreateGistAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.gist;

    /// <summary>Gistを削除する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task DeleteGistAsync(GistArgs args, CancellationToken cancelToken = default)
        => this.client.DeleteGistAsync(args, takeId(), cancelToken);

    /// <summary>リポジトリのチェンジセット一覧を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<Changeset[]> GetChangesetsAsync(GetChangesetsArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetChangesetsAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.changesets;

    /// <summary>リポジトリのチェンジセット情報を取得する</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<GetChangesetResult> GetChangesetAsync(GetChangesetArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetChangesetAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result;

    /// <summary>プルリクエスト情報を取得する</summary>
    /// <remarks>プルリクエスト元リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<PullRequest> GetPullRequestAsync(PullRequestArgs args, CancellationToken cancelToken = default)
        => (await this.client.GetPullRequestAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result.pullrequest;

    /// <summary>プルリクエストにコメント追加/状態更新する</summary>
    /// <remarks>プルリクエスト元リポジトリの読み取り権限とレビュア権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task CommentPullRequestAsync(CommentPullRequestArgs args, CancellationToken cancelToken = default)
        => this.client.CommentPullRequestAsync(args, takeId(), cancelToken);

    /// <summary>プルリクエストのレビュアを更新する</summary>
    /// <remarks>プルリクエスト元リポジトリの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public async Task<EditPullRequestReviewersResult> EditPullRequestReviewersAsync(EditPullRequestReviewersArgs args, CancellationToken cancelToken = default)
        => (await this.client.EditPullRequestReviewersAsync(args, takeId(), cancelToken).ConfigureAwait(false)).result;
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
