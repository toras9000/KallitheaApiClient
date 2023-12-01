#r "nuget: Lestaly, 0.51.0"
#nullable enable
using System.Net.Http;
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script (v1.5.0 or lator).
// You can install .NET SDK 8.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Restart docker container.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var dataDir = baseDir.RelativeDirectory("data");
    var serviceUri = new Uri(@"http://localhost:9999/");

    Console.WriteLine("Restart service");
    var composeFile = baseDir.RelativeFile("docker-compose.yml");
    await "docker".args("compose", "--file", composeFile.FullName, "down");
    await "docker".args("compose", "--file", composeFile.FullName, "up", "-d");

    Console.WriteLine("Waiting initialize ... ");
    using var initTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    var iniFile = dataDir.RelativeFile("config/kallithea.ini");
    do { await Task.Delay(500, initTimeout.Token); iniFile.Refresh(); } while (!iniFile.Exists);

    Console.WriteLine("Waiting for accessible ...");
    using var checkTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    using var checker = new HttpClient();
    while (!await checker.IsSuccessStatusAsync(serviceUri)) { await Task.Delay(1000, checkTimeout.Token); }
});