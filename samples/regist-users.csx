#r "nuget: KallitheaApiClient, 0.7.0.13"
#r "nuget: Lestaly, 0.42.0"
#nullable enable
using System.Text.RegularExpressions;
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;

// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Create a repository group along with adding users to create a simple dedicated area.
// Users to be registered are defined in a csv file.

await Paved.RunAsync((Func<ValueTask>)(async () =>
{
    // Read the user information to be registered.
    var targets = ThisSource.RelativeFile("regist-users-list.csv").ReadAllText()
        .SplitFields(',')
        .Skip(1)    // Skip header
        .Where(f => 4 <= f?.Length)
        .Select(f => new { LoginID = f[0], FirstName = f[1], LastName = f[2], Mail = f[3], Password = f.ElementAtOrDefault(4), })
        .ToArray();

    // Initialize the kallithea client. (Requires admin)
    var url = new Uri("http://localhost:9999/_admin/api");
    var key = "1111222233334444555566667777888899990000";
    using var client = new SimpleKallitheaClient(url, key);

    // If not, create a parent group for the group for the user.
    var baseRepoGrpName = "users";
    var baseRepoGrpGroup = await Try.FuncOrDefaultAsync(async () => await client.GetRepoGroupInfoAsync(new(baseRepoGrpName)));
    if (baseRepoGrpGroup == null)
    {
        baseRepoGrpGroup = await client.CreateRepoGroupAsync(new(baseRepoGrpName));
    }

    // Create listed users.
    foreach (var target in targets)
    {
        Console.WriteLine($"User: {target.LoginID}");
        try
        {
            // Create user.
            var user = await client.CreateUserAsync(new(target.LoginID, target.Mail, target.FirstName, target.LastName, target.Password));
            Console.WriteLine($"  User created.");

            // Create repository groups for user. User is the owner.
            var userRepoGrpName = $"{baseRepoGrpName}/{target.LoginID}";
            var userRepoGrp = await client.CreateRepoGroupAsync(new(target.LoginID, parent: baseRepoGrpName, owner: target.LoginID));
            Console.WriteLine($"  Repo group created. '{userRepoGrpName}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
        }
        Console.WriteLine();
    }

}), o => o.AnyPause());
