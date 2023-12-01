#r "nuget: Lestaly, 0.51.0"
#nullable enable
using System.Threading;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script (v1.5.0 or lator).
// You can install .NET SDK 8.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Stop docker container and deletion of persistent data.

await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var dataDir = baseDir.RelativeDirectory("data");

    var composeFile = baseDir.RelativeFile("docker-compose.yml");
    Console.WriteLine("Stop service");
    await "docker".args("compose", "--file", composeFile.FullName, "down");

    Console.WriteLine("Delete config/repos");
    if (dataDir.Exists) { dataDir.DoFiles(c => c.File?.SetReadOnly(false)); dataDir.Delete(recursive: true); }
});
