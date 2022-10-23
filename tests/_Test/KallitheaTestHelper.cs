using KallitheaApiClient;

namespace KallitheaApiClientTest._Test;

public interface ITestAsyncDisposable<TEntity> : IAsyncDisposable
{
    TEntity Entity { get; }
}

public static class KallitheaTestHelper
{
    public static async Task<ITestAsyncDisposable<UserInfo>> CreateTestUserAsync(this KallitheaClient self, CreateUserArgs args)
    {
        var response = await self.CreateUserAsync(args).ConfigureAwait(false);
        return new TestPeriod<UserInfo>(response.result.user, async () => await self.DeleteUserAsync(new(response.result.user.user_id.ToString())));
    }

    public static async Task<ITestAsyncDisposable<UserGroupInfo>> CreateTestUserGroupAsync(this KallitheaClient self, CreateUserGroupArgs args)
    {
        var response = await self.CreateUserGroupAsync(args).ConfigureAwait(false);
        return new TestPeriod<UserGroupInfo>(response.result.user_group, async () => await self.DeleteUserGroupAsync(new(response.result.user_group.users_group_id.ToString())));
    }

    public static async Task<ITestAsyncDisposable<RepoInfo>> CreateTestRepoAsync(this KallitheaClient self, CreateRepoArgs args)
    {
        var rspCreate = await self.CreateRepoAsync(args).ConfigureAwait(false);
        var rspRepo = await self.GetRepoAsync(new(args.repo_name)).ConfigureAwait(false);
        return new TestPeriod<RepoInfo>(rspRepo.result.repo, async () => await self.DeleteRepoAsync(new(rspRepo.result.repo.repo_id.ToString())));
    }

    public static async Task<ITestAsyncDisposable<RepoGroupInfo>> CreateTestRepoGroupAsync(this KallitheaClient self, CreateRepoGroupArgs args)
    {
        var response = await self.CreateRepoGroupAsync(args).ConfigureAwait(false);
        return new TestPeriod<RepoGroupInfo>(response.result.repo_group, async () => await self.DeleteRepoGroupAsync(new(response.result.repo_group.group_id.ToString())));
    }

    public static async Task<ITestAsyncDisposable<GistInfo>> CreateTestGistAsync(this KallitheaClient self, CreateGistArgs args)
    {
        var response = await self.CreateGistAsync(args).ConfigureAwait(false);
        return new TestPeriod<GistInfo>(response.result.gist, async () => await self.DeleteRepoGroupAsync(new(response.result.gist.gist_id.ToString())));
    }

    private class TestPeriod<TEntity> : ITestAsyncDisposable<TEntity>
    {
        public TestPeriod(TEntity entity, Func<ValueTask> cleanup)
        {
            this.Entity = entity;
            this.cleanup = cleanup;
        }
        public TEntity Entity { get; }
        public async ValueTask DisposeAsync()
        {
            if (this.cleanup != null)
            {
                await this.cleanup();
                this.cleanup = null;
            }
        }
        private Func<ValueTask>? cleanup;
    }
}
