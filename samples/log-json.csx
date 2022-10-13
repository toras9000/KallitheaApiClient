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

    // Handle API log events and record them in the log file.
    using var logWriter = ThisSource.GetRelativeFile("api_log.txt").CreateTextWriter();
    client.Logging += log => logWriter.WriteLine($"Request:\n  {log.Request}\nResponse:(result={log.Status})\n  {log.Response}\n");

    // Make some kind of API call.
    var user = (await client.GetUserAsync()).result;
    var repos = (await client.GetReposAsync()).result.repos;
    foreach (var repo in repos.Take(3))   // only some repos
    {
        Console.WriteLine($"Repository: {repo.repo_name}");
        try
        {
            // Get repository changesets.
            var changesets = (await client.GetChangesetsAsync(new($"{repo.repo_id}", max_revisions: "5", reverse: true))).result.changesets;
            foreach (var change in changesets)
            {
                Console.WriteLine($"  {change.summary.short_id} : {change.summary.message?.FirstLine()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
        }
    }

}, o => o.AnyPause());
