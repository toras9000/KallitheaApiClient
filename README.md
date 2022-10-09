# KallitheaApiClient

[![NugetShield]][NugetPackage]

[NugetPackage]: https://www.nuget.org/packages/KallitheaApiClient
[NugetShield]: https://img.shields.io/nuget/v/KallitheaApiClient

This is [Kallithea](https://kallithea-scm.org/) API client library for .NET.  
Kallithea is a self-hosted repository hosting WEB application.  

It's a relatively simple mapper to the API's JSON interface, so it should be easy to match with the Kallithea API documentation.  
Sorry, Intellisense messages are provided in Japanese. This is because I currently think that the main users are me and the people around me.  

The first three parts of the package version represent the target version of Kallithea.  
Be aware that if the versions do not match, the API specifications may not match.  


Example:
```csharp
using var client = new KallitheaClient(new("http://<your-hosting-server>/_admin/api"));
client.ApiKey = "<your-api-key>";

await client.CreateUserAsync(new("foo", "foo@example.com", "foo", "foo", "foo123"));

await client.CreateUserGroupAsync(new("all"));
await client.AddUserToUserGroupAsync(new("all", "admin"));
await client.AddUserToUserGroupAsync(new("all", "foo"));

await client.CreateUserGroupAsync(new("tester"));
await client.AddUserToUserGroupAsync(new("tester", "foo"));

await client.CreateRepoGroupAsync(new("share"));
await client.GrantUserGroupPermToRepoGroupAsync(new("share", "tester", "group.admin"));

await client.CreateRepoGroupAsync(new("users"));
await client.CreateRepoGroupAsync(new("foo", parent: "users"));
await client.GrantUserPermToRepoGroupAsync(new("users/foo", "foo", "group.admin"));
await client.CreateRepoAsync(new("users/foo/repo1", owner: "foo", repo_type: "git"));
await client.CreateRepoAsync(new("users/foo/repo2", owner: "foo", repo_type: "hg"));
```
