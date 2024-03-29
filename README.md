# KallitheaApiClient

[![NugetShield]][NugetPackage]

[NugetPackage]: https://www.nuget.org/packages/KallitheaApiClient
[NugetShield]: https://img.shields.io/nuget/v/KallitheaApiClient

This is [Kallithea](https://kallithea-scm.org/) API client library for .NET. (unofficial)  
Kallithea is a self-hosted repository hosting WEB application.  

It's a relatively simple mapper to the API's JSON interface, so it should be easy to match with the Kallithea [API documentation](https://kallithea.readthedocs.io/en/latest/api/api.html).  
Sorry, IntelliSense messages (documentation comments) for types and members are provided in Japanese. This is because I currently think that the main users are me and the people around me.  

## Package and API version 

Although the Kallithea API specification may change from version to version, this library targets only a single version.  
If the version targeted by the library does not match the server version, there is a large possibility that it will not work properly.  

Package versions are in semantic versioning format, but are numbered according to the following arrangement.  
Always used the pre-release versioning for this package version number.  
The core version part represents the version of the target server.  
The pre-release version part is used to represent the library version, not as a pre-release.  
The first of the pre-release version numbers is changed when the library specification changes (binary incompatibility).  
The second pre-release version number is changed for bug fixes and other cases where binary compatibility is maintained.  

## Examples

Some samples are shown below.  
These use C#9 or later syntax.  

### Creation of users, repositories, etc.
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

### Displays the most recent changesets for each repository.
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
        var args = new GetChangesetsArgs(repo.repo_name, max_revisions: "3", reverse: true);
        var changesets = await client.GetChangesetsAsync(args);
        foreach (var change in changesets)
        {
            var msgline = change.summary.message?.Split('\r', '\n')[0].Trim();
            Console.WriteLine($"  {change.summary.short_id} : {msgline}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Can't get changeset : {ex.Message}");
    }
}
```
