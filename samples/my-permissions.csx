#r "nuget: KallitheaApiClient, 0.7.0.9"
#r "nuget: Lestaly, 0.13.0"

// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

using KallitheaApiClient;
using Lestaly;

await Paved.RunAsync(async () =>
{
    // Initialize the client
    var url = new Uri("http://localhost:9999/_admin/api");
    var key = "1111222233334444555566667777888899990000";
    using var client = new KallitheaClient(url, key);

    // Get user information.
    var user = (await client.GetUserAsync()).result;
    Console.WriteLine($"User: {user.user.username}");

    // Print a list of permissions to the repository.
    if (0 < user.permissions.repositories.Count)
    {
        Console.WriteLine($"  RepoPerms:");
        foreach (var repoPerm in user.permissions.repositories.OrderBy(e => e.Key))
        {
            Console.WriteLine($"  ->{repoPerm.Key}: repository.{repoPerm.Value}");
        }
    }
    // Print a list of permissions to the repository group.
    if (0 < user.permissions.repositories_groups.Count)
    {
        Console.WriteLine($"  RepoGroupPerms:");
        foreach (var grpPerm in user.permissions.repositories_groups)
        {
            Console.WriteLine($"  ->{grpPerm.Key}: repogroup.{grpPerm.Value}");
        }
    }

}, o => o.AnyPause());
