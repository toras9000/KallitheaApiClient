using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using KallitheaApiClient.Converters;

namespace KallitheaApiClient;

/// <summary>リポジトリ種別</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RepoType
{
    /// <summary>mercurial</summary>
    hg,
    /// <summary>git</summary>
    git,
}

/// <summary>権限メンバー種別</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberType
{
    /// <summary>ユーザ</summary>
    user,
    /// <summary>ユーザグループ</summary>
    user_group,
}

/// <summary>リポジトリ権限</summary>
[JsonConverter(typeof(RepoPermJsonConverter))]
public enum RepoPerm
{
    /// <summary>なし</summary>
    none,
    /// <summary>読取権限</summary>
    read,
    /// <summary>書込権限</summary>
    write,
    /// <summary>管理権限</summary>
    admin,
}

/// <summary>リポジトリグループ権限</summary>
[JsonConverter(typeof(RepoGroupPermJsonConverter))]
public enum RepoGroupPerm
{
    /// <summary>なし</summary>
    none,
    /// <summary>読取権限</summary>
    read,
    /// <summary>書込権限</summary>
    write,
    /// <summary>管理権限</summary>
    admin,
}

/// <summary>ユーザグループ権限</summary>
[JsonConverter(typeof(UserGroupPermJsonConverter))]
public enum UserGroupPerm
{
    /// <summary>なし</summary>
    none,
    /// <summary>読取権限</summary>
    read,
    /// <summary>書込権限</summary>
    write,
    /// <summary>管理権限</summary>
    admin,
}

/// <summary>取得ノード種別</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NodesType
{
    /// <summary>すべて</summary>
    all,
    /// <summary>ファイル</summary>
    files,
    /// <summary>ディレクトリ</summary>
    dirs,
}

/// <summary>子要素へのパーミッション適用方法</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PermRecurse
{
    /// <summary>なし</summary>
    none,
    /// <summary>リポジトリ</summary>
    repos,
    /// <summary>リポジトリグループ</summary>
    groups,
    /// <summary>すべて</summary>
    all,
}

/// <summary>Gist の公開種別</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GistType
{
    /// <summary>公開</summary>
    @public,
    /// <summary>非公開</summary>
    @private,
}

/// <summary>プルリクエストの状態</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PullRequestStatus
{
    /// <summary>未レビュー</summary>
    not_reviewed,
    /// <summary>レビュー中</summary>
    under_review,
    /// <summary>却下</summary>
    rejected,
    /// <summary>承認</summary>
    approved,
}

/// <summary>アタッチされた(フォークされた)リポジトリの扱い</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ForksTreatment
{
    /// <summary>デタッチ</summary>
    detach,
    /// <summary>削除</summary>
    delete,
}

/// <summary>プロパティ名と値</summary>
/// <typeparam name="TValue">値の型</typeparam>
/// <param name="name">プロパティ名</param>
/// <param name="value">プロパティ値</param>
public record struct PropertyValue<TValue>(string name, TValue value);

/// <summary>プロパティ名と値のコレクション</summary>
/// <typeparam name="TValue">プロパティ値の型</typeparam>
[JsonConverter(typeof(PropertySetJsonConverterFactory))]
public class PropertySet<TValue> : Collection<PropertyValue<TValue>> { }


/// <summary>モジュール情報</summary>
/// <param name="name">モジュール名</param>
/// <param name="version">モジュールバージョン</param>
[JsonConverter(typeof(ModuleInfoJsonConverter))]
public record ModuleInfo(string name, string version);

/// <summary>IPアドレス範囲情報</summary>
/// <param name="start_ip">開始アドレス</param>
/// <param name="end_ip">終了アドレス</param>
[JsonConverter(typeof(IpRangeJsonConverter))]
public record IpRange(string start_ip, string end_ip);

/// <summary>IPアドレス情報</summary>
/// <param name="ip_addr">IPアドレス</param>
/// <param name="ip_range">IPアドレス範囲</param>
public record IpAddrInfo(string ip_addr, IpRange ip_range);

/// <summary>ユーザの権限情報</summary>
/// <param name="global">グローバル権限</param>
/// <param name="repositories">リポジトリ権限</param>
/// <param name="repositories_groups">リポジトリグループ権限</param>
/// <param name="user_groups">ユーザグループ権限</param>
public record UserPermissions(string[] global, PropertySet<RepoPerm> repositories, PropertySet<RepoGroupPerm> repositories_groups, PropertySet<UserGroupPerm> user_groups);

/// <summary>ユーザ情報</summary>
/// <param name="user_id">ユーザID</param>
/// <param name="username">ユーザ名</param>
/// <param name="firstname">ファーストネーム</param>
/// <param name="lastname">ラストネーム</param>
/// <param name="email">メールアドレス</param>
/// <param name="emails">追加のメールアドレス</param>
/// <param name="active">ユーザがアクティブであるか否か</param>
/// <param name="admin">ユーザが管理者であるか否か</param>
public record UserInfo(long user_id, string username, string firstname, string lastname, string email, string[] emails, bool active, bool admin);

/// <summary>ユーザグループ情報</summary>
/// <param name="users_group_id">ユーザグループID</param>
/// <param name="group_name">ユーザグループ名</param>
/// <param name="group_description">ユーザグループ説明</param>
/// <param name="active">ユーザグループがアクティブであるか否か</param>
/// <param name="owner">ユーザグループの所有ユーザー名</param>
/// <param name="members">グループメンバー</param>
public record UserGroupInfo(long users_group_id, string group_name, string group_description, bool active, string owner, UserInfo[] members);

/// <summary>チェンジセット要約情報</summary>
/// <param name="short_id">短いコミット識別子</param>
/// <param name="raw_id">コミット識別子</param>
/// <param name="revision">リビジョン番号</param>
/// <param name="message">コミットメッセージ</param>
/// <param name="date">コミット日時</param>
/// <param name="author">コミット作者</param>
public record ChangesetSummary(string short_id, string raw_id, long revision, string message, string date, string author);

/// <summary>チェンジセット要約情報 (別版)</summary>
/// <param name="short_id">短いコミット識別子</param>
/// <param name="raw_id">コミット識別子</param>
/// <param name="revision">リビジョン番号</param>
/// <param name="message">コミットメッセージ</param>
/// <param name="date">コミット日時</param>
/// <param name="author">コミット作者情報</param>
public record ChangesetSummary2(string short_id, string raw_id, long revision, string message, string date, Author author);

/// <summary>チェンジセットの変更ファイルリスト</summary>
/// <param name="added">追加されたファイルリスト</param>
/// <param name="changed">変更されたファイルリスト</param>
/// <param name="removed">削除されたファイルリスト</param>
public record ChangesetFileList(string[] added, string[] changed, string[] removed);

/// <summary>チェンジセット情報</summary>
/// <param name="summary">チェンジセット要約情報</param>
/// <param name="filelist">チェンジセット変更ファイルリスト</param>
public record Changeset(ChangesetSummary summary, ChangesetFileList? filelist);

/// <summary>リポジトリ権限設定情報</summary>
/// <param name="name">メンバー名称</param>
/// <param name="type">メンバー種別</param>
/// <param name="permission">権限</param>
public record Member(string name, MemberType type, string permission);

/// <summary>レビュアー情報</summary>
/// <param name="username">ユーザ名</param>
public record Reviewer(string username);

/// <summary>コメント情報</summary>
/// <param name="comment_id">コメントID</param>
/// <param name="username">ユーザ名</param>
/// <param name="text">コメント</param>
/// <param name="created_on">作成日時</param>
public record Comment(long comment_id, string username, string text, DateTime created_on);

/// <summary>レビューステータス情報</summary>
/// <param name="status">ステータス</param>
/// <param name="modified_at">更新日時</param>
/// <param name="reviewer">レビュア名</param>
public record Status(string status, string modified_at, string reviewer);

/// <summary>プルリクエスト情報</summary>
/// <param name="pull_request_id">プルリクエストID</param>
/// <param name="url">プルリクエストのURL</param>
/// <param name="reviewers">レビュアー</param>
/// <param name="revisions">プルリクエスト内容のリビジョン</param>
/// <param name="owner">プルリクエストの所有者(発行者)</param>
/// <param name="title">タイトル</param>
/// <param name="description">説明</param>
/// <param name="org_repo_url">プルリクエスト元リポジトリURL</param>
/// <param name="org_ref_parts">プルリクエスト元チェンジセット情報</param>
/// <param name="other_ref_parts">プルリクエスト対象のベースチェンジセット？</param>
/// <param name="status">プルリクエストのステータス</param>
/// <param name="comments">コメント</param>
/// <param name="statuses">レビューステータス</param>
/// <param name="created_on">作成日時</param>
/// <param name="updated_on">更新日時</param>
public record PullRequest(
    long pull_request_id, string url, Reviewer[] reviewers, string[] revisions,
    string owner, string title, string description, string org_repo_url, string[] org_ref_parts, string[] other_ref_parts,
    string status, Comment[] comments, Status[] statuses, string created_on, string updated_on
);

/// <summary>リポジトリ情報</summary>
/// <param name="repo_id">リポジトリID</param>
/// <param name="repo_name">リポジトリパス</param>
/// <param name="repo_type">リポジトリ種別</param>
/// <param name="clone_uri">クローン元URL</param>
/// <param name="private">非公開リポジトリであるか否か</param>
/// <param name="created_on">作成日時</param>
/// <param name="description">説明</param>
/// <param name="landing_rev">ランディングリビジョン</param>
/// <param name="owner">所有ユーザ</param>
/// <param name="fork_of">フォーク元リポジトリパス</param>
/// <param name="enable_downloads">ダウンロードが有効であるか否か</param>
/// <param name="enable_statistics">統計が有効であるか否か</param>
/// <param name="last_changeset">最終チェンジセット</param>
public record RepoInfo(
    long repo_id, string repo_name, string repo_type, string clone_uri, bool @private,
    string created_on, string description, string[] landing_rev, string owner, string fork_of,
    bool enable_downloads, bool enable_statistics, ChangesetSummary last_changeset
);

/// <summary>リポジトリ拡張フィールド情報</summary>
/// <param name="key">拡張フィールドキー</param>
/// <param name="value">拡張フィールド値</param>
public record ExtraField(string key, string value);

/// <summary>リポジトリ拡張情報</summary>
/// <param name="repo">リポジトリ情報</param>
/// <param name="extra_fields">拡張フィールド(拡張フィールドが設定されている時)</param>
[JsonConverter(typeof(RepoInfoExJsonConverter))]
public record RepoInfoEx(RepoInfo repo, ExtraField[]? extra_fields) : RepoInfo(repo)
{
    /// <summary>基底メンバの初期化用に使う</summary>
    protected RepoInfo repo { get; } = repo;
}

/// <summary>名前付きリビジョン情報</summary>
/// <param name="tags">タグ一覧</param>
/// <param name="branches">ブランチ一覧</param>
/// <param name="bookmarks">ブックマーク一覧</param>
public record NamedRevs(PropertySet<string> tags, PropertySet<string> branches, PropertySet<string> bookmarks);

/// <summary>チェンジセット作者情報</summary>
/// <param name="name">名前</param>
/// <param name="email">メールアドレス</param>
public record Author(string name, string email);

/// <summary>リポジトリ内ノード情報</summary>
/// <param name="name">名前</param>
/// <param name="type">種別</param>
public record RepoNode(string name, string type);

/// <summary>リポジトリグループ情報</summary>
/// <param name="group_id">リポジトリグループID</param>
/// <param name="group_name">リポジトリグループパス</param>
/// <param name="group_description">説明</param>
/// <param name="parent_group">親グループ</param>
/// <param name="repositories">含んでいるリポジトリ名(パス)</param>
/// <param name="owner">所有ユーザ</param>
public record RepoGroupInfo(long group_id, string group_name, string group_description, string parent_group, string[] repositories, string owner);

/// <summary>Gist情報</summary>
/// <param name="gist_id">Gist ID</param>
/// <param name="type">公開種別</param>
/// <param name="access_id">アクセスID</param>
/// <param name="description">説明</param>
/// <param name="url">GistのURL</param>
/// <param name="expires">有効期間(UnixTime)</param>
/// <param name="created_on">作成日時</param>
public record GistInfo(long gist_id, string type, string access_id, string description, string url, double expires, string created_on);

/// <summary>Gistコンテンツ</summary>
/// <param name="content">内容</param>
/// <param name="lexer">書式</param>
public record GistContent(string content, string lexer);

/// <summary>API呼び出しのログ</summary>
/// <param name="Request">要求データ</param>
/// <param name="Status">APIへの応答を得たかどうか</param>
/// <param name="Response">応答データ</param>
public record ApiLog(string Request, bool Status, string Response);


/// <summary>リポジトリを指定する要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
public record RepoArgs(string repoid);

/// <summary>リポジトリグループを指定する要求パラメータ</summary>
/// <param name="repogroupid">リポジトリグループIDまたはリポジトリグループパス</param>
public record RepoGroupArgs(string repogroupid);

/// <summary>ユーザを指定する要求パラメータ</summary>
/// <param name="userid">ユーザIDまたはユーザ名</param>
public record UserArgs(string userid);

/// <summary>ユーザグループを指定する要求パラメータ</summary>
/// <param name="usergroupid">ユーザグループID/ユーザグループ名</param>
public record UserGroupArgs(string usergroupid);

/// <summary>ユーザグループとユーザの組み合わせを示す要求パラメータ</summary>
/// <param name="usergroupid">ユーザグループIDまたはユーザグループ名</param>
/// <param name="userid">ユーザIDまたはユーザ名</param>
public record UserWithUserGroupArgs(string usergroupid, string userid);

/// <summary>リポジトリの再スキャン要求パラメータ</summary>
/// <param name="remove_obsolete">見つからないリポジトリをデータベースから削除するか否か</param>
public record RescanReposArgs(bool? remove_obsolete);

/// <summary>リポジトリを指定する要求パラメータ</summary>
/// <param name="repoid">リポジトリID</param>
/// <param name="clone_uri">プル元のリポジトリURI</param>
public record PullArgs(string repoid, string? clone_uri = null);

/// <summary>ユーザ作成要求パラメータ</summary>
/// <param name="username">ユーザ名</param>
/// <param name="email">メールアドレス</param>
/// <param name="firstname">ファーストネーム</param>
/// <param name="lastname">ラストネーム</param>
/// <param name="password">パスワード</param>
/// <param name="active">ユーザがアクティブであるか否か</param>
/// <param name="admin">ユーザが管理者であるか否か</param>
/// <param name="extern_type">認証メソッド名</param>
/// <param name="extern_name">外部認証名</param>
public record CreateUserArgs(string username, string email, string firstname, string lastname, string? password = null, bool? active = null, bool? admin = null, string? extern_type = "internal", string? extern_name = null);

/// <summary>ユーザ情報更新要求パラメータ</summary>
/// <param name="userid">ユーザID</param>
/// <param name="username">ユーザ名</param>
/// <param name="email">メールアドレス</param>
/// <param name="firstname">ファーストネーム</param>
/// <param name="lastname">ラストネーム</param>
/// <param name="password">パスワード</param>
/// <param name="active">ユーザがアクティブであるか否か</param>
/// <param name="admin">ユーザが管理者であるか否か</param>
/// <param name="extern_type">認証メソッド名</param>
/// <param name="extern_name">外部認証名</param>
public record UpdateUserArgs(string userid, string? username = null, string? email = null, string? firstname = null, string? lastname = null, string? password = null, bool? active = null, bool? admin = null, string? extern_type = null, string? extern_name = null);

/// <summary>ユーザグループの作成要求パラメータ</summary>
/// <param name="group_name">ユーザグループ名</param>
/// <param name="description">ユーザグループ説明</param>
/// <param name="owner">オーナーユーザ</param>
/// <param name="active">ユーザグループがアクティブであるか否か</param>
public record CreateUserGroupArgs(string group_name, string? description = null, string? owner = null, bool active = true);

/// <summary>ユーザグループの更新要求パラメータ</summary>
/// <param name="usergroupid">ユーザグループIDまたはユーザグループ名</param>
/// <param name="group_name">ユーザグループ名</param>
/// <param name="description">ユーザグループ説明</param>
/// <param name="owner">オーナーユーザ</param>
/// <param name="active">ユーザグループがアクティブであるか否か</param>
public record UpdateUserGroupArgs(string usergroupid, string? group_name = null, string? description = null, string? owner = null, bool? active = null);

/// <summary>リポジトリ情報取得要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="with_revision_names">名前付きリビジョン情報を取得するか否か</param>
/// <param name="with_pullrequests">プルリクエスト情報を取得するか否か</param>
public record GetRepoArgs(string repoid, bool? with_revision_names = null, bool? with_pullrequests = null);

/// <summary>リポジトリリビジョン内のノード一覧取得要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="revision">対象リビジョン</param>
/// <param name="root_path">対象の基準パス</param>
/// <param name="ret_type">取得ノード種別</param>
public record GetRepoNodesArgs(string repoid, string revision, string root_path, NodesType? ret_type = null);

/// <summary>リポジトリ作成要求パラメータ</summary>
/// <param name="repo_name">リポジトリパス</param>
/// <param name="owner">所有ユーザIDまたはユーザ名(管理者ユーザのみ指定可能)</param>
/// <param name="repo_type">リポジトリ種別</param>
/// <param name="description">説明</param>
/// <param name="private">非公開リポジトリであるか否か</param>
/// <param name="clone_uri">クローンURL</param>
/// <param name="landing_rev">ランディングリビジョン(rev_type:rev)</param>
/// <param name="copy_permissions">親グループからのパーミッションコピーをするか否か</param>
public record CreateRepoArgs(
    string repo_name, string? owner = null, RepoType? repo_type = null, string? description = null,
    bool? @private = null, string? clone_uri = null, string landing_rev = "rev:tip", bool? copy_permissions = null
);

/// <summary>リポジトリ更新要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="name">新しいリポジトリ名称</param>
/// <param name="owner">所有ユーザIDまたはユーザ名</param>
/// <param name="description">説明</param>
/// <param name="private">非公開リポジトリであるか否か</param>
/// <param name="clone_uri">クローンURL</param>
/// <param name="landing_rev">ランディングリビジョン(rev_type:rev)</param>
/// <param name="enable_downloads">ダウンロードが有効であるか否か</param>
/// <param name="enable_statistics">統計が有効であるか否か</param>
public record UpdateRepoArgs(
    string repoid, string? name = null, string? owner = null, string? description = null,
    bool? @private = null, string? clone_uri = null, string? landing_rev = null,
    bool? enable_downloads = null, bool? enable_statistics = null
);

/// <summary>リポジトリフォーク要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="fork_name">フォーク先リポジトリ名</param>
/// <param name="owner">所有ユーザIDまたはユーザ名(管理者ユーザのみ指定可能)</param>
/// <param name="description">説明</param>
/// <param name="private">非公開リポジトリであるか否か</param>
/// <param name="landing_rev">ランディングリビジョン(rev_type:rev)</param>
/// <param name="copy_permissions">パーミッションコピーをするか否か</param>
public record ForkRepoArgs(
    string repoid, string fork_name, string? owner = null, string? description = null,
    bool @private = false, string landing_rev = "rev:tip", bool? copy_permissions = null
);

/// <summary>リポジトリ削除要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="forks">アタッチされたフォークリポジトリの扱い</param>
public record DeleteRepoArgs(string repoid, ForksTreatment? forks = null);

/// <summary>リポジトリへのユーザ権限設定要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="userid">ユーザIDまたはユーザ名</param>
/// <param name="perm">ユーザのリポジトリ権限</param>
public record GrantUserPermToRepoArgs(string repoid, string userid, RepoPerm perm);

/// <summary>リポジトリのユーザ権限解除要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="userid">ユーザIDまたはユーザ名</param>
public record RevokeUserPermFromRepoArgs(string repoid, string userid);

/// <summary>リポジトリへのユーザグループ権限設定要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="usergroupid">ユーザグループIDまたはユーザグループ名</param>
/// <param name="perm">ユーザのリポジトリ権限</param>
public record GrantUserGroupPermToRepoArgs(string repoid, string usergroupid, RepoPerm perm);

/// <summary>リポジトリのユーザグループ権限解除要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="usergroupid">ユーザグループIDまたはユーザグループ名</param>
public record RevokeUserGroupPermFromRepoArgs(string repoid, string usergroupid);

/// <summary>リポジトリグループの作成要求パラメータ</summary>
/// <param name="group_name">リポジトリグループ名</param>
/// <param name="description">説明</param>
/// <param name="owner">所有ユーザ</param>
/// <param name="parent">親グループ</param>
/// <param name="copy_permissions">親グループからパーミッションコピーをするか否か</param>
public record CreateRepoGroupArgs(string group_name, string? description = null, string? owner = null, string? parent = null, bool? copy_permissions = null);

/// <summary>リポジトリグループの更新要求パラメータ</summary>
/// <param name="repogroupid">リポジトリグループIDまたはリポジトリグループパス</param>
/// <param name="group_name">新しいリポジトリグループ名称</param>
/// <param name="description">説明</param>
public record UpdateRepoGroupArgs(string repogroupid, string? group_name = null, string? description = null);

/// <summary>リポジトリグループへのユーザ権限設定要求パラメータ</summary>
/// <param name="repogroupid">ユーザグループIDまたはユーザグループ名</param>
/// <param name="userid">ユーザIDまたはユーザ名</param>
/// <param name="perm">ユーザのリポジトリグループ権限</param>
/// <param name="apply_to_children">子要素に適用するかどうか</param>
public record GrantUserPermToRepoGroupArgs(string repogroupid, string userid, RepoGroupPerm perm, PermRecurse? apply_to_children = null);

/// <summary>リポジトリグループのユーザ権限解除要求パラメータ</summary>
/// <param name="repogroupid">ユーザグループIDまたはユーザグループ名</param>
/// <param name="userid">ユーザIDまたはユーザ名</param>
/// <param name="apply_to_children">子要素に適用するかどうか</param>
public record RevokeUserPermFromRepoGroupArgs(string repogroupid, string userid, PermRecurse? apply_to_children = null);

/// <summary>リポジトリグループへのユーザグループ権限設定要求パラメータ</summary>
/// <param name="repogroupid">リポジトリグループIDまたはリポジトリグループパス</param>
/// <param name="usergroupid">ユーザグループIDまたはユーザグループ名</param>
/// <param name="perm">ユーザのリポジトリグループ権限</param>
/// <param name="apply_to_children">子要素に適用するかどうか</param>
public record GrantUserGroupPermToRepoGroupArgs(string repogroupid, string usergroupid, RepoGroupPerm perm, PermRecurse? apply_to_children = null);

/// <summary>リポジトリグループのユーザグループ権限解除要求パラメータ</summary>
/// <param name="repogroupid">リポジトリグループIDまたはリポジトリグループパス</param>
/// <param name="usergroupid">ユーザグループIDまたはユーザグループ名</param>
/// <param name="apply_to_children">子要素に適用するかどうか</param>
public record RevokeUserGroupPermFromToRepoGroupArgs(string repogroupid, string usergroupid, PermRecurse? apply_to_children = null);

/// <summary>Gist IDを指定する要求パラメータ</summary>
/// <param name="gistid">Gist ID</param>
public record GistArgs(string gistid);

/// <summary>Gist作成要求パラメータ</summary>
/// <param name="files">Gist内容ファイル</param>
/// <param name="description">説明</param>
/// <param name="owner">所有ユーザ</param>
/// <param name="gist_type">公開種別</param>
/// <param name="lifetime">有効期間。登録時点からの有効時間 [minutes]</param>
public record CreateGistArgs(PropertySet<GistContent> files, string description = "", GistType gist_type = GistType.@public, string? owner = null, int? lifetime = null);

/// <summary>リポジトリのチェンジセット一覧取得要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="start">取得対象範囲：開始リビジョン</param>
/// <param name="end">取得対象範囲：終了リビジョン</param>
/// <param name="start_date">取得対象範囲：開始日時</param>
/// <param name="end_date">取得対象範囲：終了日時</param>
/// <param name="branch_name">ブランチ名</param>
/// <param name="reverse">逆順取得するか否か</param>
/// <param name="with_file_list">ファイルリストを取得するか否か</param>
/// <param name="max_revisions">最大取得リビジョン数</param>
public record GetChangesetsArgs(
    string repoid, string? start = null, string? end = null, string? start_date = null, string? end_date = null,
    string? branch_name = null, bool? reverse = null, bool? with_file_list = null, string? max_revisions = null
 );

/// <summary>リポジトリのチェンジセット情報取得要求パラメータ</summary>
/// <param name="repoid">リポジトリIDまたはリポジトリパス</param>
/// <param name="raw_id">取得対象リビジョン</param>
/// <param name="with_reviews">レビュア情報を取得するか否か</param>
public record GetChangesetArgs(string repoid, string raw_id, bool? with_reviews = null);

/// <summary>プルリクエスト情報取得要求パラメータ</summary>
/// <param name="pullrequest_id">プルリクエストID</param>
public record PullRequestArgs(string pullrequest_id);

/// <summary>プルリクエストへのコメント追加/状態更新要求パラメータ</summary>
/// <param name="pull_request_id">プルリクエストID</param>
/// <param name="comment_msg">コメントメッセージ</param>
/// <param name="status">設定するステータス</param>
/// <param name="close_pr">プルリクエストを閉じるか否か</param>
public record CommentPullRequestArgs(string pull_request_id, string comment_msg = "", PullRequestStatus? status = null, bool? close_pr = null);

/// <summary>プルリクエストのレビュアー更新要求パラメータ</summary>
/// <param name="pull_request_id">プルリクエストID</param>
/// <param name="add">追加するレビュアー</param>
/// <param name="remove">削除するレビュアー</param>
public record EditPullRequestReviewersArgs(string pull_request_id, string[]? add = null, string[]? remove = null);


/// <summary>API応答コンテナ</summary>
/// <typeparam name="T">応答情報</typeparam>
/// <param name="id">要求識別子</param>
/// <param name="result">結果の応答情報</param>
public record ApiResponse<T>(string id, T result);

/// <summary>リモートからのpull要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="repository">リポジトリ名</param>
public record PullResult(string msg, string repository);

/// <summary>リポジトリの再スキャン要求 応答情報</summary>
/// <param name="added">追加されたリポジトリ名</param>
/// <param name="removed">削除されたリポジトリ名</param>
public record RescanReposResult(string[] added, string[] removed);

/// <summary>リポジトリのキャッシュ無効化要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="repository">リポジトリ名</param>
public record InvalidateCacheResult(string msg, string repository);

/// <summary>ユーザIPホワイトリストとサーバから見たIPの取得要求 応答情報</summary>
/// <param name="server_ip_addr">サーバからみたIPアドレス</param>
/// <param name="user_ips">ユーザのIPホワイトリスト</param>
public record GetIpResult(string server_ip_addr, IpAddrInfo[] user_ips);

/// <summary>サーバ情報の取得要求 応答情報</summary>
/// <param name="modules">モジュール情報の一覧</param>
/// <param name="py_version">Python バージョン</param>
/// <param name="platform">プラットフォーム種別</param>
/// <param name="kallithea_version">Kallithea バージョン</param>
public record GetServerInfoResult(ModuleInfo[] modules, string py_version, string platform, string kallithea_version);

/// <summary>ユーザ情報の取得要求 応答情報</summary>
/// <param name="user">ユーザ情報</param>
/// <param name="permissions">パーミッション情報</param>
public record GetUserResult(UserInfo user, UserPermissions permissions);

/// <summary>ユーザの一覧取得要求 応答情報</summary>
/// <param name="users">ユーザ一覧</param>
public record GetUsersResult(UserInfo[] users);

/// <summary>ユーザの作成要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="user">ユーザ情報</param>
public record CreateUserResult(string msg, UserInfo user);

/// <summary>ユーザの更新要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="user">ユーザ情報</param>
public record UpdateUserResult(string msg, UserInfo user);

/// <summary>ユーザの削除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
public record DeleteUserResult(string msg);

/// <summary>ユーザグループ情報の取得要求 応答情報</summary>
public record GetUserGroupResult(UserGroupInfo user_group);

/// <summary>ユーザグループ一覧の取得要求 応答情報</summary>
public record GetUserGroupsResult(UserGroupInfo[] user_groups);

/// <summary>ユーザグループの作成要求 応答情報</summary>
public record CreateUserGroupResult(string msg, UserGroupInfo user_group);

/// <summary>ユーザグループの更新要求 応答情報</summary>
public record UpdateUserGroupResult(string msg, UserGroupInfo user_group);

/// <summary>ユーザグループの削除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
public record DeleteUserGroupResult(string msg);

/// <summary>ユーザグループへのユーザを追加要求 応答情報</summary>
/// <param name="success">追加したか否か</param>
/// <param name="msg">メッセージ</param>
public record AddUserToUserGroupResult(bool success, string msg);

/// <summary>ユーザグループからのユーザ削除要求 応答情報</summary>
/// <param name="success">削除したか否か</param>
/// <param name="msg">メッセージ</param>
public record RemoveUserToUserGroupResult(bool success, string msg);

/// <summary>リポジトリ情報取得要求 応答情報</summary>
/// <param name="repo">リポジトリ情報</param>
/// <param name="members">権限設定されたユーザおよびユーザグループ</param>
/// <param name="followers">フォロワーユーザ</param>
/// <param name="revs">名前付きリビジョン情報(リビジョン情報取得が有効時)</param>
/// <param name="pull_requests">プルリクエスト一覧(プルリクエスト情報取得が有効時)</param>
public record GetRepoResult(RepoInfoEx repo, Member[] members, UserInfo[] followers, NamedRevs? revs, PullRequest[]? pull_requests);

/// <summary>リポジトリ一覧取得要求 応答情報</summary>
/// <param name="repos">リポジトリ一覧</param>
public record GetReposResult(RepoInfoEx[] repos);

/// <summary>リポジトリリビジョン内のノード一覧取得要求 応答情報</summary>
/// <param name="nodes">ノード一覧</param>
public record GetRepoNodesResult(RepoNode[] nodes);

/// <summary>リポジトリ作成要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record CreateRepoResult(string msg, bool success);

/// <summary>リポジトリ更新要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="repository">リポジトリ情報</param>
public record UpdateRepoResult(string msg, RepoInfoEx repository);

/// <summary>リポジトリフォーク要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record ForkRepoResult(string msg, bool success);

/// <summary>リポジトリ削除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record DeleteRepoResult(string msg, bool success);

/// <summary>リポジトリへのユーザ権限設定要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record GrantUserPermToRepoResult(string msg, bool success);

/// <summary>リポジトリのユーザ権限解除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record RevokeUserPermFromRepoResult(string msg, bool success);

/// <summary>リポジトリへのユーザグループ権限設定要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record GrantUserGroupPermToRepoResult(string msg, bool success);

/// <summary>リポジトリのユーザグループ権限解除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record RevokeUserGroupPermFromRepoResult(string msg, bool success);

/// <summary>リポジトリグループの情報取得要求 応答情報</summary>
/// <param name="repogroup">リポジトリグループ情報</param>
/// <param name="members">権限設定されたユーザおよびユーザグループ</param>
public record GetRepoGroupResult(RepoGroupInfo repogroup, Member[] members);

/// <summary>リポジトリグループの一覧取得要求 応答情報</summary>
/// <param name="repogroups">リポジトリグループの一覧</param>
public record GetRepoGroupsResult(RepoGroupInfo[] repogroups);

/// <summary>リポジトリグループ作成要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="repo_group">リポジトリグループ情報</param>
public record CreateRepoGroupResult(string msg, RepoGroupInfo repo_group);

/// <summary>リポジトリグループ更新要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="repo_group">リポジトリグループ情報</param>
public record UpdateRepoGroupResult(string msg, RepoGroupInfo repo_group);

/// <summary>リポジトリグループ削除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
public record DeleteRepoGroupResult(string msg);

/// <summary>リポジトリグループへのユーザ権限設定要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record GrantUserPermToRepoGroupResult(string msg, bool success);

/// <summary>リポジトリグループからのユーザ権限解除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record RevokeUserPermFromRepoGroupResult(string msg, bool success);

/// <summary>リポジトリグループへのユーザグループ権限設定要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record GrantUserGroupPermToRepoGroupResult(string msg, bool success);

/// <summary>リポジトリグループからのユーザグループ権限解除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record RevokeUserGroupPermFromRepoGroupResult(string msg, bool success);

/// <summary>Gist情報取得 応答情報</summary>
/// <param name="gist">Gist情報</param>
public record GetGistResult(GistInfo gist);

/// <summary>Gist一覧取得 応答情報</summary>
/// <param name="gists">Gist一覧</param>
public record GetGistsResult(GistInfo[] gists);

/// <summary>Gist作成要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
/// <param name="gist">Gist情報</param>
public record CreateGistResult(string msg, GistInfo gist);

/// <summary>Gist削除要求 応答情報</summary>
/// <param name="msg">メッセージ</param>
public record DeleteGistResult(string msg);

/// <summary>チェンジセット一覧取得 応答情報</summary>
/// <param name="changesets">チェンジセット一覧</param>
public record GetChangesetsResult(Changeset[] changesets);

/// <summary>チェンジセット情報取得 応答情報</summary>
/// <param name="summary">チェンジセット要約情報</param>
/// <param name="filelist">チェンジセット変更ファイルリスト</param>
/// <param name="reviews">レビュー情報</param>
public record GetChangesetResult(ChangesetSummary2 summary, ChangesetFileList filelist, Status[]? reviews);

/// <summary>プルリクエスト情報取得 応答情報</summary>
/// <param name="pullrequest">プルリクエスト情報</param>
public record GetPullRequestResult(PullRequest pullrequest);

/// <summary>プルリクエストへのコメント追加/状態更新 応答情報</summary>
/// <param name="success">エラーなく応答が取得された場合は常に true </param>
public record CommentPullRequestResult(bool success);

/// <summary>プルリクエストのレビュアー更新 応答情報</summary>
/// <param name="added">追加されたレビュア</param>
/// <param name="already_present">既に追加されていたレビュア</param>
/// <param name="removed">削除された(または元々対象でない)レビュア</param>
public record EditPullRequestReviewersResult(string[] added, string[] already_present, string[] removed);
