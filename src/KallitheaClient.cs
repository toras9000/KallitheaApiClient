#if DEBUG
using System.Diagnostics;
#endif
using System.Net.Http.Json;
using System.Text.Json;

namespace KallitheaApiClient;

/// <summary>
/// kallithea API クライアント
/// </summary>
public class KallitheaClient : IDisposable
{
    // 構築
    #region コンストラクタ
    /// <summary>コンストラクタ</summary>
    /// <param name="apiEntry">APIエントリポイント</param>
    /// <param name="apiKey">APIキー。後からの設定可能。</param>
    /// <param name="clientFactory">HttpClient生成デリゲート。IHttpClientFactory による生成を仲介することを推奨。指定しない場合は新しくインスタンスを生成する。</param>
    /// <exception cref="ArgumentNullException"></exception>
    public KallitheaClient(Uri apiEntry, string? apiKey = null, Func<HttpClient>? clientFactory = null)
    {
        this.ApiEntry = apiEntry ?? throw new ArgumentNullException(nameof(apiEntry));
        this.ApiKey = apiKey;
        this.http = clientFactory?.Invoke() ?? new HttpClient();
    }
    #endregion

    // 公開プロパティ
    #region APIアクセス情報
    /// <summary>APIエントリポイント</summary>
    public Uri ApiEntry { get; }

    /// <summary>APIキー</summary>
    public string? ApiKey { get; set; }
    #endregion

    // 公開イベント
    #region ログ
    /// <summary>API要求ログ</summary>
    public event Action<ApiLog>? Logging;
    #endregion

    // 公開メソッド
    #region API呼び出し
    /// <summary>リモートからのpullをトリガーする。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<PullResult>> PullAsync(PullArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "pull", args).PostAsync<PullResult>(cancelToken);

    /// <summary>リポジトリの再スキャンをトリガーする。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<RescanReposResult>> RescanReposAsync(RescanReposArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "rescan_repos", args).PostAsync<RescanReposResult>(cancelToken);

    /// <summary>リポジトリのキャッシュを無効化する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<InvalidateCacheResult>> InvalidateCacheAsync(RepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "invalidate_cache", args).PostAsync<InvalidateCacheResult>(cancelToken);

    /// <summary>ユーザIPホワイトリストとサーバから見たIPを取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<GetIpResult>> GetIpAsync(UserArgs? args = null, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "get_ip", args).PostAsync<GetIpResult>(cancelToken);

    /// <summary>サーバ情報を取得する。</summary>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<GetServerInfoResult>> GetServerInfoAsync(string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "get_server_info", default(object)).PostAsync<GetServerInfoResult>(cancelToken);

    /// <summary>ユーザ情報を取得する。</summary>
    /// <remarks>管理者権限を持たない場合、自身の情報のみ取得可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetUserResult>> GetUserAsync(UserArgs? args = null, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_user", args).PostAsync<JsonElement>(cancelToken).ConfigureAwait(false);
        var user = rsp.result.Deserialize<UserInfo>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(GetUserResult)}.{nameof(GetUserResult.user)}");
        var perm = rsp.result.GetProperty(nameof(GetUserResult.permissions)).Deserialize<UserPermissions>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(GetUserResult)}.{nameof(GetUserResult.permissions)}");
        return new ApiResponse<GetUserResult>(rsp.id, new(user, perm));
    }

    /// <summary>ユーザの一覧を取得する</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetUsersResult>> GetUsersAsync(string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_users", default(object)).PostAsync<UserInfo[]>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetUsersResult>(rsp.id, new(rsp.result));
    }

    /// <summary>ユーザを作成する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<CreateUserResult>> CreateUserAsync(CreateUserArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "create_user", args).PostAsync<CreateUserResult>(cancelToken);

    /// <summary>ユーザ情報を更新する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<UpdateUserResult>> UpdateUserAsync(UpdateUserArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "update_user", args).PostAsync<UpdateUserResult>(cancelToken);

    /// <summary>ユーザを削除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<DeleteUserResult>> DeleteUserAsync(UserArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "delete_user", args).PostAsync<DeleteUserResult>(cancelToken);

    /// <summary>ユーザグループ情報を取得する。</summary>
    /// <remarks>ユーザグループに対する読み取り以上の権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetUserGroupResult>> GetUserGroupAsync(UserGroupArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_user_group", args).PostAsync<UserGroupInfo>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetUserGroupResult>(rsp.id, new(rsp.result));
    }

    /// <summary>ユーザグループ一覧を取得する。</summary>
    /// <remarks>ユーザグループに対する読み取り以上の権限を持つキーで実行可能。</remarks>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetUserGroupsResult>> GetUserGroupsAsync(string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_user_groups", default(object)).PostAsync<UserGroupInfo[]>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetUserGroupsResult>(rsp.id, new(rsp.result));
    }

    /// <summary>ユーザグループを作成する。</summary>
    /// <remarks>ユーザグループの作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<CreateUserGroupResult>> CreateUserGroupAsync(CreateUserGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "create_user_group", args).PostAsync<CreateUserGroupResult>(cancelToken);

    /// <summary>ユーザグループを更新する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<UpdateUserGroupResult>> UpdateUserGroupAsync(UpdateUserGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "update_user_group", args).PostAsync<UpdateUserGroupResult>(cancelToken);

    /// <summary>ユーザグループを削除する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<UpdateUserGroupResult>> DeleteUserGroupAsync(UserGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "delete_user_group", args).PostAsync<UpdateUserGroupResult>(cancelToken);

    /// <summary>ユーザグループにユーザを追加する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<AddUserToUserGroupResult>> AddUserToUserGroupAsync(UserWithUserGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "add_user_to_user_group", args).PostAsync<AddUserToUserGroupResult>(cancelToken);

    /// <summary>ユーザグループからユーザを削除する。</summary>
    /// <remarks>ユーザグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<RemoveUserToUserGroupResult>> RemoveUserFromUserGroupAsync(UserWithUserGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "remove_user_from_user_group", args).PostAsync<RemoveUserToUserGroupResult>(cancelToken);

    /// <summary>リポジトリの情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetRepoResult>> GetRepoAsync(GetRepoArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_repo", args).PostAsync<JsonElement>(cancelToken).ConfigureAwait(false);
        var repo = rsp.result.Deserialize<RepoInfoEx>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(GetRepoResult)}.{nameof(GetRepoResult.repo)}");
        var members = rsp.result.GetProperty(nameof(GetRepoResult.members)).Deserialize<Member[]>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(GetRepoResult)}.{nameof(GetRepoResult.members)}");
        var followers = rsp.result.GetProperty(nameof(GetRepoResult.followers)).Deserialize<UserInfo[]>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(GetRepoResult)}.{nameof(GetRepoResult.followers)}");
        var revs = rsp.result.Deserialize<NamedRevs>();
        var pull_req = rsp.result.TryGetProperty(nameof(GetRepoResult.pull_requests), out var prop_preq) ? prop_preq.Deserialize<PullRequest[]>() : null;
        return new ApiResponse<GetRepoResult>(rsp.id, new(repo, members, followers, revs, pull_req));
    }

    /// <summary>リポジトリの一覧を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetReposResult>> GetReposAsync(string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_repos", default(object)).PostAsync<RepoInfoEx[]>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetReposResult>(rsp.id, new(rsp.result));
    }

    /// <summary>リポジトリのリビジョンに含まれるノード情報を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetRepoNodesResult>> GetRepoNodesAsync(GetRepoNodesArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_repo_nodes", args).PostAsync<RepoNode[]>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetRepoNodesResult>(rsp.id, new(rsp.result));
    }

    /// <summary>リポジトリを作成する。</summary>
    /// <remarks>リポジトリの作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<CreateRepoResult>> CreateRepoAsync(CreateRepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "create_repo", args).PostAsync<CreateRepoResult>(cancelToken);

    /// <summary>リポジトリを更新する。</summary>
    /// <remarks>リポジトリの書き込み権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<UpdateRepoResult>> UpdateRepoAsync(UpdateRepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "update_repo", args).PostAsync<UpdateRepoResult>(cancelToken);

    /// <summary>リポジトリをフォークする。</summary>
    /// <remarks>フォーク元リポジトリの読み取り権限とリポジトリ作成権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<ForkRepoResult>> ForkRepoAsync(ForkRepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "fork_repo", args).PostAsync<ForkRepoResult>(cancelToken);

    /// <summary>リポジトリを削除する。</summary>
    /// <remarks>リポジトリの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<DeleteRepoResult>> DeleteRepoAsync(DeleteRepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "delete_repo", args).PostAsync<DeleteRepoResult>(cancelToken);

    /// <summary>リポジトリにユーザの権限を設定(追加または更新)する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<GrantUserPermToRepoResult>> GrantUserPermToRepoAsync(GrantUserPermToRepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "grant_user_permission", args).PostAsync<GrantUserPermToRepoResult>(cancelToken);

    /// <summary>リポジトリからユーザの権限を解除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<RevokeUserPermFromRepoResult>> RevokeUserPermFromRepoAsync(RevokeUserPermFromRepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "revoke_user_permission", args).PostAsync<RevokeUserPermFromRepoResult>(cancelToken);

    /// <summary>リポジトリにユーザグループの権限を設定(追加または更新)する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<GrantUserGroupPermToRepoResult>> GrantUserGroupPermToRepoAsync(GrantUserGroupPermToRepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "grant_user_group_permission", args).PostAsync<GrantUserGroupPermToRepoResult>(cancelToken);

    /// <summary>リポジトリからユーザグループの権限を解除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<RevokeUserGroupPermFromRepoResult>> RevokeUserGroupPermFromRepoAsync(RevokeUserGroupPermFromRepoArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "revoke_user_group_permission", args).PostAsync<RevokeUserGroupPermFromRepoResult>(cancelToken);

    /// <summary>リポジトリグループの情報を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetRepoGroupResult>> GetRepoGroupAsync(RepoGroupArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_repo_group", args).PostAsync<JsonElement>(cancelToken).ConfigureAwait(false);
        var repogroup = rsp.result.Deserialize<RepoGroupInfo>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(GetRepoGroupResult)}.{nameof(GetRepoGroupResult.repogroup)}");
        var members = rsp.result.GetProperty(nameof(GetRepoGroupResult.members)).Deserialize<Member[]>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(GetRepoGroupResult)}.{nameof(GetRepoGroupResult.members)}");
        return new ApiResponse<GetRepoGroupResult>(rsp.id, new(repogroup, members));
    }

    /// <summary>リポジトリグループの一覧を取得する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetRepoGroupsResult>> GetRepoGroupsAsync(string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_repo_groups", default(object)).PostAsync<RepoGroupInfo[]>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetRepoGroupsResult>(rsp.id, new(rsp.result));
    }

    /// <summary>リポジトリグループを作成する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<CreateRepoGroupResult>> CreateRepoGroupAsync(CreateRepoGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "create_repo_group", args).PostAsync<CreateRepoGroupResult>(cancelToken);

    /// <summary>リポジトリグループを更新する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<UpdateRepoGroupResult>> UpdateRepoGroupAsync(UpdateRepoGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "update_repo_group", args).PostAsync<UpdateRepoGroupResult>(cancelToken);

    /// <summary>リポジトリグループを削除する。</summary>
    /// <remarks>管理者ユーザのキーでのみ実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<DeleteRepoGroupResult>> DeleteRepoGroupAsync(RepoGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "delete_repo_group", args).PostAsync<DeleteRepoGroupResult>(cancelToken);

    /// <summary>リポジトリグループにユーザの権限を設定(追加または更新)する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<GrantUserPermToRepoGroupResult>> GrantUserPermToRepoGroupAsync(GrantUserPermToRepoGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "grant_user_permission_to_repo_group", args).PostAsync<GrantUserPermToRepoGroupResult>(cancelToken);

    /// <summary>リポジトリグループからユーザの権限を解除する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<RevokeUserPermFromRepoGroupResult>> RevokeUserPermFromRepoGroupAsync(RevokeUserPermFromRepoGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "revoke_user_permission_from_repo_group", args).PostAsync<RevokeUserPermFromRepoGroupResult>(cancelToken);

    /// <summary>リポジトリグループにユーザグループの権限を設定(追加または更新)する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<GrantUserGroupPermToRepoGroupResult>> GrantUserGroupPermToRepoGroupAsync(GrantUserGroupPermToRepoGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "grant_user_group_permission_to_repo_group", args).PostAsync<GrantUserGroupPermToRepoGroupResult>(cancelToken);

    /// <summary>リポジトリグループからユーザグループの権限を解除する。</summary>
    /// <remarks>リポジトリグループの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<RevokeUserGroupPermFromRepoGroupResult>> RevokeUserGroupPermFromRepoGroupAsync(RevokeUserGroupPermFromToRepoGroupArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "revoke_user_group_permission_from_repo_group", args).PostAsync<RevokeUserGroupPermFromRepoGroupResult>(cancelToken);

    /// <summary>Gist情報を取得する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetGistResult>> GetGistAsync(GistArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_gist", args).PostAsync<GistInfo>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetGistResult>(rsp.id, new(rsp.result));
    }

    /// <summary>Gist一覧を取得する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetGistsResult>> GetGistsAsync(UserArgs? args = null, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_gists", args).PostAsync<GistInfo[]>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetGistsResult>(rsp.id, new(rsp.result));
    }

    /// <summary>Gistを作成する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<CreateGistResult>> CreateGistAsync(CreateGistArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "create_gist", args).PostAsync<CreateGistResult>(cancelToken);

    /// <summary>Gistを削除する。</summary>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<DeleteGistResult>> DeleteGistAsync(GistArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "delete_gist", args).PostAsync<DeleteGistResult>(cancelToken);

    /// <summary>リポジトリのチェンジセット一覧を取得する。</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetChangesetsResult>> GetChangesetsAsync(GetChangesetsArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_changesets", args).PostAsync<JsonElement[]>(cancelToken).ConfigureAwait(false);
        var changesets = rsp.result
            .Select(e =>
            {
                var summary = e.Deserialize<ChangesetSummary>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(Changeset)}");
                var filelist = default(ChangesetFileList);
                if (args.with_file_list == true)
                {
                    filelist = e.Deserialize<ChangesetFileList>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(ChangesetFileList)}");
                }
                return new Changeset(summary, filelist);
            })
            .ToArray();
        return new ApiResponse<GetChangesetsResult>(rsp.id, new(changesets));
    }

    /// <summary>リポジトリのチェンジセット情報を取得する</summary>
    /// <remarks>リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetChangesetResult>> GetChangesetAsync(GetChangesetArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_changeset", args).PostAsync<JsonElement>(cancelToken).ConfigureAwait(false);
        var summary = rsp.result.Deserialize<ChangesetSummary2>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(ChangesetSummary2)}");
        var filelist = rsp.result.Deserialize<ChangesetFileList>() ?? throw new UnexpectedResultException(rsp.id, $"{nameof(ChangesetFileList)}");
        var reviews = rsp.result.TryGetProperty(nameof(GetChangesetResult.reviews), out var reviews_prop) ? reviews_prop.Deserialize<Status[]>() : default;
        return new ApiResponse<GetChangesetResult>(rsp.id, new(summary, filelist, reviews));
    }

    /// <summary>プルリクエスト情報を取得する</summary>
    /// <remarks>プルリクエスト元リポジトリの読み取り権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<GetPullRequestResult>> GetPullRequestAsync(PullRequestArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "get_pullrequest", args).PostAsync<PullRequest>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<GetPullRequestResult>(rsp.id, new(rsp.result));
    }

    /// <summary>プルリクエストにコメント追加/状態更新する</summary>
    /// <remarks>プルリクエスト元リポジトリの読み取り権限とレビュア権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public async Task<ApiResponse<CommentPullRequestResult>> CommentPullRequestAsync(CommentPullRequestArgs args, string? id = null, CancellationToken cancelToken = default)
    {
        var rsp = await CreateContext(id, "comment_pullrequest", args).PostAsync<bool>(cancelToken).ConfigureAwait(false);
        return new ApiResponse<CommentPullRequestResult>(rsp.id, new(rsp.result));
    }

    /// <summary>プルリクエストのレビュアを更新する</summary>
    /// <remarks>プルリクエスト元リポジトリの管理権限を持つキーで実行可能。</remarks>
    /// <param name="args">要求パラメータ</param>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="cancelToken">キャンセルトークン</param>
    /// <returns>レスポンス取得タスク</returns>
    public Task<ApiResponse<EditPullRequestReviewersResult>> EditPullRequestReviewersAsync(EditPullRequestReviewersArgs args, string? id = null, CancellationToken cancelToken = default)
        => CreateContext(id, "edit_reviewers", args).PostAsync<EditPullRequestReviewersResult>(cancelToken);
    #endregion

    #region 破棄
    /// <summary>リソース破棄</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    // 非公開型
    #region APIアクセス
    /// <summary>API要求コンテキスト インタフェース</summary>
    /// <typeparam name="TArgs">要求パラメータ型</typeparam>
    protected interface IRequestContext<TArgs>
    {
        /// <summary>要求を送信する</summary>
        /// <typeparam name="TResult">応答結果データ型</typeparam>
        /// <param name="cancelToken">キャンセルトークン</param>
        /// <returns>API応答データ</returns>
        Task<ApiResponse<TResult>> PostAsync<TResult>(CancellationToken cancelToken);
    }
    #endregion

    // 保護メソッド
    #region APIアクセス
    /// <summary>要求コンテキストを生成する</summary>
    /// <typeparam name="TArgs">要求パラメータ型</typeparam>
    /// <param name="id">任意の要求識別子</param>
    /// <param name="method">要求メソッド名</param>
    /// <param name="args">要求パラメータ</param>
    /// <returns>API要求コンテキスト</returns>
    protected IRequestContext<TArgs> CreateContext<TArgs>(string? id, string method, TArgs? args) where TArgs : class
        => new RequestContext<TArgs>(this, id, method, args);
    #endregion

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
                this.http?.Dispose();
            }

            // 破棄済みマーク
            this.isDisposed = true;
        }
    }
    #endregion

    // 非公開型
    #region APIアクセス用の型
    /// <summary>API要求コンテキスト</summary>
    /// <remarks>
    /// TArgs と TResult がAPI毎に異なるが、なるべく推論を利用したかったためこのラップ型を作成した。
    /// TResult は必ず明示指定が必要なものであるため、両者を1つのメソッドで扱おうとすると TArgs に推論を利用できなくなるので、
    /// TArgs と TResult の出現個所を分ける必要があった。
    /// </remarks>
    /// <typeparam name="TArgs">要求パラメータ型</typeparam>
    /// <param name="Client"></param>
    /// <param name="Id">任意の要求識別子</param>
    /// <param name="Method">要求メソッド名</param>
    /// <param name="Args">要求パラメータ</param>
    private record RequestContext<TArgs>(KallitheaClient Client, string? Id, string Method, TArgs? Args) : IRequestContext<TArgs> where TArgs : class
    {
        /// <inheritdoc />
        public async Task<ApiResponse<TResult>> PostAsync<TResult>(CancellationToken cancelToken)
        {
            if (string.IsNullOrEmpty(this.Client.ApiKey)) throw new InvalidOperationException("api_key is not set.");

            // 要求パラメータ
            var indentify = this.Id ?? "";
            var input = new ApiRequest(indentify, this.Client.ApiKey, this.Method, this.Args);

            // API要求
            using var response = await this.Client.http.PostAsJsonAsync(this.Client.ApiEntry, input, cancelToken).ConfigureAwait(false);

            // ログイベントのリスナーがいればAPI要求のログを通知
            var listener = this.Client.Logging;
            if (listener != null)
            {
                var reqData = JsonSerializer.Serialize(input);
                var status = response.IsSuccessStatusCode;
                var rspData = status ? (await response.Content.ReadAsStringAsync().ConfigureAwait(false)) : "";
                listener(new(reqData, status, rspData));
            }

            // 要求が成功レスポンスを示すかを確認
            if (!response.IsSuccessStatusCode) throw new UnexpectedResponseException(indentify, response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}");

            // 応答をデコード
            var output = await response.Content.ReadFromJsonAsync<RawApiResponse<TResult>>(options: null, cancelToken).ConfigureAwait(false)
                ?? throw new UnexpectedResultException(indentify, typeof(TResult).Name);

            // エラーメッセージがある場合はエラーとして扱う
            if (!string.IsNullOrWhiteSpace(output.error)) throw new ErrorResponseException(indentify, output.error);

            // 返却用に型を変換
            return new ApiResponse<TResult>(output.id, output.result);
        }

        /// <summary>API要求データ型</summary>
        /// <param name="id">任意の要求識別子</param>
        /// <param name="api_key">APIキー</param>
        /// <param name="method">要求メソッド名</param>
        /// <param name="args">要求パラメータ</param>
        private record ApiRequest(string id, string api_key, string method, TArgs? args);

        /// <summary>API応答データ型</summary>
        /// <typeparam name="TResult">応答結果データ型</typeparam>
        /// <param name="id">要求識別子</param>
        /// <param name="result">応答結果データ</param>
        /// <param name="error">エラーメッセージ</param>
        private record RawApiResponse<TResult>(string id, TResult result, string error);
    }
    #endregion

    #region 内部型
    /// <summary>get_user 要求の応答JSONスキーマ</summary>
    private record InternalGetUserResult(long user_id, string username, string firstname, string lastname, string email, string[] emails, bool active, bool admin, UserPermissions permissions);
    #endregion

    // 非公開フィールド
    #region リソース
    /// <summary>HTTPクライアント</summary>
    private readonly HttpClient http;
    #endregion

    #region 状態フラグ
    /// <summary>破棄済みフラグ</summary>
    private bool isDisposed;
    #endregion
}
