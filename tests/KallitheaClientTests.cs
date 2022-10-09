using FluentAssertions;

namespace KallitheaApiClient.Tests;

// 現状、テストコードは実装過程に使った呼び出し起点程度の状態となる。


[TestClass()]
public class KallitheaClientTests
{
    public Uri ApiEntry { get; } = new Uri(@"http://localhost:9999/_admin/api");
    public string ApiKey { get; } = "086c66326616d606874f3a726ce98aa9928bbc82";
    public HttpClient Client { get; } = new HttpClient();


    [TestMethod()]
    public async Task PullAsyncAsync()
    {
        var repoid = "users/foo/repoP1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.PullAsync(new(repoid), id: "testid");
        response.id.Should().Be("testid");
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.repository.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task InvalidateCacheAsync()
    {
        var repoid = "users/foo/repo1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.InvalidateCacheAsync(new(repoid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetIpAsync_ById()
    {
        var userid = "4";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetIpAsync(new(userid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetIpAsync_ByName()
    {
        var userid = "bar";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetIpAsync(new(userid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetIpAsync_ByNone()
    {
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetIpAsync(id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetServerInfoAsync()
    {
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetServerInfoAsync(id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetUserAsync_ById()
    {
        var userid = "1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetUserAsync(new(userid), id: "testid");
        response.id.Should().Be("testid");
        response.result.user.user_id.Should().Be(1);
    }

    [TestMethod()]
    public async Task GetUserAsync_ByName()
    {
        var userid = "bar";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetUserAsync(new(userid), id: "testid");
        response.id.Should().Be("testid");
        response.result.user.user_id.Should().Be(4);
    }

    [TestMethod()]
    public async Task GetUserAsync_ByNone()
    {
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetUserAsync(id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetUsersAsync()
    {
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetUsersAsync(id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task CreateUserAsync_Min()
    {
        var userid = "aaa";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.CreateUserAsync(new(userid, "aaa@example.com", "first", "last"), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteUserAsync(new(userid));
    }

    [TestMethod()]
    public async Task CreateUserAsync_Max()
    {
        var userid = "aaa";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.CreateUserAsync(new(userid, "bbb@example.com", "first", "last", "pass", active: true, admin: false), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteUserAsync(new(userid));
    }

    [TestMethod()]
    public async Task UpdateUserAsync_Min()
    {
        var userid = "aaa";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateUserAsync(new(userid, "aaa@example.com", "first", "last"));
        var response = await client.UpdateUserAsync(new(userid), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteUserAsync(new(userid));
    }

    [TestMethod()]
    public async Task UpdateUserAsync_Max()
    {
        var userid = "aaa";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateUserAsync(new(userid, "aaa@example.com", "first", "last"));
        var response = await client.UpdateUserAsync(new(userid, userid + "2", "asd@example.com", "new-first", "new-last", "new-pass", active: false, admin: false), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteUserAsync(new(userid + "2"));
    }

    [TestMethod()]
    public async Task DeleteUserAsync_ByName()
    {
        var userid = "aaa";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateUserAsync(new(userid, "aaa@example.com", "first", "last"));
        var response = await client.DeleteUserAsync(new(userid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetUserGroupAsync_ById()
    {
        var usergroupid = "1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetUserGroupAsync(new(usergroupid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetUserGroupAsync_ByName()
    {
        var usergroupid = "all";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetUserGroupAsync(new(usergroupid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetUserGroupsAsync()
    {
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetUserGroupsAsync(id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task CreateUserGroupAsync()
    {
        var group_name = "test";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.CreateUserGroupAsync(new(group_name), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteUserGroupAsync(new(group_name));
    }

    [TestMethod()]
    public async Task UpdateUserGroupAsync_Min()
    {
        var group_name = "test";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateUserGroupAsync(new(group_name));
        var response = await client.UpdateUserGroupAsync(new(group_name), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteUserGroupAsync(new(group_name));
    }

    [TestMethod()]
    public async Task DeleteUserGroupAsync()
    {
        var group_name = "test";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateUserGroupAsync(new(group_name));
        var response = await client.DeleteUserGroupAsync(new(group_name), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task AddUserToUserGroupAsync()
    {
        var group_name = "test";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateUserGroupAsync(new(group_name));
        var response = await client.AddUserToUserGroupAsync(new(group_name, "foo"), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteUserGroupAsync(new(group_name));
    }

    [TestMethod()]
    public async Task DeleteUserFromUserGroupAsync()
    {
        var group_name = "test";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateUserGroupAsync(new(group_name));
        await client.AddUserToUserGroupAsync(new(group_name, "foo"));
        var response = await client.DeleteUserFromUserGroupAsync(new(group_name, "foo"), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteUserGroupAsync(new(group_name));
    }

    [TestMethod()]
    public async Task GetRepoAsync_Normal()
    {
        var repoid = "users/foo/repo1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetRepoAsync(new(repoid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetRepoAsync_WithRevs()
    {
        var repoid = "users/foo/repoP1-fork";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetRepoAsync(new(repoid, with_revision_names: true), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetRepoAsync_WithPullReq()
    {
        var repoid = "users/foo/repoP1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetRepoAsync(new(repoid, with_pullrequests: true), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetReposAsync()
    {
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetReposAsync(id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetRepoNodesAsync()
    {
        var repoid = "users/foo/repo3";
        var revision = "HEAD";
        var path = "";
        var type = "all";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetRepoNodesAsync(new(repoid, revision, path, type), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task CreateRepoAsync()
    {
        var repo_name = "share/test_repo";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.CreateRepoAsync(new(repo_name), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoAsync(new(repo_name));
    }

    [TestMethod()]
    public async Task UpdateRepoAsync()
    {
        var repoid = "share/test_repo";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoAsync(new(repoid));
        var response = await client.UpdateRepoAsync(new(repoid, "tttt"), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoAsync(new("share/tttt"));
    }

    [TestMethod()]
    public async Task ForkRepoAsync()
    {
        var repoid = "users/foo/repo3";
        var forkrepo = "users/foo/repo3-fork";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.ForkRepoAsync(new(repoid, forkrepo), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoAsync(new(forkrepo));
    }

    [TestMethod()]
    public async Task DeleteRepoAsync()
    {
        var repoid = "share/test_repo";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoAsync(new(repoid));
        var response = await client.DeleteRepoAsync(new(repoid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task DeleteRepoAsync_AndFork()
    {
        var repoid = "share/test_repo";
        var forkrepo = "share/test_repo-fork";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoAsync(new(repoid));
        await client.ForkRepoAsync(new(repoid, forkrepo));
        var response = await client.DeleteRepoAsync(new(repoid, "delete"), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task DeleteRepoAsync_WithoutFork()
    {
        var repoid = "share/test_repo";
        var forkrepo = "share/test_repo-fork";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoAsync(new(repoid));
        await client.ForkRepoAsync(new(repoid, forkrepo));
        var response = await client.DeleteRepoAsync(new(repoid, "detach"), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoAsync(new(forkrepo));
    }

    [TestMethod()]
    public async Task GrantUserPermissionAsync()
    {
        var repoid = "share/test_repo";
        var user = "bar";
        var perm = "repository.admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoAsync(new(repoid));
        var response = await client.GrantUserPermToRepoAsync(new(repoid, user, perm), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoAsync(new(repoid));
    }

    [TestMethod()]
    public async Task RevokeUserPermFromRepoAsync()
    {
        var repoid = "share/test_repo";
        var user = "bar";
        var perm = "repository.admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoAsync(new(repoid));
        await client.GrantUserPermToRepoAsync(new(repoid, user, perm));
        var response = await client.RevokeUserPermFromRepoAsync(new(repoid, user), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoAsync(new(repoid));
    }

    [TestMethod()]
    public async Task GrantUserGroupPermToRepoAsync()
    {
        var repoid = "share/test_repo";
        var usergroup = "all";
        var perm = "repository.admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoAsync(new(repoid));
        var response = await client.GrantUserGroupPermToRepoAsync(new(repoid, usergroup, perm), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoAsync(new(repoid));
    }

    [TestMethod()]
    public async Task RevokeUserGroupPermFromRepoAsync()
    {
        var repoid = "share/test_repo";
        var usergroup = "all";
        var perm = "repository.admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoAsync(new(repoid));
        await client.GrantUserGroupPermToRepoAsync(new(repoid, usergroup, perm));
        var response = await client.RevokeUserGroupPermFromRepoAsync(new(repoid, usergroup), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoAsync(new(repoid));
    }

    [TestMethod()]
    public async Task GetReposGroupAsync()
    {
        var repogroupid = "users/foo";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetRepoGroupAsync(new(repogroupid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetRepoGroupsAsync()
    {
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetRepoGroupsAsync(id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task CreateRepoGroupAsync()
    {
        var parent = "users/foo";
        var group = "poe";

        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.CreateRepoGroupAsync(new(group, parent: parent), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoGroupAsync(new($"{parent}/{group}"));
    }

    [TestMethod()]
    public async Task UpdateRepoGroupAsync()
    {
        var parent = "users/foo";
        var group = "poe";
        var new_group = "puyo";

        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoGroupAsync(new(group, parent: parent));
        var response = await client.UpdateRepoGroupAsync(new($"{parent}/{group}", group_name: new_group), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoGroupAsync(new($"{parent}/{new_group}"));
    }

    [TestMethod()]
    public async Task DeleteRepoGroupAsync()
    {
        var parent = "users/foo";
        var group = "poe";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoGroupAsync(new(group, parent: parent));
        var response = await client.DeleteRepoGroupAsync(new($"{parent}/{group}"), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GrantUserPermToRepoGroupAsync()
    {
        var parent = "share";
        var group = "poe";
        var user = "bar";
        var perm = "group.admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoGroupAsync(new(group, parent: parent));
        var response = await client.GrantUserPermToRepoGroupAsync(new($"{parent}/{group}", user, perm), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoGroupAsync(new($"{parent}/{group}"));
    }

    [TestMethod()]
    public async Task RevokeUserPermFromRepoGroupAsync()
    {
        var parent = "share";
        var group = "poe";
        var user = "bar";
        var perm = "group.admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoGroupAsync(new(group, parent: parent));
        await client.GrantUserPermToRepoGroupAsync(new($"{parent}/{group}", user, perm));
        var response = await client.RevokeUserPermFromRepoGroupAsync(new($"{parent}/{group}", user), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoGroupAsync(new($"{parent}/{group}"));
    }

    [TestMethod()]
    public async Task GrantUserGroupPermToRepoGroupAsync()
    {
        var parent = "share";
        var group = "poe";
        var usergroup = "all";
        var perm = "group.admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoGroupAsync(new(group, parent: parent));
        var response = await client.GrantUserGroupPermToRepoGroupAsync(new($"{parent}/{group}", usergroup, perm), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoGroupAsync(new($"{parent}/{group}"));
    }

    [TestMethod()]
    public async Task RevokeUserGroupPermFromRepoGroupAsync()
    {
        var parent = "share";
        var group = "poe";
        var usergroup = "all";
        var perm = "group.admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        await client.CreateRepoGroupAsync(new(group, parent: parent));
        await client.GrantUserGroupPermToRepoGroupAsync(new($"{parent}/{group}", usergroup, perm));
        var response = await client.RevokeUserGroupPermFromRepoGroupAsync(new($"{parent}/{group}", usergroup), id: "testid");
        response.id.Should().Be("testid");

        await client.DeleteRepoGroupAsync(new($"{parent}/{group}"));
    }

    [TestMethod()]
    public async Task GetGistAsync()
    {
        var gistid = "4";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetGistAsync(new(gistid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetGistsAsync_ByID()
    {
        var userid = "admin";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetGistsAsync(new(userid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetGistsAsync_ByNone()
    {
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetGistsAsync(id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task CreateGistArgs_Single()
    {
        var files = new Dictionary<string, GistContent>()
        {
            { "aaa.cs", new GistContent("aaaaa", "csharp") },
            { "bbb.cs", new GistContent("bbbbb", "csharp") },
        };
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.CreateGistAsync(new(files), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetChangesetsAsync_Min()
    {
        var repoid = "users/foo/repo1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetChangesetsAsync(new(repoid), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetChangesetsAsync_WithChanges()
    {
        var repoid = "users/foo/repo1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetChangesetsAsync(new(repoid, with_file_list: true), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetChangesetsAsync_Reverse()
    {
        var repoid = "users/foo/repoP1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetChangesetsAsync(new(repoid, with_file_list: true, reverse: false), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetChangesetAsync()
    {
        var repoid = "users/foo/repo1";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetChangesetAsync(new(repoid, "6009a393feb1"), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task GetPullRequestAsync()
    {
        var pullrequest_id = "2";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.GetPullRequestAsync(new(pullrequest_id), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task CommentPullRequestAsync()
    {
        var pullrequest_id = "2";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.CommentPullRequestAsync(new(pullrequest_id), id: "testid");
        response.id.Should().Be("testid");
    }

    [TestMethod()]
    public async Task EditPullRequestReviewersAsync()
    {
        var pullrequest_id = "2";
        var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);
        var response = await client.EditPullRequestReviewersAsync(new(pullrequest_id, add: new[] { "foo", "bar" }), id: "testid");
        response.id.Should().Be("testid");
    }

}