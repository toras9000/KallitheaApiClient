#r "nuget: KallitheaApiClient, 0.7.0.16"
#r "nuget: Lestaly, 0.42.0"
#nullable enable
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;

// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

await Paved.RunAsync(async () =>
{
    // Initialize the client
    var url = new Uri("http://localhost:9999/_admin/api");
    var key = "1111222233334444555566667777888899990000";
    using var client = new SimpleKallitheaClient(url, key);

    // Get repositories information.
    var repos = await client.GetReposAsync();

    // Save to csv file.
    var saveFile = ThisSource.RelativeFile($"get-repos_{DateTime.Now:yyyyMMdd_Hhmmss}.csv");
    await repos
        .Select(r => new
        {
            Name = r.repo_name,
            Type = r.repo_type,
            Description = r.description,
            Owner = r.owner,
            LastCommited = r.last_changeset?.date,
        })
        .SaveToCsvAsync(saveFile);

}, o => o.AnyPause());
