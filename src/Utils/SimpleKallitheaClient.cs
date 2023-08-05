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
        this.Client = new KallitheaClient(apiEntry, apiKey, clientFactory);
    }
    #endregion

    // 公開プロパティ
    #region APIアクセス情報
    /// <summary>APIエントリポイント</summary>
    public Uri ApiEntry => this.Client.ApiEntry;

    /// <summary>APIキー</summary>
    public string? ApiKey
    {
        get => this.Client.ApiKey;
        set => this.Client.ApiKey = value;
    }
    #endregion

    // 公開イベント
    #region ログ
    /// <summary>API要求ログ</summary>
    public event Action<ApiLog>? Logging
    {
        add => this.Client.Logging += value;
        remove => this.Client.Logging -= value;
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
        => this.Client.PullAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリの再スキャンをトリガーする。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RescanReposResult> RescanReposAsync(RescanReposArgs args, CancellationToken cancelToken = default)
        => this.Client.RescanReposAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリのキャッシュを無効化する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<InvalidateCacheResult> InvalidateCacheAsync(RepoArgs args, CancellationToken cancelToken = default)
        => this.Client.InvalidateCacheAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザIPホワイトリストとサーバから見たIPを取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetIpResult> GetIpAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => this.Client.GetIpAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>サーバ情報を取得する。</summary>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetServerInfoResult> GetServerInfoAsync(CancellationToken cancelToken = default)
        => this.Client.GetServerInfoAsync(TakeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザ情報を取得する。</summary>
    /// <remarks>管理者権限を持たない場合、自身の情報のみ取得可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetUserResult> GetUserAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => this.Client.GetUserAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザ情報を取得する。</summary>
    /// <remarks>管理者権限を持たない場合、自身の情報のみ取得可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserInfo> GetUserInfoAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => this.Client.GetUserAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.user);

    /// <summary>ユーザの一覧を取得する</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserInfo[]> GetUsersAsync(CancellationToken cancelToken = default)
        => this.Client.GetUsersAsync(TakeId(), cancelToken).ConvertResponse(r => r.users);

    /// <summary>ユーザを作成する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserInfo> CreateUserAsync(CreateUserArgs args, CancellationToken cancelToken = default)
        => this.Client.CreateUserAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.user);

    /// <summary>ユーザ情報を更新する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserInfo> UpdateUserAsync(UpdateUserArgs args, CancellationToken cancelToken = default)
        => this.Client.UpdateUserAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.user);

    /// <summary>ユーザを削除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task DeleteUserAsync(UserArgs args, CancellationToken cancelToken = default)
        => this.Client.DeleteUserAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザグループ情報を取得する。</summary>
    /// <remarks>ユーザグループに対する読み取り以上の権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo> GetUserGroupAsync(UserGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.GetUserGroupAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.user_group);

    /// <summary>ユーザグループ一覧を取得する。</summary>
    /// <remarks>ユーザグループに対する読み取り以上の権限を持つキーで実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo[]> GetUserGroupsAsync(CancellationToken cancelToken = default)
        => this.Client.GetUserGroupsAsync(TakeId(), cancelToken).ConvertResponse(r => r.user_groups);

    /// <summary>ユーザグループを作成する。</summary>
    /// <remarks>ユーザグループの作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo> CreateUserGroupAsync(CreateUserGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.CreateUserGroupAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.user_group);

    /// <summary>ユーザグループを更新する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo> UpdateUserGroupAsync(UpdateUserGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.UpdateUserGroupAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.user_group);

    /// <summary>ユーザグループを削除する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<UserGroupInfo> DeleteUserGroupAsync(UserGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.DeleteUserGroupAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.user_group);

    /// <summary>ユーザグループにユーザを追加する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<AddUserToUserGroupResult> AddUserToUserGroupAsync(UserWithUserGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.AddUserToUserGroupAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>ユーザグループからユーザを削除する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RemoveUserToUserGroupResult> RemoveUserFromUserGroupAsync(UserWithUserGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.RemoveUserFromUserGroupAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリの情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetRepoResult> GetRepoAsync(GetRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.GetRepoAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリの情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoInfoEx> GetRepoInfoAsync(GetRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.GetRepoAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.repo);

    /// <summary>リポジトリの一覧を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoInfoEx[]> GetReposAsync(CancellationToken cancelToken = default)
        => this.Client.GetReposAsync(TakeId(), cancelToken).ConvertResponse(r => r.repos);

    /// <summary>リポジトリのリビジョンに含まれるノード情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoNode[]> GetRepoNodesAsync(GetRepoNodesArgs args, CancellationToken cancelToken = default)
        => this.Client.GetRepoNodesAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.nodes);

    /// <summary>リポジトリを作成する。</summary>
    /// <remarks>リポジトリの作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<CreateRepoResult> CreateRepoAsync(CreateRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.CreateRepoAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリを更新する。</summary>
    /// <remarks>リポジトリの書き込み権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoInfoEx> UpdateRepoAsync(UpdateRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.UpdateRepoAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.repository);

    /// <summary>リポジトリをフォークする。</summary>
    /// <remarks>フォーク元リポジトリの読み取り権限とリポジトリ作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<ForkRepoResult> ForkRepoAsync(ForkRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.ForkRepoAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリを削除する。</summary>
    /// <remarks>リポジトリの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<DeleteRepoResult> DeleteRepoAsync(DeleteRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.DeleteRepoAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリにユーザの権限を設定(追加または更新)する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GrantUserPermToRepoResult> GrantUserPermToRepoAsync(GrantUserPermToRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.GrantUserPermToRepoAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリからユーザの権限を解除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RevokeUserPermFromRepoResult> RevokeUserPermFromRepoAsync(RevokeUserPermFromRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.RevokeUserPermFromRepoAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリにユーザグループの権限を設定(追加または更新)する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GrantUserGroupPermToRepoResult> GrantUserGroupPermToRepoAsync(GrantUserGroupPermToRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.GrantUserGroupPermToRepoAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリからユーザグループの権限を解除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RevokeUserGroupPermFromRepoResult> RevokeUserGroupPermFromRepoAsync(RevokeUserGroupPermFromRepoArgs args, CancellationToken cancelToken = default)
        => this.Client.RevokeUserGroupPermFromRepoAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループの情報を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetRepoGroupResult> GetRepoGroupAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.GetRepoGroupAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループの情報を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoGroupInfo> GetRepoGroupInfoAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.GetRepoGroupAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.repogroup);

    /// <summary>リポジトリグループの一覧を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoGroupInfo[]> GetRepoGroupsAsync(CancellationToken cancelToken = default)
        => this.Client.GetRepoGroupsAsync(TakeId(), cancelToken).ConvertResponse(r => r.repogroups);

    /// <summary>リポジトリグループを作成する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoGroupInfo> CreateRepoGroupAsync(CreateRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.CreateRepoGroupAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.repo_group);

    /// <summary>リポジトリグループを更新する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RepoGroupInfo> UpdateRepoGroupAsync(UpdateRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.UpdateRepoGroupAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.repo_group);

    /// <summary>リポジトリグループを削除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<DeleteRepoGroupResult> DeleteRepoGroupAsync(RepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.DeleteRepoGroupAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループにユーザの権限を設定(追加または更新)する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GrantUserPermToRepoGroupResult> GrantUserPermToRepoGroupAsync(GrantUserPermToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.GrantUserPermToRepoGroupAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループからユーザの権限を解除する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RevokeUserPermFromRepoGroupResult> RevokeUserPermFromRepoGroupAsync(RevokeUserPermFromRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.RevokeUserPermFromRepoGroupAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループにユーザグループの権限を設定(追加または更新)する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GrantUserGroupPermToRepoGroupResult> GrantUserGroupPermToRepoGroupAsync(GrantUserGroupPermToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.GrantUserGroupPermToRepoGroupAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリグループからユーザグループの権限を解除する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<RevokeUserGroupPermFromRepoGroupResult> RevokeUserGroupPermFromRepoGroupAsync(RevokeUserGroupPermFromToRepoGroupArgs args, CancellationToken cancelToken = default)
        => this.Client.RevokeUserGroupPermFromRepoGroupAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>Gist情報を取得する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GistInfo> GetGistAsync(GistArgs args, CancellationToken cancelToken = default)
        => this.Client.GetGistAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.gist);

    /// <summary>Gist一覧を取得する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GistInfo[]> GetGistsAsync(UserArgs? args = null, CancellationToken cancelToken = default)
        => this.Client.GetGistsAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.gists);

    /// <summary>Gistを作成する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GistInfo> CreateGistAsync(CreateGistArgs args, CancellationToken cancelToken = default)
        => this.Client.CreateGistAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.gist);

    /// <summary>Gistを削除する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<DeleteGistResult> DeleteGistAsync(GistArgs args, CancellationToken cancelToken = default)
        => this.Client.DeleteGistAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>リポジトリのチェンジセット一覧を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<Changeset[]> GetChangesetsAsync(GetChangesetsArgs args, CancellationToken cancelToken = default)
        => this.Client.GetChangesetsAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.changesets);

    /// <summary>リポジトリのチェンジセット情報を取得する</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<GetChangesetResult> GetChangesetAsync(GetChangesetArgs args, CancellationToken cancelToken = default)
        => this.Client.GetChangesetAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>プルリクエスト情報を取得する</summary>
    /// <remarks>プルリクエスト元リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<PullRequest> GetPullRequestAsync(PullRequestArgs args, CancellationToken cancelToken = default)
        => this.Client.GetPullRequestAsync(args, TakeId(), cancelToken).ConvertResponse(r => r.pullrequest);

    /// <summary>プルリクエストにコメント追加/状態更新する</summary>
    /// <remarks>プルリクエスト元リポジトリの読み取り権限とレビュア権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<CommentPullRequestResult> CommentPullRequestAsync(CommentPullRequestArgs args, CancellationToken cancelToken = default)
        => this.Client.CommentPullRequestAsync(args, TakeId(), cancelToken).UnwrapResponse();

    /// <summary>プルリクエストのレビュアを更新する</summary>
    /// <remarks>プルリクエスト元リポジトリの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>結果取得タスク</returns>
    public Task<EditPullRequestReviewersResult> EditPullRequestReviewersAsync(EditPullRequestReviewersArgs args, CancellationToken cancelToken = default)
        => this.Client.EditPullRequestReviewersAsync(args, TakeId(), cancelToken).UnwrapResponse();
    #endregion

    #region 破棄
    /// <summary>リソース破棄</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    // 保護フィールド
    #region リソース
    /// <summary>APIクライアント</summary>
    protected KallitheaClient Client { get; }
    #endregion

    // 保護メソッド
    #region 破棄
    #region リソース
    /// <summary>要求IDを取得する</summary>
    /// <returns>要求ID</returns>
    protected string TakeId()
    {
        var value = Interlocked.Increment(ref this.id);
        return value.ToString();
    }
    #endregion

    /// <summary>リソース破棄</summary>
    /// <param name="disposing">マネージ破棄過程であるか否か</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            // マネージ破棄過程であればマネージオブジェクトを破棄する
            if (disposing)
            {
                this.Client?.Dispose();
            }

            // 破棄済みマーク
            this.isDisposed = true;
        }
    }
    #endregion

    // 非公開フィールド
    #region 状態フラグ
    /// <summary>要求ID</summary>
    private uint id;

    /// <summary>破棄済みフラグ</summary>
    private bool isDisposed;
    #endregion
}
