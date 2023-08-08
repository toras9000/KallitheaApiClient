#r "nuget: System.Data.SQLite.Core, 1.0.118"
#r "nuget: Dapper, 2.0.123"
#r "nuget: LibGit2Sharp, 0.27.2"
#r "nuget: AngleSharp, 1.0.4"
#r "nuget: KallitheaApiClient, 0.7.0.22"
#r "nuget: Lestaly, 0.43.0"
#nullable enable
using System.Data.SQLite;
using AngleSharp;
using AngleSharp.Html.Dom;
using Dapper;
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;
using LibGit2Sharp;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var dataDir = baseDir.RelativeDirectory("data");

    // API key to set up.
    var apiKey = "1111222233334444555566667777888899990000";

    // Connection settings for kallithea db.
    var db_settings = new SQLiteConnectionStringBuilder();
    db_settings.DataSource = dataDir.RelativeFile("./config/kallithea.db").FullName;
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

        // Force rewrite of admin's API key. 
        Console.WriteLine("Rewrite the API key for test.");
        await db.ExecuteAsync("update users set api_key = @key where username = 'admin'", new { key = apiKey, });
    }

    // Perform initial settings using API.
    Console.WriteLine("Set up entities for testing.");
    Console.WriteLine("...");
    var serviceBase = new Uri("http://localhost:9999");
    using var client = new SimpleKallitheaClient(new(serviceBase, "/_admin/api"));
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

    await client.CreateRepoGroupAsync(new("many"));
    await client.GrantUserGroupPermToRepoGroupAsync(new("share", "all", RepoGroupPerm.admin));
    for (var i = 0; i < 50; i++)
    {
        await client.CreateRepoAsync(new($"many/repo{i}", owner: "admin", repo_type: RepoType.git));
    }

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

    makeDummyCommits(dataDir.RelativeDirectory("./repos/users/foo/repo1"), context =>
    {
        var author = new Author("foo", "foo@example.com");
        var commit1 = context.AddCommit("commit 1", author, new FileBlob[] { new("aaa.txt", "aaa\naaa"), new("bbb.txt", "bbb\nbbb"), });
        var commit2 = context.AddCommit("commit 2", author, new FileBlob[] { new("aaa.txt", "aAa\nAaA"), new("ccc.txt", "ccc\nccc"), });
        var commit3 = context.AddCommit("commit 3", author, new FileBlob[] { new("bbb.txt", null), new("ddd.txt", "ddd\nddd"), });
        context.AddBranch(commit1, "br1");
        context.AddBranch(commit2, "br2");
        context.AddTag(commit1.Sha, "tag1");
        context.AddTag(commit2.Sha, "tag2");
    });
    using (var db = new SQLiteConnection(db_settings.ConnectionString))
    {
        var getuser = await client.GetUserAsync(new("foo"));
        var getrepo = await client.GetRepoAsync(new("users/foo/repo1"));
        var changesets = await client.GetChangesetsAsync(new($"{getrepo.repo.repo_id}"));
        var changeset = await client.GetChangesetAsync(new(getrepo.repo.repo_name, changesets[0].summary.raw_id));

        await db.OpenAsync();

        var commentParams = new
        {
            repo_id = getrepo.repo.repo_id,
            revision = changeset.summary.raw_id,
            line_no = default(string),
            f_path = default(string),
            user_id = getuser.user.user_id,
            text = default(string),
        };
        var commentSql = $"""
            insert into changeset_comments (repo_id, revision, line_no, f_path, user_id, text, created_on, modified_at)
            values (@{nameof(commentParams.repo_id)}, @{nameof(commentParams.revision)}, @{nameof(commentParams.line_no)}, @{nameof(commentParams.f_path)},
                    @{nameof(commentParams.user_id)}, @{nameof(commentParams.text)}, datetime('now', 'localtime'), datetime('now', 'localtime'))
        """;
        // commit comments
        await db.ExecuteAsync(
            sql: commentSql,
            param: commentParams with { line_no = null, f_path = null, text = $"test comment for {changeset.summary.message} (1)", }
        );
        await db.ExecuteAsync(
            sql: commentSql,
            param: commentParams with { line_no = null, f_path = null, text = $"test comment for {changeset.summary.message} (2)", }
        );
        var comment_id = await db.ExecuteScalarAsync(
            sql: @"select comment_id from changeset_comments where repo_id = @repo_id and user_id = @user_id and revision = @revision",
            param: commentParams
        );

        // inline comments
        var changefile = changeset.filelist.added.FirstOrDefault() ?? changeset.filelist.changed.FirstOrDefault();
        await db.ExecuteAsync(
            sql: commentSql,
            param: commentParams with { line_no = "n1", f_path = changefile, text = $"test comment for {changefile} line1", }
        );
        await db.ExecuteAsync(
            sql: commentSql,
            param: commentParams with { line_no = "n2", f_path = changefile, text = $"test comment for {changefile} line2", }
        );

        // reviews
        var reviewParams = new
        {
            repo_id = getrepo.repo.repo_id,
            user_id = getuser.user.user_id,
            revision = changeset.summary.raw_id,
            status = "under_review",
            comment_id = comment_id,
            version = 0,
            pull_request_id = default(int?),
        };
        var reviewSql = $"""
            insert into changeset_statuses(repo_id, user_id, revision, status, changeset_comment_id, version, pull_request_id, modified_at)
            values (@{nameof(reviewParams.repo_id)}, @{nameof(reviewParams.user_id)}, @{nameof(reviewParams.revision)},
                    @{nameof(reviewParams.status)}, @{nameof(reviewParams.comment_id)}, @{nameof(reviewParams.version)},
                    @{nameof(reviewParams.pull_request_id)}, datetime('now', 'localtime'))
        """;
        await db.ExecuteAsync(
            sql: reviewSql,
            param: reviewParams
        );
    }
    await client.InvalidateCacheAsync(new("users/foo/repo1"));

    makeDummyCommits(dataDir.RelativeDirectory("./repos/users/foo/repo3"), context =>
    {
        var author = new Author("foo", "foo@example.com");
        var commit1 = context.AddCommit("commit 1", author, new FileBlob[] { new("xxx/a.txt", "xA"), new("a.txt", "A"), new("b.txt", "B") });
        var commit2 = context.AddCommit("commit 2", author, new FileBlob[] { new("b.txt", null) });
        var commit3 = context.AddCommit("commit 3", author, new FileBlob[] { new("xxx/b.txt", "xB"), new("xxx/c.txt", "xC"), new("yyy/a.txt", "yA"), new("c.txt", "C") });
    });
    using (var db = new SQLiteConnection(db_settings.ConnectionString))
    {
        var getrepo = await client.GetRepoAsync(new("users/foo/repo3"));

        await db.OpenAsync();

        var field_setting = await db.ExecuteScalarAsync($"select app_settings_id from settings where app_settings_name = 'repository_fields'");
        if (field_setting == null)
        {
            await db.ExecuteAsync("insert into settings(app_settings_name, app_settings_value, app_settings_type) values ('repository_fields', 'True', 'bool')");
        }
        else
        {
            await db.ExecuteAsync($"update settings set app_settings_value = 'True' where app_settings_name = 'repository_fields'");
        }

        var fieldParams = new
        {
            repo_id = getrepo.repo.repo_id,
            key = default(string),
            label = default(string),
            desc = default(string),
            value = default(string),
        };
        var fieldSql = $"""
            insert into repositories_fields (repository_id, field_key, field_label, field_desc, field_value, field_type, created_on)
            values (@{nameof(fieldParams.repo_id)}, @{nameof(fieldParams.key)}, @{nameof(fieldParams.label)}, @{nameof(fieldParams.desc)},
                    @{nameof(fieldParams.value)}, 'str', datetime('now', 'localtime'))
        """;
        await db.ExecuteAsync(
            sql: fieldSql,
            param: fieldParams with { key = "testkey1", label = "testlabel1", desc = "testdesc1", value = "testvalue1", }
        );
        await db.ExecuteAsync(
            sql: fieldSql,
            param: fieldParams with { key = "testkey2", label = "testlabel2", desc = "testdesc2", value = "testvalue2", }
        );
    }
    await client.InvalidateCacheAsync(new("users/foo/repo3"));

    makeDummyCommits(dataDir.RelativeDirectory("./repos/users/bar/repo1"), context =>
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

    makeDummyCommits(dataDir.RelativeDirectory("./repos/users/bar/fork-foo-repo1"), context =>
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
record FileBlob(string name, string? content);
delegate Commit Committer(string message, Author author, FileBlob[] files, Commit? parent = null);
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
    var orgRef = document.GetElementById("org_ref") as IHtmlSelectElement ?? throw new PavedMessageException("Unexpected");
    var otherSelector = document.GetElementById("other_repo") as IHtmlSelectElement;
    var otherRef = document.GetElementById("other_ref") as IHtmlSelectElement ?? throw new PavedMessageException("Unexpected");

    var orgMain = orgRef.Options.First(o => o.Label == "main");
    var otherMain = otherRef.Options.First(o => o.Label == "main");

    var form = document.Forms["pull_request_form"] ?? throw new PavedMessageException("Unexpected");
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