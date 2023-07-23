using KallitheaApiClient;

namespace KallitheaApiClientTest._Test;

public class TestResourceContainer : IAsyncDisposable
{
    public TestResourceContainer(KallitheaClient client)
    {
        this.resources = new List<TestResource>();
        this.users = new List<UserInfo>();
        this.usergroups = new List<UserGroupInfo>();
        this.repos = new List<RepoInfo>();
        this.repogroups = new List<RepoGroupInfo>();
        this.gists = new List<GistInfo>();

        this.Client = client;
        this.Users = this.users.AsReadOnly();
        this.UserGroups = this.usergroups.AsReadOnly();
        this.Repos = this.repos.AsReadOnly();
        this.RepoGroups = this.repogroups.AsReadOnly();
        this.Gists = this.gists.AsReadOnly();
    }

    public KallitheaClient Client { get; }
    public IReadOnlyList<UserInfo> Users { get; }
    public IReadOnlyList<UserGroupInfo> UserGroups { get; }
    public IReadOnlyList<RepoInfo> Repos { get; }
    public IReadOnlyList<RepoGroupInfo> RepoGroups { get; }
    public IReadOnlyList<GistInfo> Gists { get; }

    public UserInfo ToBeDiscarded(UserInfo user)
    {
        var resource = new TestResource(() => this.Client.DeleteUserAsync(new($"{user.user_id}")));
        this.resources.Add(resource);
        return this.AddTo(user);
    }

    public UserGroupInfo ToBeDiscarded(UserGroupInfo usergroup)
    {
        var resource = new TestResource(() => this.Client.DeleteUserGroupAsync(new($"{usergroup.users_group_id}")));
        this.resources.Add(resource);
        return this.AddTo(usergroup);
    }

    public RepoInfo ToBeDiscarded(RepoInfo repo)
    {
        var resource = new TestResource(() => this.Client.DeleteRepoAsync(new($"{repo.repo_id}")));
        this.resources.Add(resource);
        return this.AddTo(repo);
    }

    public RepoGroupInfo ToBeDiscarded(RepoGroupInfo repogroup)
    {
        var resource = new TestResource(() => this.Client.DeleteRepoGroupAsync(new($"{repogroup.group_id}")));
        this.resources.Add(resource);
        return this.AddTo(repogroup);
    }

    public GistInfo ToBeDiscarded(GistInfo gist)
    {
        var resource = new TestResource(() => this.Client.DeleteGistAsync(new($"{gist.gist_id}")));
        this.resources.Add(resource);
        return this.AddTo(gist);
    }

    public UserInfo AddTo(UserInfo user)
    {
        this.users.Add(user);
        return user;
    }

    public UserGroupInfo AddTo(UserGroupInfo usergroup)
    {
        this.usergroups.Add(usergroup);
        return usergroup;
    }

    public RepoInfo AddTo(RepoInfo repo)
    {
        this.repos.Add(repo);
        return repo;
    }

    public RepoGroupInfo AddTo(RepoGroupInfo repogroup)
    {
        this.repogroups.Add(repogroup);
        return repogroup;
    }

    public GistInfo AddTo(GistInfo gist)
    {
        this.gists.Add(gist);
        return gist;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var res in this.resources.AsEnumerable().Reverse())
        {
            try { await res.DisposeAsync().ConfigureAwait(false); } catch { }
        }
        this.users.Clear();
    }

    private class TestResource : IAsyncDisposable
    {
        public TestResource(Func<Task> disposer)
        {
            this.disposer = disposer;
        }

        public async ValueTask DisposeAsync()
        {
            await this.disposer().ConfigureAwait(false);
        }

        private Func<Task> disposer;
    }

    private List<TestResource> resources;
    private List<UserInfo> users;
    private List<UserGroupInfo> usergroups;
    private List<RepoInfo> repos;
    private List<RepoGroupInfo> repogroups;
    private List<GistInfo> gists;
}

public static partial class TestResourceContainerExtensions
{
    public static async Task<ApiResponse<CreateUserResult>> WillBeDiscarded(this Task<ApiResponse<CreateUserResult>> self, TestResourceContainer container)
    {
        var response = await self.ConfigureAwait(false);
        container.ToBeDiscarded(response.result.user);
        return response;
    }

    public static async Task<ApiResponse<CreateUserGroupResult>> WillBeDiscarded(this Task<ApiResponse<CreateUserGroupResult>> self, TestResourceContainer container)
    {
        var response = await self.ConfigureAwait(false);
        container.ToBeDiscarded(response.result.user_group);
        return response;
    }

    public static async Task<ApiResponse<GetRepoResult>> WillBeDiscarded(this Task<ApiResponse<GetRepoResult>> self, TestResourceContainer container)
    {
        var response = await self.ConfigureAwait(false);
        container.ToBeDiscarded(response.result.repo);
        return response;
    }

    public static async Task<ApiResponse<CreateRepoGroupResult>> WillBeDiscarded(this Task<ApiResponse<CreateRepoGroupResult>> self, TestResourceContainer container)
    {
        var response = await self.ConfigureAwait(false);
        container.ToBeDiscarded(response.result.repo_group);
        return response;
    }

    public static async Task<ApiResponse<CreateGistResult>> WillBeDiscarded(this Task<ApiResponse<CreateGistResult>> self, TestResourceContainer container)
    {
        var response = await self.ConfigureAwait(false);
        container.ToBeDiscarded(response.result.gist);
        return response;
    }
}

public static partial class TestResourceContainerExtensions
{
    public static async Task<UserInfo> CreateTestUserAsync(this TestResourceContainer self, CreateUserArgs args)
    {
        var response = await self.Client.CreateUserAsync(args).WillBeDiscarded(self);
        return response.result.user;
    }

    public static async Task<UserGroupInfo> CreateTestUserGroupAsync(this TestResourceContainer self, CreateUserGroupArgs args)
    {
        var response = await self.Client.CreateUserGroupAsync(args).WillBeDiscarded(self);
        return response.result.user_group;
    }

    public static async Task<RepoInfo> CreateTestRepoAsync(this TestResourceContainer self, CreateRepoArgs args)
    {
        await self.Client.CreateRepoAsync(args);
        try
        {
            var response = await self.Client.GetRepoAsync(new(args.repo_name)).WillBeDiscarded(self);
            return response.result.repo;
        }
        catch
        {
            try { await self.Client.DeleteRepoAsync(new(args.repo_name)); } catch { }
            throw;
        }
    }

    public static async Task<RepoInfo> ForkTestRepoAsync(this TestResourceContainer self, ForkRepoArgs args)
    {
        await self.Client.ForkRepoAsync(args);
        try
        {
            var repo = await self.Client.GetRepoAsync(new(args.fork_name)).WillBeDiscarded(self);
            return repo.result.repo;
        }
        catch
        {
            try { await self.Client.DeleteRepoAsync(new(args.fork_name)); } catch { }
            throw;
        }
    }

    public static async Task<RepoGroupInfo> CreateTestRepoGroupAsync(this TestResourceContainer self, CreateRepoGroupArgs args)
    {
        var response = await self.Client.CreateRepoGroupAsync(args).WillBeDiscarded(self);
        return response.result.repo_group;
    }

    public static async Task<GistInfo> CreateTestGistAsync(this TestResourceContainer self, CreateGistArgs args)
    {
        var response = await self.Client.CreateGistAsync(args).WillBeDiscarded(self);
        return response.result.gist;
    }
}
