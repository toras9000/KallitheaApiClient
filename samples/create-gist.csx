#r "nuget: KallitheaApiClient, 0.7.0.9"
#r "nuget: Lestaly, 0.13.0"

// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;

await Paved.RunAsync(async () =>
{
    // Initialize the client
    var url = new Uri("http://localhost:9999/_admin/api");
    var key = "1111222233334444555566667777888899990000";
    using var client = new KallitheaClient(url, key);

    // Information to create a Gist 
    var files = new Dictionary<string, GistContent>
    {
        { "file1.txt", new GistContent("text\ncontent", "text") },
        { "file2.cs",  new GistContent("using System;\n\nConsole.WriteLine(\"Hello World.\");", "csharp") },
    };
    var lifetime = 10 * 24 * 60;  // 10 days [minutes]

    // Create Gist
    var created = await client.CreateGistAsync(new(files, "test gist", GistType.@public, lifetime: lifetime));
    Console.WriteLine($"Createt: {created.result.msg}");
    Console.WriteLine($"  Gist ID     : {created.result.gist.gist_id}");
    Console.WriteLine($"  Type        : {created.result.gist.type}");
    Console.WriteLine($"  Description : {created.result.gist.description}");
    Console.WriteLine($"  URL         : {created.result.gist.url}");
    var expires = (created.result.gist.expires < 0) ? "permanent" : DateTime.UnixEpoch.AddSeconds(created.result.gist.expires).ToString();
    Console.WriteLine($"  Expires     : {expires}");

}, o => o.AnyPause());
