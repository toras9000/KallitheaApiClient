using System.Net;
using FluentAssertions;
using KallitheaApiClientTest._Test;

namespace KallitheaApiClient.Tests;

// 現状、テストコードは実装過程に使った呼び出し起点程度の状態となる。


[TestClass()]
public class KallitheaClientTests
{
    public Uri ApiEntry { get; } = new Uri(@"http://localhost:9999/_admin/api");
    public string ApiKey { get; } = "1111222233334444555566667777888899990000";
    public HttpClient Client { get; } = new HttpClient();


    [TestMethod()]
    public async Task PullAsyncAsync()
    {
        // init
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        // test data
        var reqid = "abcd";
        var repoid = "users/foo/clone1";

        // test call & validate
        var response = await client.PullAsync(new(repoid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.repository.Should().Be(repoid);
    }

    [TestMethod()]
    public async Task InvalidateCacheAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";

        var response = await client.InvalidateCacheAsync(new(repoid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.repository.Should().Be(repoid);
    }

    [TestMethod()]
    public async Task GetIpAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var userid = "admin";

        var response = await client.GetIpAsync(new(userid), id: reqid);
        response.id.Should().Be(reqid);
        IPAddress.TryParse(response.result.server_ip_addr, out var _).Should().BeTrue();
    }

    [TestMethod()]
    public async Task GetServerInfoAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";

        var response = await client.GetServerInfoAsync(id: reqid);
        response.id.Should().Be(reqid);
        response.result.py_version.Should().NotBeNullOrWhiteSpace();
        response.result.kallithea_version.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod()]
    public async Task GetUserAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var userid = "bar";

        var response = await client.GetUserAsync(new(userid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.user.user_id.Should().Be(4);
        response.result.permissions.global.Should().NotBeEmpty();
        response.result.permissions.repositories.Select(i => i.name).Should().Contain(new[] { "users/bar/repo1", "users/bar/repo2", });
        response.result.permissions.repositories_groups.Select(i => i.name).Should().Contain(new[] { "share", "users", });
    }

    [TestMethod()]
    public async Task GetUsersAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";

        var response = await client.GetUsersAsync(id: reqid);
        response.id.Should().Be(reqid);
        response.result.users.Should().Satisfy(
            u => u.username == "foo",
            u => u.username == "bar",
            u => u.username == "admin"
        );
    }

    [TestMethod()]
    public async Task CreateUserAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var userid = "aaa";
        var response = await client.CreateUserAsync(new(userid, "aaa@example.com", "first", "last"), id: reqid).WillBeDiscarded(resources);
        response.id.Should().Be(reqid);
        response.result.user.username.Should().Be(userid);
        response.result.user.firstname.Should().Be("first");
        response.result.user.lastname.Should().Be("last");
        response.result.user.email.Should().Be("aaa@example.com");
        response.result.user.active.Should().Be(true);
        response.result.user.admin.Should().Be(false);
    }

    [TestMethod()]
    public async Task CreateUserAsync_Max()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var userid = "bbb";
        var response = await client.CreateUserAsync(new(userid, "bbb@example.com", "first", "last", "pass", active: true, admin: false), id: reqid).WillBeDiscarded(resources);
        response.id.Should().Be(reqid);
        response.result.user.username.Should().Be(userid);
        response.result.user.firstname.Should().Be("first");
        response.result.user.lastname.Should().Be("last");
        response.result.user.email.Should().Be("bbb@example.com");
        response.result.user.active.Should().Be(true);
        response.result.user.admin.Should().Be(false);
    }

    [TestMethod()]
    public async Task UpdateUserAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var userid = "aaa";
        var testuser = await resources.CreateTestUserAsync(new(userid, "aaa@example.com", "first", "last"));

        var response = await client.UpdateUserAsync(new(userid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.user.username.Should().Be(userid);
        response.result.user.firstname.Should().Be("first");
        response.result.user.lastname.Should().Be("last");
        response.result.user.email.Should().Be("aaa@example.com");
        response.result.user.active.Should().Be(true);
        response.result.user.admin.Should().Be(false);
    }

    [TestMethod()]
    public async Task UpdateUserAsync_Max()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var userid = "aaa";
        var testuser = await resources.CreateTestUserAsync(new(userid, "aaa@example.com", "first", "last"));

        var response = await client.UpdateUserAsync(new(userid, userid + "2", "asd@example.com", "new-first", "new-last", "new-pass", active: false, admin: false), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.user.user_id.Should().Be(testuser.user_id);
        response.result.user.username.Should().Be(userid + "2");
        response.result.user.firstname.Should().Be("new-first");
        response.result.user.lastname.Should().Be("new-last");
        response.result.user.email.Should().Be("asd@example.com");
        response.result.user.active.Should().Be(false);
        response.result.user.admin.Should().Be(false);
    }

    [TestMethod()]
    public async Task DeleteUserAsync_ByName()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var userid = "aaa";
        var testuser = await resources.CreateTestUserAsync(new(userid, "aaa@example.com", "first", "last"));

        var response = await client.DeleteUserAsync(new(userid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task GetUserGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var usergroupid = "all";

        var response = await client.GetUserGroupAsync(new(usergroupid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.user_group.owner.Should().Be("admin");
        response.result.user_group.members.Should().Satisfy(
            u => u.username == "foo",
            u => u.username == "bar",
            u => u.username == "admin"
        );
    }

    [TestMethod()]
    public async Task GetUserGroupsAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";

        var response = await client.GetUserGroupsAsync(id: reqid);
        response.id.Should().Be(reqid);
        response.result.user_groups.Should().Satisfy(
            g => g.group_name == "all",
            g => g.group_name == "tester"
        );
    }

    [TestMethod()]
    public async Task CreateUserGroupAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var group_name = "test";
        var response = await client.CreateUserGroupAsync(new(group_name), id: reqid).WillBeDiscarded(resources);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.user_group.group_name.Should().Be(group_name);
        response.result.user_group.group_description.Should().BeNullOrEmpty();
        response.result.user_group.active.Should().Be(true);
        response.result.user_group.owner.Should().Be("admin");
        response.result.user_group.members.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task CreateUserGroupAsync_Max()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var group_name = "test";
        var response = await client.CreateUserGroupAsync(new(group_name, "desc-text", "foo", false), id: reqid).WillBeDiscarded(resources);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.user_group.group_name.Should().Be(group_name);
        response.result.user_group.group_description.Should().Be("desc-text");
        response.result.user_group.active.Should().Be(false);
        response.result.user_group.owner.Should().Be("foo");
        response.result.user_group.members.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task UpdateUserGroupAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var group_name = "test";
        var testusergroup = await resources.CreateTestUserGroupAsync(new(group_name));

        var response = await client.UpdateUserGroupAsync(new(group_name), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.user_group.users_group_id.Should().Be(testusergroup.users_group_id);
        response.result.user_group.group_name.Should().Be(group_name);
        response.result.user_group.group_description.Should().BeNullOrEmpty();
        response.result.user_group.active.Should().Be(true);
        response.result.user_group.owner.Should().Be("admin");
        response.result.user_group.members.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task UpdateUserGroupAsync_Max()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var group_name = "test";
        var testusergroup = await resources.CreateTestUserGroupAsync(new(group_name));

        var response = await client.UpdateUserGroupAsync(new(group_name, "new-name", "new-desc", "bar", false), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.user_group.users_group_id.Should().Be(testusergroup.users_group_id);
        response.result.user_group.group_name.Should().Be("new-name");
        response.result.user_group.group_description.Should().Be("new-desc");
        response.result.user_group.active.Should().Be(false);
        response.result.user_group.owner.Should().Be("bar");
        response.result.user_group.members.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task DeleteUserGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var group_name = "test";
        await client.CreateUserGroupAsync(new(group_name), id: reqid);

        var response = await client.DeleteUserGroupAsync(new(group_name), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task AddUserToUserGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var group_name = "test";
        var testusergroup = await resources.CreateTestUserGroupAsync(new(group_name));

        var response = await client.AddUserToUserGroupAsync(new(group_name, "foo"), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetUserGroupAsync(new(group_name), id: reqid)).result.user_group.members.Should().Satisfy(
            u => u.username == "foo"
        );
    }

    [TestMethod()]
    public async Task RemoveUserFromUserGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var group_name = "test";
        var testusergroup = await resources.CreateTestUserGroupAsync(new(group_name));
        await client.AddUserToUserGroupAsync(new(group_name, "foo"));
        (await client.GetUserGroupAsync(new(group_name), id: reqid)).result.user_group.members.Should().Satisfy(u => u.username == "foo");

        var response = await client.RemoveUserFromUserGroupAsync(new(group_name, "foo"), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetUserGroupAsync(new(group_name), id: reqid)).result.user_group.members.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task GetRepoAsync_Normal()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/bar/fork-foo-clone1";

        var response = await client.GetRepoAsync(new(repoid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.repo.repo_name.Should().Be(repoid);
        response.result.repo.repo_type.Should().Be("git");
        response.result.repo.clone_uri.Should().BeNullOrEmpty();
        response.result.repo.@private.Should().Be(false);
        response.result.repo.created_on.Should().NotBeEmpty();
        response.result.repo.owner.Should().Be("bar");
        response.result.repo.fork_of.Should().Be("users/foo/clone1");
        response.result.repo.enable_downloads.Should().Be(false);
        response.result.repo.enable_statistics.Should().Be(false);
        response.result.repo.last_changeset.author.Should().Contain("toras9000");
        response.result.repo.last_changeset.date.Should().NotBeEmpty();
        response.result.repo.last_changeset.short_id.Should().NotBeEmpty();
        response.result.repo.last_changeset.message.Should().NotBeEmpty();
        response.result.members.Should()
            .Contain(m => m.name == "foo" && m.type == MemberType.user).And
            .Contain(m => m.name == "tester" && m.type == MemberType.user_group);
        response.result.followers.Should().Contain(f => f.username == "bar");
        if (response.result.revs != null)
        {
            response.result.revs.tags.Should().BeNullOrEmpty();
            response.result.revs.branches.Should().BeNullOrEmpty();
            response.result.revs.bookmarks.Should().BeNullOrEmpty();
        }
        response.result.pull_requests.Should().BeNullOrEmpty();
    }

    [TestMethod()]
    public async Task GetRepoAsync_WithRevs()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";

        var response = await client.GetRepoAsync(new(repoid, with_revision_names: true), id: reqid);
        response.id.Should().Be(reqid);
        response.result.revs.Should().NotBeNull();
        if (response.result.revs == null) return;
        response.result.revs.branches.Select(x => x.name).Should().Contain("main", "br1", "br2");
        response.result.revs.tags.Select(x => x.name).Should().Contain("tag1", "tag2");
        response.result.revs.bookmarks.Select(x => x.name).Should().BeNullOrEmpty();
    }

    [TestMethod()]
    public async Task GetRepoAsync_WithPullReq()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";

        var response = await client.GetRepoAsync(new(repoid, with_pullrequests: true), id: reqid);
        response.id.Should().Be(reqid);
        response.result.revs.Should().NotBeNull();
        if (response.result.revs == null) return;
        response.result.pull_requests.Should().NotBeNull();
    }

    [TestMethod()]
    public async Task GetReposAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";

        var response = await client.GetReposAsync(id: reqid);
        response.id.Should().Be(reqid);
        response.result.repos.Should().Contain(r => r.repo_name == "users/foo/repo1" && r.repo_type == "git");
        response.result.repos.Should().Contain(r => r.repo_name == "users/foo/repo2" && r.repo_type == "hg");
        response.result.repos.Should().Contain(r => r.repo_name == "users/foo/clone1" && r.repo_type == "git");
        response.result.repos.Should().Contain(r => r.repo_name == "users/foo/fork-bar-repo1" && r.repo_type == "git");
        response.result.repos.Should().Contain(r => r.repo_name == "users/bar/repo1" && r.repo_type == "git");
        response.result.repos.Should().Contain(r => r.repo_name == "users/bar/repo2" && r.repo_type == "hg");
        response.result.repos.Should().Contain(r => r.repo_name == "users/bar/fork-foo-repo1" && r.repo_type == "git");
        response.result.repos.Should().Contain(r => r.repo_name == "users/bar/fork-foo-clone1" && r.repo_type == "git");
    }

    [TestMethod()]
    public async Task GetRepoNodesAsync_Files()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo3";
        var revision = "HEAD";
        var path = "";
        var type = NodesType.files;

        var response = await client.GetRepoNodesAsync(new(repoid, revision, path, type), id: reqid);
        response.id.Should().Be(reqid);
        response.result.nodes.Should().Contain(n => n.name == "a.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "c.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "xxx/a.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "xxx/b.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "xxx/c.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "yyy/a.txt" && n.type == "file");
    }

    [TestMethod()]
    public async Task GetRepoNodesAsync_Dirs()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo3";
        var revision = "HEAD";
        var path = "";
        var type = NodesType.dirs;

        var response = await client.GetRepoNodesAsync(new(repoid, revision, path, type), id: reqid);
        response.id.Should().Be(reqid);
        response.result.nodes.Should().Contain(n => n.name == "xxx" && n.type == "dir");
        response.result.nodes.Should().Contain(n => n.name == "yyy" && n.type == "dir");
    }

    [TestMethod()]
    public async Task GetRepoNodesAsync_All()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo3";
        var revision = "HEAD";
        var path = "";
        var type = NodesType.all;

        var response = await client.GetRepoNodesAsync(new(repoid, revision, path, type), id: reqid);
        response.id.Should().Be(reqid);
        response.result.nodes.Should().Contain(n => n.name == "a.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "c.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "xxx" && n.type == "dir");
        response.result.nodes.Should().Contain(n => n.name == "xxx/a.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "xxx/b.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "xxx/c.txt" && n.type == "file");
        response.result.nodes.Should().Contain(n => n.name == "yyy" && n.type == "dir");
        response.result.nodes.Should().Contain(n => n.name == "yyy/a.txt" && n.type == "file");
    }

    [TestMethod()]
    public async Task CreateRepoAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repo_name = "share/test_repo";
        var response = await client.CreateRepoAsync(new(repo_name), id: reqid);
        try
        {
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrEmpty();

            var repo = (await client.GetRepoAsync(new(repo_name))).result.repo;
            repo.repo_name.Should().Be(repo_name);
            repo.repo_type.Should().Be("hg");
            repo.clone_uri.Should().BeNullOrEmpty();
            repo.@private.Should().Be(false);
            repo.created_on.Should().NotBeEmpty();
            repo.owner.Should().Be("admin");
            repo.fork_of.Should().BeNullOrEmpty();
            repo.enable_downloads.Should().Be(false);
            repo.enable_statistics.Should().Be(false);
        }
        finally
        {
            await client.DeleteRepoAsync(new(repo_name));
        }
    }

    [TestMethod()]
    public async Task CreateRepoAsync_Many()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repo_name = "share/test_repo";

        var args = new CreateRepoArgs(
            repo_name: repo_name,
            repo_type: RepoType.git,
            description: "test-desc",
            owner: "foo",
            @private: true,
            clone_uri: "https://github.com/toras9000/KallitheaApiClient.git"
        );
        var response = await client.CreateRepoAsync(args, id: reqid);
        try
        {
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrEmpty();

            var repo = (await client.GetRepoAsync(new(repo_name))).result.repo;
            repo.repo_name.Should().Be(repo_name);
            repo.repo_type.Should().Be("git");
            repo.description.Should().Be("test-desc");
            repo.clone_uri.Should().Be("https://github.com/toras9000/KallitheaApiClient.git");
            repo.@private.Should().Be(true);
            repo.owner.Should().Be("foo");
        }
        finally
        {
            await client.DeleteRepoAsync(new(repo_name));
        }
    }

    [TestMethod()]
    public async Task UpdateRepoAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var repoid = "share/test_repo";
        var testrepo = await resources.CreateTestRepoAsync(new(repoid, owner: "foo", repo_type: RepoType.git));

        var response = await client.UpdateRepoAsync(new(repoid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrWhiteSpace();
        response.result.repository.repo_name.Should().Be("share/test_repo");
        response.result.repository.repo_type.Should().Be("git");
        response.result.repository.clone_uri.Should().BeNullOrEmpty();
        response.result.repository.@private.Should().Be(false);
        response.result.repository.created_on.Should().NotBeEmpty();
        response.result.repository.owner.Should().Be("foo");
        response.result.repository.fork_of.Should().BeNullOrEmpty();
        response.result.repository.enable_downloads.Should().Be(false);
        response.result.repository.enable_statistics.Should().Be(false);
    }

    [TestMethod()]
    public async Task UpdateRepoAsync_Many()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var repoid = "share/test_repo";
        var restrepo = await resources.CreateTestRepoAsync(new(repoid, owner: "foo", repo_type: RepoType.git));

        var response = await client.UpdateRepoAsync(new(repoid, "test_ren", owner: "bar", description: "new-desc", @private: true, enable_downloads: true, enable_statistics: true), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrWhiteSpace();
        response.result.repository.repo_name.Should().Be("share/test_ren");
        response.result.repository.repo_type.Should().Be("git");
        response.result.repository.description.Should().Be("new-desc");
        response.result.repository.@private.Should().Be(true);
        response.result.repository.owner.Should().Be("bar");
        response.result.repository.fork_of.Should().BeNullOrEmpty();
        response.result.repository.enable_downloads.Should().Be(true);
        response.result.repository.enable_statistics.Should().Be(true);
    }

    [TestMethod()]
    public async Task ForkRepoAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";
        var forkrepo = "share/repo1-fork";

        var response = await client.ForkRepoAsync(new(repoid, forkrepo), id: reqid);
        try
        {
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrWhiteSpace();

            var repo = (await client.GetRepoAsync(new(forkrepo))).result.repo;
            repo.repo_name.Should().Be(forkrepo);
            repo.repo_type.Should().Be("git");
            repo.description.Should().NotBeNullOrEmpty();
            repo.clone_uri.Should().BeNullOrEmpty();
            repo.@private.Should().Be(false);
            repo.fork_of.Should().Be("users/foo/repo1");
            repo.owner.Should().Be("admin");
        }
        finally
        {
            await client.DeleteRepoAsync(new(forkrepo));
        }
    }

    [TestMethod()]
    public async Task ForkRepoAsync_Many()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";
        var forkrepo = "share/repo1-fork";

        var response = await client.ForkRepoAsync(new(repoid, forkrepo, owner: "bar", description: "fork-desc", @private: true), id: reqid);
        try
        {
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrWhiteSpace();

            var repo = (await client.GetRepoAsync(new(forkrepo))).result.repo;
            repo.repo_name.Should().Be(forkrepo);
            repo.repo_type.Should().Be("git");
            repo.description.Should().Be("fork-desc");
            repo.clone_uri.Should().BeNullOrEmpty();
            repo.@private.Should().Be(true);
            repo.fork_of.Should().Be("users/foo/repo1");
            repo.owner.Should().Be("bar");
        }
        finally
        {
            await client.DeleteRepoAsync(new(forkrepo));
        }
    }

    [TestMethod()]
    public async Task DeleteRepoAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "share/test_repo";
        await client.CreateRepoAsync(new(repoid));

        var response = await client.DeleteRepoAsync(new(repoid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task DeleteRepoAsync_AndFork()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var testname = $"share/test_repo_{timestamp}";
        var forkname = $"share/test_repo-fork_{timestamp}";

        await client.CreateRepoAsync(new(testname));
        await client.ForkRepoAsync(new(testname, forkname));

        try
        {
            var response = await client.DeleteRepoAsync(new(testname, ForksTreatment.delete), id: reqid);
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrEmpty();
        }
        catch
        {
            var repos = (await client.GetReposAsync()).result.repos;
            var forkrepo = repos.FirstOrDefault(r => r.repo_name == forkname);
            if (forkrepo != null) try { await client.DeleteRepoAsync(new(forkrepo.repo_id.ToString())); } catch { }
            var testrepo = repos.FirstOrDefault(r => r.repo_name == testname);
            if (testrepo != null) try { await client.DeleteRepoAsync(new(testrepo.repo_id.ToString())); } catch { }
            throw;
        }
    }

    [TestMethod()]
    public async Task DeleteRepoAsync_WithoutFork()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var testname = $"share/test_repo_{timestamp}";
        var forkname = $"share/test_repo-fork_{timestamp}";

        await client.CreateRepoAsync(new(testname));
        await client.ForkRepoAsync(new(testname, forkname));

        try
        {
            var response = await client.DeleteRepoAsync(new(testname, ForksTreatment.detach), id: reqid);
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrEmpty();

            var forkrepo = (await client.GetRepoAsync(new(forkname))).result.repo;
            forkrepo.fork_of.Should().BeNullOrEmpty();

            await client.DeleteRepoAsync(new(forkrepo.repo_id.ToString()));
        }
        catch
        {
            var repos = (await client.GetReposAsync()).result.repos;
            var forkrepo = repos.FirstOrDefault(r => r.repo_name == forkname);
            if (forkrepo != null) try { await client.DeleteRepoAsync(new(forkrepo.repo_id.ToString())); } catch { }
            var testrepo = repos.FirstOrDefault(r => r.repo_name == testname);
            if (testrepo != null) try { await client.DeleteRepoAsync(new(testrepo.repo_id.ToString())); } catch { }
            throw;
        }
    }

    [TestMethod()]
    public async Task GrantUserPermissionAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var repoid = $"share/test_repo_{DateTime.Now:yyyyMMdd_HHmmss}";
        var restrepo = await resources.CreateTestRepoAsync(new(repoid, owner: "foo", repo_type: RepoType.git));

        var response = await client.GrantUserPermToRepoAsync(new(repoid, "bar", RepoPerm.admin), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetRepoAsync(new(repoid))).result.members
            .Should().Contain(m => m.name == "bar" && m.type == MemberType.user && m.permission == RepoPerm.admin.ToPermName());

        await client.GrantUserPermToRepoAsync(new(repoid, "foo", RepoPerm.write), id: reqid);

        (await client.GetRepoAsync(new(repoid))).result.members
            .Should().Contain(m => m.name == "foo" && m.type == MemberType.user && m.permission == RepoPerm.write.ToPermName());

    }

    [TestMethod()]
    public async Task RevokeUserPermFromRepoAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var repoid = "share/test_repo";
        var restrepo = await resources.CreateTestRepoAsync(new(repoid, owner: "foo", repo_type: RepoType.git));
        await client.GrantUserPermToRepoAsync(new(repoid, "bar", RepoPerm.admin));

        var response = await client.RevokeUserPermFromRepoAsync(new(repoid, "default"), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetRepoAsync(new(repoid))).result.members
            .Should().NotContain(m => m.name == "default" && m.type == MemberType.user);

        await client.RevokeUserPermFromRepoAsync(new(repoid, "bar"), id: reqid);

        (await client.GetRepoAsync(new(repoid))).result.members
            .Should().NotContain(m => m.name == "bar" && m.type == MemberType.user);
    }

    [TestMethod()]
    public async Task GrantUserGroupPermToRepoAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var repoid = "share/test_repo";
        var testrepo = await resources.CreateTestRepoAsync(new(repoid, owner: "foo", repo_type: RepoType.git));

        var response = await client.GrantUserGroupPermToRepoAsync(new(repoid, "all", RepoPerm.read), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetRepoAsync(new(repoid))).result.members
            .Should().Contain(m => m.name == "all" && m.type == MemberType.user_group && m.permission == RepoPerm.read.ToPermName());

        await client.GrantUserGroupPermToRepoAsync(new(repoid, "tester", RepoPerm.admin), id: reqid);

        (await client.GetRepoAsync(new(repoid))).result.members
            .Should().Contain(m => m.name == "tester" && m.type == MemberType.user_group && m.permission == RepoPerm.admin.ToPermName());
    }

    [TestMethod()]
    public async Task RevokeUserGroupPermFromRepoAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var repoid = "share/test_repo";
        var testrepo = await resources.CreateTestRepoAsync(new(repoid, owner: "foo", repo_type: RepoType.git));
        await client.GrantUserGroupPermToRepoAsync(new(repoid, "all", RepoPerm.read), id: reqid);

        var response = await client.RevokeUserGroupPermFromRepoAsync(new(repoid, "all"), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetRepoAsync(new(repoid))).result.members
            .Should().NotContain(m => m.name == "all" && m.type == MemberType.user_group);
    }

    [TestMethod()]
    public async Task GetRepoGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repogroupid = "users/foo";

        var response = await client.GetRepoGroupAsync(new(repogroupid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.repogroup.group_name.Should().Be("users/foo");
        response.result.repogroup.parent_group.Should().Be("users");
        response.result.repogroup.owner.Should().Be("foo");
        response.result.repogroup.repositories.Should().Contain("users/foo/repo1", "users/foo/repo2", "users/foo/clone1");
        response.result.members.Should().Contain(m => m.name == "foo" && m.type == MemberType.user);
        response.result.members.Should().Contain(m => m.name == "tester" && m.type == MemberType.user_group);
    }

    [TestMethod()]
    public async Task GetRepoGroupsAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";

        var response = await client.GetRepoGroupsAsync(id: reqid);
        response.id.Should().Be(reqid);
        response.result.repogroups.Should().Contain(m => m.group_name == "share");
        response.result.repogroups.Should().Contain(m => m.group_name == "users");
        response.result.repogroups.Should().Contain(m => m.group_name == "users/foo");
        response.result.repogroups.Should().Contain(m => m.group_name == "users/bar");
    }

    [TestMethod()]
    public async Task CreateRepoGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var parent = "share";
        var group = "poe";

        var response = await client.CreateRepoGroupAsync(new(group, parent: parent), id: reqid);
        try
        {
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrEmpty();

            var repogroup = (await client.GetRepoGroupAsync(new($"{parent}/{group}"), id: reqid)).result;
            repogroup.repogroup.group_name.Should().Be($"{parent}/{group}");
        }
        finally
        {
            await client.DeleteRepoGroupAsync(new($"{parent}/{group}"));
        }
    }

    [TestMethod()]
    public async Task UpdateRepoGroupAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var parent = "share";
        var group = "poe";
        var repogroup = await resources.CreateTestRepoGroupAsync(new(group, parent: parent));

        var response = await client.UpdateRepoGroupAsync(new($"{parent}/{group}"), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.repo_group.group_name.Should().Be("share/poe");
        response.result.repo_group.parent_group.Should().Be("share");
        response.result.repo_group.owner.Should().Be("admin");
        response.result.repo_group.repositories.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task UpdateRepoGroupAsync_Many()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var parent = "share";
        var group = "poe";
        var repogroup = await resources.CreateTestRepoGroupAsync(new(group, parent: parent));

        var response = await client.UpdateRepoGroupAsync(new($"{parent}/{group}", group_name: "new-poe", description: "new-desc"), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
        response.result.repo_group.group_name.Should().Be("share/new-poe");
        response.result.repo_group.parent_group.Should().Be("share");
        response.result.repo_group.group_description.Should().Be("new-desc");
        response.result.repo_group.owner.Should().Be("admin");
        response.result.repo_group.repositories.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task DeleteRepoGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var parent = "users/foo";
        var group = "poe";

        var repogroup = (await client.CreateRepoGroupAsync(new(group, parent: parent))).result.repo_group;

        var response = await client.DeleteRepoGroupAsync(new(repogroup.group_name), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();
    }

    [TestMethod()]
    public async Task GrantUserPermToRepoGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var parent = "share";
        var group = "poe";
        var repogroup = await resources.CreateTestRepoGroupAsync(new(group, parent: parent));

        var response = await client.GrantUserPermToRepoGroupAsync(new(repogroup.group_name, "bar", RepoGroupPerm.admin), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetRepoGroupAsync(new(repogroup.group_name), id: reqid)).result.members
            .Should().Contain(m => m.name == "bar" && m.type == MemberType.user && m.permission == RepoGroupPerm.admin.ToPermName());

    }

    [TestMethod()]
    public async Task RevokeUserPermFromRepoGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var parent = "share";
        var group = "poe";
        var repogroup = await resources.CreateTestRepoGroupAsync(new(group, parent: parent));
        await client.GrantUserPermToRepoGroupAsync(new(repogroup.group_name, "foo", RepoGroupPerm.admin));

        var response = await client.RevokeUserPermFromRepoGroupAsync(new(repogroup.group_name, "foo"), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetRepoGroupAsync(new(repogroup.group_name), id: reqid)).result.members
            .Should().NotContain(m => m.name == "foo" && m.type == MemberType.user);
    }

    [TestMethod()]
    public async Task GrantUserGroupPermToRepoGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var parent = "share";
        var group = "poe";
        var repogroup = await resources.CreateTestRepoGroupAsync(new(group, parent: parent));

        var response = await client.GrantUserGroupPermToRepoGroupAsync(new(repogroup.group_name, "all", RepoGroupPerm.admin), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetRepoGroupAsync(new(repogroup.group_name), id: reqid)).result.members
            .Should().Contain(m => m.name == "all" && m.type == MemberType.user_group && m.permission == RepoGroupPerm.admin.ToPermName());
    }

    [TestMethod()]
    public async Task RevokeUserGroupPermFromRepoGroupAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var parent = "share";
        var group = "poe";
        var repogroup = await resources.CreateTestRepoGroupAsync(new(group, parent: parent));
        await client.GrantUserGroupPermToRepoGroupAsync(new(repogroup.group_name, "all", RepoGroupPerm.admin));

        var response = await client.RevokeUserGroupPermFromRepoGroupAsync(new(repogroup.group_name, "all"), id: reqid);
        response.id.Should().Be(reqid);
        response.result.msg.Should().NotBeNullOrEmpty();

        (await client.GetRepoGroupAsync(new(repogroup.group_name), id: reqid)).result.members
            .Should().NotContain(m => m.name == "all" && m.type == MemberType.user_group);
    }

    [TestMethod()]
    public async Task GetGistAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var reqid = "abcd";
        var gist = await resources.CreateTestGistAsync(new(new() { new("aaa.cs", new GistContent("aaaaa", "csharp")) }, description: "gist-desc", gist_type: GistType.@private, owner: "foo", lifetime: 500));

        var response = await client.GetGistAsync(new(gist.gist_id.ToString()), id: reqid);
        response.id.Should().Be(reqid);
        response.result.gist.type.Should().Be(GistType.@private.ToString());
        response.result.gist.access_id.Should().NotBeNullOrEmpty();
        response.result.gist.description.Should().Be("gist-desc");
        response.result.gist.url.Should().StartWith(this.ApiEntry.GetLeftPart(UriPartial.Authority));
        response.result.gist.created_on.Should().NotBeNullOrEmpty();
        response.result.gist.expires.Should().BeApproximately((DateTime.UtcNow.AddMinutes(500) - DateTime.UnixEpoch).TotalSeconds, 10); // 10秒ぐらいの差は許容する。
    }

    [TestMethod()]
    public async Task CreateGistArgs_Single()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var files = new PropertySet<GistContent>()
        {
            new("aaa.cs", new GistContent("aaaaa", "csharp")),
        };

        var response = await client.CreateGistAsync(new(files, description: "gist-desc", gist_type: GistType.@private, owner: "foo", lifetime: 500), id: reqid);
        try
        {
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrEmpty();
            response.result.gist.type.Should().Be(GistType.@private.ToString());
            response.result.gist.access_id.Should().NotBeNullOrEmpty();
            response.result.gist.description.Should().Be("gist-desc");
            response.result.gist.url.Should().StartWith(this.ApiEntry.GetLeftPart(UriPartial.Authority));
            response.result.gist.created_on.Should().NotBeNullOrEmpty();
            response.result.gist.expires.Should().BeApproximately((DateTime.UtcNow.AddMinutes(500) - DateTime.UnixEpoch).TotalSeconds, 10); // 10秒ぐらいの差は許容する。
        }
        finally
        {
            await client.DeleteGistAsync(new(response.result.gist.gist_id.ToString()));
        }
    }

    [TestMethod()]
    public async Task CreateGistArgs_Multi()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var files = new PropertySet<GistContent>()
        {
            new("aaa.cs", new GistContent("aaaaa", "csharp")),
            new("bbb.cs", new GistContent("bbbbb", "csharp")),
        };

        var response = await client.CreateGistAsync(new(files, description: "gist-desc", gist_type: GistType.@public, owner: "bar", lifetime: 100), id: reqid);
        try
        {
            response.id.Should().Be(reqid);
            response.result.msg.Should().NotBeNullOrEmpty();
            response.result.gist.type.Should().Be(GistType.@public.ToString());
            response.result.gist.access_id.Should().NotBeNullOrEmpty();
            response.result.gist.description.Should().Be("gist-desc");
            response.result.gist.url.Should().StartWith(this.ApiEntry.GetLeftPart(UriPartial.Authority));
            response.result.gist.created_on.Should().NotBeNullOrEmpty();
            response.result.gist.expires.Should().BeApproximately((DateTime.UtcNow.AddMinutes(100) - DateTime.UnixEpoch).TotalSeconds, 10); // 10秒ぐらいの差は許容する。
        }
        finally
        {
            await client.DeleteGistAsync(new(response.result.gist.gist_id.ToString()));
        }
    }

    [TestMethod()]
    public async Task GetChangesetsAsync_Min()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";

        var response = await client.GetChangesetsAsync(new(repoid), id: reqid);
        response.id.Should().Be(reqid);
        response.result.changesets.Should().AllSatisfy(c =>
        {
            c.summary.author.Should().Contain("foo");
            c.summary.short_id.Should().NotBeNullOrWhiteSpace();
            c.summary.raw_id.Should().NotBeNullOrWhiteSpace();
            c.filelist.Should().BeNull();
        });
        response.result.changesets.Should().SatisfyRespectively(
            c => c.summary.message.Should().Be("commit 1"),
            c => c.summary.message.Should().Be("commit 2"),
            c => c.summary.message.Should().Be("commit 3")
        );
    }

    [TestMethod()]
    public async Task GetChangesetsAsync_WithChanges()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";

        var response = await client.GetChangesetsAsync(new(repoid, with_file_list: true), id: reqid);
        response.id.Should().Be(reqid);
        response.result.changesets.Should().SatisfyRespectively(
            c =>
            {
                c.filelist!.added.Should().Equal("aaa.txt", "bbb.txt");
                c.filelist.changed.Should().BeEmpty();
                c.filelist.removed.Should().BeEmpty();
            },
            c =>
            {
                c.filelist!.added.Should().Equal("ccc.txt");
                c.filelist.changed.Should().Equal("aaa.txt");
                c.filelist.removed.Should().BeEmpty();
            },
            c =>
            {
                c.filelist!.added.Should().Equal("ddd.txt");
                c.filelist.changed.Should().BeEmpty();
                c.filelist.removed.Should().Equal("bbb.txt");
            }
        );
    }

    [TestMethod()]
    public async Task GetChangesetsAsync_Reverse()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";

        var response = await client.GetChangesetsAsync(new(repoid, reverse: true), id: reqid);
        response.id.Should().Be(reqid);
        response.result.changesets.Should().SatisfyRespectively(
            c => { c.summary.message.Should().Be("commit 3"); },
            c => { c.summary.message.Should().Be("commit 2"); },
            c => { c.summary.message.Should().Be("commit 1"); }
        );
    }

    [TestMethod()]
    public async Task GetChangesetAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var repoid = "users/foo/repo1";
        var changesets = (await client.GetChangesetsAsync(new(repoid), id: reqid)).result.changesets;

        var response = await client.GetChangesetAsync(new(repoid, changesets[0].summary.raw_id), id: reqid);
        response.id.Should().Be(reqid);
        response.result.summary.message.Should().Be("commit 1");
        response.result.summary.short_id.Should().NotBeNullOrWhiteSpace();
        response.result.summary.raw_id.Should().NotBeNullOrWhiteSpace();
        response.result.summary.author.name.Should().Be("foo");
        response.result.summary.author.email.Should().Contain("foo");
        response.result.filelist.added.Should().Equal("aaa.txt", "bbb.txt");
        response.result.filelist.changed.Should().BeEmpty();
        response.result.filelist.removed.Should().BeEmpty();
    }

    [TestMethod()]
    public async Task GetChangesetAsync_WithReviews()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        await using var resources = new TestResourceContainer(client);

        var testrepo = "users/foo/repo1";
        var changesets = (await client.GetChangesetsAsync(new(testrepo))).result.changesets;

        var reqid = "abcd";
        var response = await client.GetChangesetAsync(new(testrepo, changesets[0].summary.raw_id, with_reviews: true), id: reqid);
        response.id.Should().Be(reqid);
        response.result.reviews.Should()
            .ContainEquivalentOf(new Status(status: "under_review", modified_at: "", reviewer: "foo"), c => c.Excluding(s => s.modified_at));
    }

    [TestMethod()]
    public async Task GetPullRequestAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var pullrequest_id = "1";

        var response = await client.GetPullRequestAsync(new(pullrequest_id), id: reqid);
        response.id.Should().Be(reqid);
        response.result.pullrequest.pull_request_id.Should().Be(1);
        response.result.pullrequest.url.Should().NotBeNullOrWhiteSpace();
        response.result.pullrequest.revisions.Should().NotBeEmpty();
        response.result.pullrequest.owner.Should().Be("admin");
        response.result.pullrequest.title.Should().Be("pr-title");
        response.result.pullrequest.description.Should().Be("pr-desc");
        response.result.pullrequest.org_repo_url.Should().EndWith("users/bar/fork-foo-repo1");
        response.result.pullrequest.org_ref_parts.Should().NotBeEmpty();
        response.result.pullrequest.status.Should().Be("new");
        response.result.pullrequest.comments.Should().NotBeEmpty();
        response.result.pullrequest.statuses.Should().NotBeEmpty();
        response.result.pullrequest.created_on.Should().NotBeNullOrWhiteSpace();
        response.result.pullrequest.updated_on.Should().NotBeNullOrWhiteSpace();
    }

    [TestMethod()]
    public async Task CommentPullRequestAsync_NoStatus()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var pullrequest_id = "1";

        var pr_before = (await client.GetPullRequestAsync(new(pullrequest_id), id: reqid)).result.pullrequest;

        var response = await client.CommentPullRequestAsync(new(pullrequest_id, comment_msg: "poepoe"), id: reqid);
        response.id.Should().Be(reqid);

        var pr_after = (await client.GetPullRequestAsync(new(pullrequest_id), id: reqid)).result.pullrequest;
        pr_after.comments.Length.Should().BeGreaterThan(pr_before.comments.Length);
        pr_after.comments.ExceptBy(pr_before.comments.Select(c => c.comment_id), c => c.comment_id)
            .Should().AllSatisfy(c =>
            {
                c.text.Should().Be("poepoe");
                c.username.Should().Be("admin");
            });
        pr_after.statuses.Should().BeEquivalentTo(pr_before.statuses);
    }

    [TestMethod()]
    public async Task CommentPullRequestAsync_SetStatus()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var pullrequest_id = "1";
        var pr_status = PullRequestStatus.approved;

        var pr_before = (await client.GetPullRequestAsync(new(pullrequest_id), id: reqid)).result.pullrequest;

        var response = await client.CommentPullRequestAsync(new(pullrequest_id, comment_msg: "boeee", pr_status), id: reqid);
        response.id.Should().Be(reqid);

        var pr_after = (await client.GetPullRequestAsync(new(pullrequest_id), id: reqid)).result.pullrequest;
        pr_after.comments.Length.Should().BeGreaterThan(pr_before.comments.Length);
        pr_after.comments.ExceptBy(pr_before.comments.Select(c => c.comment_id), c => c.comment_id)
            .Should().AllSatisfy(c =>
            {
                c.text.Should().Be("boeee");
                c.username.Should().Be("admin");
            });
        pr_after.statuses.Length.Should().BeGreaterThan(pr_before.statuses.Length);
        pr_after.statuses.ExceptBy(pr_before.statuses.Select(s => s.modified_at), c => c.modified_at)
            .Should().AllSatisfy(s =>
            {
                s.status.Should().Be($"{pr_status}");
            });
    }

    [TestMethod()]
    public async Task EditPullRequestReviewersAsync()
    {
        using var client = new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client);

        var reqid = "abcd";
        var pullrequest_id = "1";

        var pr_before = (await client.GetPullRequestAsync(new(pullrequest_id), id: reqid)).result.pullrequest;

        var response = await client.EditPullRequestReviewersAsync(new(pullrequest_id, add: new[] { "foo", "bar", }), id: reqid);
        response.id.Should().Be(reqid);

        var reviewers1 = response.result;
        reviewers1.added.Should().BeEquivalentTo("foo", "bar");
        reviewers1.already_present.Should().BeEmpty();
        reviewers1.removed.Should().BeEmpty();

        var reviewers2 = (await client.EditPullRequestReviewersAsync(new(pullrequest_id, add: new[] { "bar", }, remove: new[] { "foo", }))).result;
        reviewers2.added.Should().BeEmpty();
        reviewers2.already_present.Should().BeEquivalentTo("bar");
        reviewers2.removed.Should().BeEquivalentTo("foo");

        var reviewers3 = (await client.EditPullRequestReviewersAsync(new(pullrequest_id, remove: new[] { "bar", }))).result;
        reviewers3.added.Should().BeEmpty();
        reviewers3.already_present.Should().BeEmpty();
        reviewers3.removed.Should().BeEquivalentTo("bar");

        var pr_after = (await client.GetPullRequestAsync(new(pullrequest_id), id: reqid)).result.pullrequest;
        pr_before.reviewers.Should().BeEmpty();
    }

}