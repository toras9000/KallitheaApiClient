// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

#r "nuget: Lestaly, 0.42.0"
using System.Threading;
using Lestaly;

// Restart docker container with deletion of persistent data.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");

    var composeFile = baseDir.RelativeFile("docker-compose.yml");
    if (!composeFile.Exists) throw new PavedMessageException("Not found compose file");

    Console.WriteLine("Stop service");
    await CmdProc.ExecAsync("docker-compose", new[] { "--file", composeFile.FullName, "down", }).AsSuccessCode();

    Console.WriteLine("Delete config/repos");
    var confDir = baseDir.RelativeDirectory("config");
    var reposDir = baseDir.RelativeDirectory("repos");
    if (confDir.Exists) { confDir.DoFiles(c => c.File?.SetReadOnly(false)); confDir.Delete(recursive: true); }
    if (reposDir.Exists) { reposDir.DoFiles(c => c.File?.SetReadOnly(false)); reposDir.Delete(recursive: true); }

    Console.WriteLine("Start service");
    await CmdProc.ExecAsync("docker-compose", new[] { "--file", composeFile.FullName, "up", "-d", }).AsSuccessCode();

    Console.Write("Waiting initialize ... ");
    using var timer = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var iniFile = baseDir.RelativeFile("config/kallithea.ini");
    do { await Task.Delay(500, timer.Token); iniFile.Refresh(); } while (!iniFile.Exists);
    Console.WriteLine("completed.");
});
