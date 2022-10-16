#r "nuget: ProcessX, 1.5.4"
#r "nuget: Lestaly, 0.13.0"

using Cysharp.Diagnostics;
using Lestaly;

await Paved.RunAsync(async () =>
{
    var composeFile = ThisSource.GetRelativeFile("docker-compose.yml");
    if (!composeFile.Exists) throw new PavedMessageException("Not found compose file");
    Console.WriteLine("Stop service");
    await ProcessX.GetDualAsyncEnumerable($"docker-compose --file \"{composeFile.FullName}\" down").Process.WaitForExitAsync();
    Console.WriteLine("Delete files");
    var confDir = ThisSource.GetRelativeDirectory("config");
    var reposDir = ThisSource.GetRelativeDirectory("repos");
    if (confDir.Exists) { confDir.DoFiles(c => c.File?.SetReadOnly(false)); confDir.Delete(recursive: true); }
    if (reposDir.Exists) { reposDir.DoFiles(c => c.File?.SetReadOnly(false)); reposDir.Delete(recursive: true); }
    Console.WriteLine("Start service");
    await ProcessX.GetDualAsyncEnumerable($"docker-compose --file \"{composeFile.FullName}\" up -d").Process.WaitForExitAsync();
});
