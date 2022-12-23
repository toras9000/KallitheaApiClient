// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

#r "nuget: KallitheaApiClient, 0.7.0.11"
#r "nuget: Lestaly, 0.20.0"
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;

await Paved.RunAsync(async () =>
{
    // Initialize the client
    var url = new Uri("http://localhost:9999/_admin/api");
    var key = "1111222233334444555566667777888899990000";
    using var client = new SimpleKallitheaClient(url, key);

    // Handle API log events and record them in the log file.
    using var logWriter = ThisSource.GetRelativeFile($"log-api-json_{DateTime.Now:yyyyMMdd_Hhmmss}.txt").CreateTextWriter();
    client.Logging += log => logWriter.WriteLine($"Request:\n  {log.Request}\nResponse:(result={log.Status})\n  {log.Response}\n");

    // Make some kind of API call.
    var user = await client.GetUserAsync();
    var repos = await client.GetReposAsync();
    foreach (var repo in repos.Take(3))   // only some repos
    {
        Console.WriteLine($"Repository: {repo.repo_name}");
        try
        {
            // Get repository changesets.
            var changesets = await client.GetChangesetsAsync(new($"{repo.repo_id}", max_revisions: "5", reverse: true));
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
