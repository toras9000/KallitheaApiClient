#r "nuget: Lestaly, 0.13.0"
using System.Threading;
using Lestaly;

await Paved.RunAsync(async () =>
{
    var composeFile = ThisSource.GetRelativeFile("docker-compose.yml");
    if (!composeFile.Exists) throw new PavedMessageException("Not found compose file");

    Console.WriteLine("Stop service");
    var downProc = Process.Start("docker-compose", $"--file \"{composeFile.FullName}\" down");
    await downProc.WaitForExitAsync();
    if (downProc.ExitCode != 0) throw new PavedMessageException($"Failed to down. ExitCode={downProc.ExitCode}");

    Console.WriteLine("Delete files");
    var confDir = ThisSource.GetRelativeDirectory("config");
    var reposDir = ThisSource.GetRelativeDirectory("repos");
    if (confDir.Exists) { confDir.DoFiles(c => c.File?.SetReadOnly(false)); confDir.Delete(recursive: true); }
    if (reposDir.Exists) { reposDir.DoFiles(c => c.File?.SetReadOnly(false)); reposDir.Delete(recursive: true); }

    Console.WriteLine("Start service");
    var upProc = Process.Start("docker-compose", $"--file \"{composeFile.FullName}\" up -d");
    await upProc.WaitForExitAsync();
    if (upProc.ExitCode != 0) throw new PavedMessageException($"Failed to up. ExitCode={upProc.ExitCode}");

    Console.Write("Waiting initialize ... ");
    using var timer = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    var iniFile = ThisSource.GetRelativeFile("config/kallithea.ini");
    do
    {
        await Task.Delay(500, timer.Token);
        iniFile.Refresh();
    } while (!iniFile.Exists);
    Console.WriteLine("completed.");
});
