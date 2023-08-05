#r "nuget: Lestaly, 0.43.0"
#nullable enable
using System.Threading;
using Lestaly;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Restart docker container with deletion of persistent data.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var dataDir = baseDir.RelativeDirectory("data");

    var composeFile = baseDir.RelativeFile("docker-compose.yml");
    if (!composeFile.Exists) throw new PavedMessageException("Not found compose file");

    Console.WriteLine("Stop service");
    await CmdProc.ExecAsync("docker-compose", new[] { "--file", composeFile.FullName, "down", }).AsSuccessCode();

    Console.WriteLine("Delete config/repos");
    if (dataDir.Exists) { dataDir.DoFiles(c => c.File?.SetReadOnly(false)); dataDir.Delete(recursive: true); }

    Console.WriteLine("Start service");
    await CmdProc.ExecAsync("docker-compose", new[] { "--file", composeFile.FullName, "up", "-d", }).AsSuccessCode();

    Console.Write("Waiting initialize ... ");
    using var timer = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    var iniFile = dataDir.RelativeFile("config/kallithea.ini");
    do { await Task.Delay(500, timer.Token); iniFile.Refresh(); } while (!iniFile.Exists);
    Console.WriteLine("completed.");
});
