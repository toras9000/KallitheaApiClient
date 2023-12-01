#r "nuget: System.Data.SQLite.Core, 1.0.118"
#r "nuget: Dapper, 2.1.24"
#r "nuget: Lestaly, 0.51.0"
#nullable enable
using System.Data.SQLite;
using Dapper;
using Lestaly;
using Lestaly.Cx;

// This script is meant to run with dotnet-script (v1.5.0 or lator).
// You can install .NET SDK 8.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// In kallithea's SQLite database, rewrite admin's API key to a fixed key for debugging.

return await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var dataDir = baseDir.RelativeDirectory("data");
    var iniFile = dataDir.RelativeFile("./config/kallithea.ini").ThrowIfNotExists(f => new PavedMessageException("ini file not found."));

    // Rewrite all log levels
    Console.WriteLine("Reqrite log levels");
    var lines = await iniFile.ReadAllLinesAsync();
    for (var i = 0; i < lines.Length; i++)
    {
        if (lines[i].IsMatch(@"^\s*level\s*=\s*\w+\s*$"))
        {
            lines[i] = "level = WARN";
        }
    }
    await iniFile.WriteAllLinesAsync(lines);
    Console.WriteLine("...Success");

    var composeFile = baseDir.RelativeFile("docker-compose.yml");
    Console.WriteLine("Restart service");
    await "docker".args("compose", "--file", composeFile.FullName, "down");
    await "docker".args("compose", "--file", composeFile.FullName, "up", "-d");
    Console.WriteLine("...Success");

});
