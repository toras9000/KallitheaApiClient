#r "nuget: KallitheaApiClient, 0.7.0.8"
#r "nuget: Lestaly, 0.13.0"

// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

using System.Text.RegularExpressions;
using KallitheaApiClient;
using Lestaly;

// Create a repository group along with adding users to create a simple dedicated area.
// Users to be registered are defined in a csv file.

await Paved.RunAsync((Func<ValueTask>)(async () =>
{
    // Read the user information to be registered.
    var targets = ThisSource.GetRelativeFile("regist-users-list.csv").ReadAllText()
        .SplitFields(',')
        .Skip(1)
        .Where(f => 4 <= f?.Length)
        .Select(f => new { LoginID = f[0], FirstName = f[1], LastName = f[2], Mail = f[3], Password = f.ElementAtOrDefault(4), })
        .ToArray();

    // Initialize the kallithea client. (Requires admin)
    using var client = new KallitheaClient(new("http://localhost:9999/_admin/api"));
    client.ApiKey = "1111222233334444555566667777888899990000";

    // If not, create a parent group for the group for the user.
    var baseRepoGrpName = "users";
    var baseRepoGrpGroup = await Try.Func(async () => (await client.GetRepoGroupAsync(new(baseRepoGrpName))).result.repogroup, _ => null);
    if (baseRepoGrpGroup == null)
    {
        baseRepoGrpGroup = (await client.CreateRepoGroupAsync(new(baseRepoGrpName))).result.repo_group;
    }

    // Create listed users.
    foreach (var user in targets)
    {
        Console.WriteLine($"User: {user.LoginID}");
        try
        {
            // Create user.
            await client.CreateUserAsync(new(user.LoginID, user.Mail, user.FirstName, user.LastName, user.Password));
            Console.WriteLine($"  User created.");

            // Create repository groups for user. User is the owner.
            var userRepoGrpName = $"{baseRepoGrpName}/{user.LoginID}";
            await client.CreateRepoGroupAsync(new(user.LoginID, parent: baseRepoGrpName, owner: user.LoginID));
            Console.WriteLine($"  Repo group created. '{userRepoGrpName}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
        }
        Console.WriteLine();
    }

}), o => o.AnyPause());
