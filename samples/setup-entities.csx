#r "nuget: KallitheaApiClient, 0.7.0.1"
#r "nuget: Lestaly, 0.9.0"

// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

using KallitheaApiClient;
using Lestaly;

await Paved.RunAsync(async () =>
{
    using var client = new KallitheaClient(new("http://localhost:9999/_admin/api"));
    client.ApiKey = "4441959e3f5c2a0ea009428382e242124dc774e6";

    await client.CreateUserAsync(new("foo", "foo@example.com", "foo", "foo", "foo123"));
    await client.CreateUserAsync(new("bar", "bar@example.com", "bar", "bar", "bar123"));

    await client.CreateUserGroupAsync(new("all"));
    await client.AddUserToUserGroupAsync(new("all", "admin"));
    await client.AddUserToUserGroupAsync(new("all", "foo"));
    await client.AddUserToUserGroupAsync(new("all", "bar"));

    await client.CreateUserGroupAsync(new("tester"));
    await client.AddUserToUserGroupAsync(new("tester", "foo"));
    await client.AddUserToUserGroupAsync(new("tester", "bar"));

    await client.CreateRepoGroupAsync(new("share"));
    await client.GrantUserGroupPermToRepoGroupAsync(new("share", "tester", "group.admin"));

    await client.CreateRepoGroupAsync(new("users"));
    await client.CreateRepoGroupAsync(new("foo", parent: "users"));
    await client.GrantUserPermToRepoGroupAsync(new("users/foo", "foo", "group.admin"));
    await client.CreateRepoGroupAsync(new("bar", parent: "users"));
    await client.GrantUserPermToRepoGroupAsync(new("users/bar", "bar", "group.admin"));

    await client.CreateRepoAsync(new("users/foo/repo1", owner: "foo", repo_type: "git"));
    await client.CreateRepoAsync(new("users/foo/repo2", owner: "foo", repo_type: "hg"));

    await client.CreateRepoAsync(new("users/bar/repo1", owner: "bar", repo_type: "git"));
    await client.CreateRepoAsync(new("users/bar/repo2", owner: "bar", repo_type: "hg"));

    Console.WriteLine("Setup completed.");

}, o => o.AnyPause());
