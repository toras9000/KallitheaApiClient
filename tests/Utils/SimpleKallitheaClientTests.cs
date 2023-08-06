using FluentAssertions;

namespace KallitheaApiClient.Utils.Tests;

[TestClass()]
public class SimpleKallitheaClientTests
{
    public Uri ApiEntry { get; } = new Uri(@"http://localhost:9999/_admin/api");
    public string ApiKey { get; } = "1111222233334444555566667777888899990000";
    public HttpClient Client { get; } = new HttpClient();

    [TestMethod()]
    public async Task SomeMethods()
    {
        using var client = new SimpleKallitheaClient(new KallitheaClient(this.ApiEntry, this.ApiKey, () => this.Client));

        var reponame = $"share/test-{DateTime.Now:yyyyMMdd-HHmmss}";

        await client.CreateRepoAsync(new(reponame, description: "test-repo"));

        var repo = await client.GetRepoAsync(new(reponame));

        var updated = await client.UpdateRepoAsync(new(reponame, description: "updated-desc"));

        updated.description.Should().Be("updated-desc");

        await client.DeleteRepoAsync(new(reponame));
    }

}
