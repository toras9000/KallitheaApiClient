#r "nuget: System.Data.SQLite.Core, 1.0.118"
#r "nuget: Dapper, 2.0.123"
#r "nuget: Lestaly, 0.43.0"
#nullable enable
using System.Data.SQLite;
using Dapper;
using Lestaly;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// 

return await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var dataDir = baseDir.RelativeDirectory("data");

    // check ini file.
    var iniFile = dataDir.RelativeFile("./config/kallithea.ini");
    if (!iniFile.Exists) throw new PavedMessageException("ini file not found.");

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
    await CmdProc.ExecAsync("docker-compose", new[] { "--file", composeFile.FullName, "down", }).AsSuccessCode();
    await CmdProc.ExecAsync("docker-compose", new[] { "--file", composeFile.FullName, "up", "-d", }).AsSuccessCode();
    Console.WriteLine("...Success");

});
