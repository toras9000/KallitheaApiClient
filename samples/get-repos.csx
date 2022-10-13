#r "nuget: KallitheaApiClient, 0.7.0.8"
#r "nuget: Lestaly, 0.13.0"

// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

using KallitheaApiClient;
using Lestaly;

await Paved.RunAsync(async () =>
{
    // Initialize the client
    using var client = new KallitheaClient(new("http://localhost:9999/_admin/api"));
    client.ApiKey = "1111222233334444555566667777888899990000";

    // Get repositories information.
    var rsp = await client.GetReposAsync();
    await rsp.result.repos
        .Select(r => new
        {
            Name = r.repo_name,
            Type = r.repo_type,
            Description = r.description,
            Owner = r.owner,
            LastCommited = r.last_changeset?.date,
        })
        .SaveToCsvAsync(ThisSource.GetRelativeFile("repos.csv").FullName);

}, o => o.AnyPause());
