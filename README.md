# KallitheaApiClient

[![NugetShield]][NugetPackage]

[NugetPackage]: https://www.nuget.org/packages/KallitheaApiClient
[NugetShield]: https://img.shields.io/nuget/v/KallitheaApiClient

This is [Kallithea](https://kallithea-scm.org/) API client library for .NET.  
Kallithea is a self-hosted repository hosting WEB application.  

It's a relatively simple mapper to the API's JSON interface, so it should be easy to match with the Kallithea [API documentation](https://kallithea.readthedocs.io/en/latest/api/api.html).  
Sorry, IntelliSense messages (documentation comments) for types and members are provided in Japanese. This is because I currently think that the main users are me and the people around me.  

The first three parts of the package version represent the target version of Kallithea.  
Be aware that if the versions do not match, the API specifications may not match.  

The fourth revision value shows the version value of this library.  
Therefore, unlike general library versioning, the difference in revision values is not necessarily a trivial change.  


Example:
```csharp
var apiUrl = new Uri("http://<your-hosting-server>/_admin/api");
var apiKey = "<your-api-key>";
using var client = new KallitheaClient(apiUrl, apiKey);

await client.CreateUserAsync(new("foo", "foo@example.com", "family", "given", password: "foo123"));

await client.CreateUserGroupAsync(new("all"));
await client.AddUserToUserGroupAsync(new("all", "admin"));
await client.AddUserToUserGroupAsync(new("all", "foo"));

await client.CreateUserGroupAsync(new("tester"));
await client.AddUserToUserGroupAsync(new("tester", "foo"));

await client.CreateRepoGroupAsync(new("share"));
await client.GrantUserGroupPermToRepoGroupAsync(new("share", "tester", RepoGroupPerm.admin));

await client.CreateRepoGroupAsync(new("users"));
await client.CreateRepoGroupAsync(new("foo", parent: "users"));
await client.GrantUserPermToRepoGroupAsync(new("users/foo", "foo", RepoGroupPerm.admin));

await client.CreateRepoAsync(new("users/foo/repo1", owner: "foo", repo_type: RepoType.git));
await client.CreateRepoAsync(new("users/foo/repo2", owner: "foo", repo_type: RepoType.hg));
```

Example:
```csharp
var apiUrl = new Uri("http://<your-hosting-server>/_admin/api");
var apiKey = "<your-api-key>";
using var client = new SimpleKallitheaClient(apiUrl, apiKey);

var repositories = await client.GetReposAsync();
foreach (var repo in repositories)
{
    Console.WriteLine($"{repo.repo_name}: RepoType={repo.repo_type}, Owner={repo.owner}");
    try
    {
        var changesets = await client.GetChangesetsAsync(new(repo.repo_id.ToString(), max_revisions: "3", reverse: true));
        foreach (var change in changesets)
        {
            Console.WriteLine($"  {change.summary.short_id} : {change.summary.message?.Trim()}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Can't get changeset : {ex.Message}");
    }
}
```
