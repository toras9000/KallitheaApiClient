// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

#r "nuget: System.Data.SQLite.Core, 1.0.118"
#r "nuget: Dapper, 2.0.123"
#r "nuget: LibGit2Sharp, 0.27.2"
#r "nuget: AngleSharp, 1.0.4"
#r "nuget: KallitheaApiClient, 0.7.0.13"
#r "nuget: Lestaly, 0.42.0"
using System.Data.SQLite;
using AngleSharp;
using AngleSharp.Html.Dom;
using Dapper;
using KallitheaApiClient;
using Lestaly;
using LibGit2Sharp;

await Paved.RunAsync(async () =>
{
    // API key to set up.
    var apiKey = "1111222233334444555566667777888899990000";

    // Connection settings for kallithea db.
    var db_settings = new SQLiteConnectionStringBuilder();
    db_settings.DataSource = ThisSource.RelativeFile("./config/kallithea.db").FullName;
    db_settings.FailIfMissing = true;

    // Connect to db and state check.
    Console.WriteLine("Check database status.");
    using (var db = new SQLiteConnection(db_settings.ConnectionString))
    {
        await db.OpenAsync();
        var users = await db.QueryAsync<string>("select username from users");
        var repos = await db.QueryAsync<string>("select repo_name from repositories");
        var repogrps = await db.QueryAsync<string>("select users_group_name from users_groups");
        if (users.Except(new[] { "admin", "default", }).Count() != 0 || repos.Count() != 0 || repogrps.Count() != 0)
        {
            throw new PavedMessageException("Processing is canceled because it is not in the initial state.");
        }

        // Force update of admin's API key. 
        Console.WriteLine("Rewrite the API key for test.");
        await db.ExecuteAsync("update users set api_key = @key where username = 'admin'", new { key = apiKey, });
    }

    // Perform initial settings using API.
    Console.WriteLine("Set up entities for testing.");
    var serviceBase = new Uri("http://localhost:9999");
    using var client = new KallitheaClient(new(serviceBase, "/_admin/api"));
    client.ApiKey = apiKey;

    await client.CreateUserAsync(new("foo", "foo@example.com", "foo", "foo", password: "foo123", extern_type: "internal"));
    await client.CreateUserAsync(new("bar", "bar@example.com", "bar", "bar", password: "bar123", extern_type: "internal"));

    await client.CreateUserGroupAsync(new("all"));
    await client.AddUserToUserGroupAsync(new("all", "admin"));
    await client.AddUserToUserGroupAsync(new("all", "foo"));
    await client.AddUserToUserGroupAsync(new("all", "bar"));

    await client.CreateUserGroupAsync(new("tester"));
    await client.AddUserToUserGroupAsync(new("tester", "foo"));
    await client.AddUserToUserGroupAsync(new("tester", "bar"));

    await client.CreateRepoGroupAsync(new("share"));
    await client.GrantUserGroupPermToRepoGroupAsync(new("share", "tester", RepoGroupPerm.admin));

    await client.CreateRepoGroupAsync(new("users"));
    await client.CreateRepoGroupAsync(new("foo", parent: "users", owner: "foo"));
    await client.GrantUserPermToRepoGroupAsync(new("users/foo", "foo", RepoGroupPerm.admin));
    await client.CreateRepoGroupAsync(new("bar", parent: "users", owner: "bar"));
    await client.GrantUserPermToRepoGroupAsync(new("users/bar", "bar", RepoGroupPerm.admin));

    await client.CreateRepoAsync(new("users/foo/repo1", owner: "foo", repo_type: RepoType.git));
    await client.CreateRepoAsync(new("users/foo/repo2", owner: "foo", repo_type: RepoType.hg));
    await client.CreateRepoAsync(new("users/foo/repo3", owner: "foo", repo_type: RepoType.git));
    await client.CreateRepoAsync(new("users/foo/clone1", owner: "foo", repo_type: RepoType.git, clone_uri: "https://github.com/toras9000/KallitheaApiClient.git"));

    await client.CreateRepoAsync(new("users/bar/repo1", owner: "bar", repo_type: RepoType.git));
    await client.CreateRepoAsync(new("users/bar/repo2", owner: "bar", repo_type: RepoType.hg));

    await client.GrantUserPermToRepoAsync(new("users/foo/repo1", "bar", RepoPerm.write));
    await client.GrantUserPermToRepoAsync(new("users/foo/repo2", "bar", RepoPerm.admin));
    await client.GrantUserGroupPermToRepoAsync(new("users/foo/repo1", "tester", RepoPerm.read));
    await client.GrantUserPermToRepoGroupAsync(new("users/foo", "bar", RepoGroupPerm.write));
    await client.GrantUserGroupPermToRepoGroupAsync(new("users/foo", "tester", RepoGroupPerm.read));

    await client.GrantUserPermToRepoAsync(new("users/bar/repo1", "foo", RepoPerm.write));
    await client.GrantUserPermToRepoAsync(new("users/bar/repo2", "foo", RepoPerm.admin));
    await client.GrantUserGroupPermToRepoAsync(new("users/bar/repo1", "tester", RepoPerm.read));
    await client.GrantUserPermToRepoGroupAsync(new("users/bar", "foo", RepoGroupPerm.write));
    await client.GrantUserGroupPermToRepoGroupAsync(new("users/bar", "tester", RepoGroupPerm.read));

    makeDummyCommits(ThisSource.RelativeDirectory("./repos/users/foo/repo1"), context =>
    {
        var author = new Author("foo", "foo@example.com");
        var commit1 = context.AddCommit("commit 1", author, new FileBlob[] { new("aaa.txt", "aaa"), new("bbb.txt", "bbb"), });
        var commit2 = context.AddCommit("commit 2", author, new FileBlob[] { new("aaa.txt", "aAa"), new("ccc.txt", "ccc"), });
        var commit3 = context.AddCommit("commit 3", author, new FileBlob[] { new("bbb.txt", null), new("ddd.txt", "ddd"), });
        context.AddBranch(commit1, "br1");
        context.AddBranch(commit2, "br2");
        context.AddTag(commit1.Sha, "tag1");
        context.AddTag(commit2.Sha, "tag2");
    });
    await client.InvalidateCacheAsync(new("users/foo/repo1"));

    makeDummyCommits(ThisSource.RelativeDirectory("./repos/users/foo/repo3"), context =>
    {
        var author = new Author("foo", "foo@example.com");
        var commit1 = context.AddCommit("commit 1", author, new FileBlob[] { new("xxx/a.txt", "xA"), new("a.txt", "A"), new("b.txt", "B") });
        var commit2 = context.AddCommit("commit 2", author, new FileBlob[] { new("b.txt", null) });
        var commit3 = context.AddCommit("commit 3", author, new FileBlob[] { new("xxx/b.txt", "xB"), new("xxx/c.txt", "xC"), new("yyy/a.txt", "yA"), new("c.txt", "C") });
    });
    await client.InvalidateCacheAsync(new("users/foo/repo1"));

    makeDummyCommits(ThisSource.RelativeDirectory("./repos/users/bar/repo1"), context =>
    {
        var author = new Author("bar", "bar@example.com");
        var commit1 = context.AddCommit("commit 1", author, new FileBlob[] { new("aaa.txt", "aaa"), new("bbb.txt", "bbb"), });
        var commit2 = context.AddCommit("commit 2", author, new FileBlob[] { new("aaa.txt", "aAa"), new("ccc.txt", "ccc"), });
        var commit3 = context.AddCommit("commit 3", author, new FileBlob[] { new("bbb.txt", "BBB"), new("ddd.txt", "ddd"), });
        context.AddBranch(commit1, "br1");
        context.AddBranch(commit2, "br2");
        context.AddTag(commit1.Sha, "tag1");
        context.AddTag(commit2.Sha, "tag2");
    });
    await client.InvalidateCacheAsync(new("users/bar/repo1"));

    await client.ForkRepoAsync(new("users/bar/repo1", "users/foo/fork-bar-repo1", "foo"));
    await client.ForkRepoAsync(new("users/foo/repo1", "users/bar/fork-foo-repo1", "bar"));
    await client.ForkRepoAsync(new("users/foo/clone1", "users/bar/fork-foo-clone1", "bar"));

    await client.GrantUserPermToRepoAsync(new("users/foo/fork-bar-repo1", "bar", RepoPerm.write));
    await client.GrantUserPermToRepoAsync(new("users/bar/fork-foo-repo1", "foo", RepoPerm.admin));
    await client.GrantUserPermToRepoAsync(new("users/bar/fork-foo-clone1", "foo", RepoPerm.admin));
    await client.GrantUserGroupPermToRepoAsync(new("users/bar/fork-foo-repo1", "tester", RepoPerm.write));
    await client.GrantUserGroupPermToRepoAsync(new("users/bar/fork-foo-clone1", "tester", RepoPerm.write));

    makeDummyCommits(ThisSource.RelativeDirectory("./repos/users/bar/fork-foo-repo1"), context =>
    {
        var author = new Author("bar", "bar@example.com");
        var commit1 = context.AddCommit("commit 4", author, new FileBlob[] { new("aaa.txt", null), new("ddd.txt", "ddd"), });
        var commit2 = context.AddCommit("commit 5", author, new FileBlob[] { new("ddd.txt", "DDD"), new("bbb.txt", null), });
        var commit3 = context.AddCommit("commit 6", author, new FileBlob[] { new("eee.txt", "eee"), new("fff.txt", "fff"), });
    });
    await client.InvalidateCacheAsync(new("users/bar/fork-foo-repo1"));

    await createPullRequestAsync(serviceBase, "users/bar/fork-foo-repo1", apiKey);

    Console.WriteLine("Setup completed.");

}, o => o.AnyPause());

record Author(string name, string addr);
record FileBlob(string name, string content);
delegate Commit Committer(string message, Author author, FileBlob[] files, Commit parent = null);
delegate void Tagger(string target, string name);
delegate void Brancher(Commit target, string name);
record RepoContext(Committer AddCommit, Brancher AddBranch, Tagger AddTag);

void makeDummyCommits(DirectoryInfo repoDir, Action<RepoContext> adapter)
{
    using var repo = new Repository(repoDir.FullName);

    Commit addCommit(Commit parent, string message, Author author, FileBlob[] files)
    {
        var treeDef = parent == null ? new TreeDefinition() : TreeDefinition.From(parent);

        foreach (var file in files)
        {
            if (file.content == null)
            {
                treeDef.Remove(file.name);
            }
            else
            {
                var content = new MemoryStream(Encoding.UTF8.GetBytes(file.content));
                var blob = repo.ObjectDatabase.CreateBlob(content);
                treeDef.Add(file.name, blob, Mode.NonExecutableFile);
            }
        }

        var tree = repo.ObjectDatabase.CreateTree(treeDef);
        var committer = new Signature(author.name, author.addr, DateTime.Now);
        var parents = parent == null ? Array.Empty<Commit>() : new[] { parent, };
        var commit = repo.ObjectDatabase.CreateCommit(committer, committer, message, tree, parents, prettifyMessage: false);
        repo.Refs.UpdateTarget(repo.Refs.Head, commit.Id);
        return commit;
    }

    var main = repo.Branches["main"];
    var tip = repo.Head.Tip == main?.Tip;
    var last = default(Commit);

    var context = new RepoContext(
        (message, author, files, parent) => last = addCommit(parent ?? repo.Head.Tip, message, author, files),
        (target, name) => repo.CreateBranch(name, target),
        (target, name) => repo.ApplyTag(name, target)
    );
    adapter(context);

    if (tip && last != null)
    {
        if (main == null) repo.Branches.Add("main", last);
        else repo.Refs.UpdateTarget(main.Reference, last.Id);
    }
}

async Task createPullRequestAsync(Uri serviceBase, string repoPath, string apiKey)
{
    var reqAddr = new Uri(serviceBase, $"/{repoPath}/pull-request/new?api_key={apiKey}");
    var config = AngleSharp.Configuration.Default.WithDefaultLoader();
    using var context = BrowsingContext.New(config);
    var document = await context.OpenAsync(reqAddr.AbsoluteUri);
    var orgSelector = document.GetElementById("org_repo") as IHtmlSelectElement;
    var orgRef = document.GetElementById("org_ref") as IHtmlSelectElement;
    var otherSelector = document.GetElementById("other_repo") as IHtmlSelectElement;
    var otherRef = document.GetElementById("other_ref") as IHtmlSelectElement;

    var orgMain = orgRef.Options.First(o => o.Label == "main");
    var otherMain = otherRef.Options.First(o => o.Label == "main");

    var form = document.Forms["pull_request_form"];
    form.Action += $"?api_key={apiKey}";
    var result = await form.SubmitAsync(createMissing: true, fields: new Dictionary<string, string>
    {
        ["api_key"] = apiKey,
        ["pullrequest_title"] = "pr-title",
        ["pullrequest_desc"] = "pr-desc",
        ["org_ref"] = orgMain.Value,
        ["other_ref"] = otherMain.Value,
    });
}