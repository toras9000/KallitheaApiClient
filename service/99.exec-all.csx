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

return await Paved.RunAsync(config: o => o.AnyPause(), action: async () =>
{
    static Task runScript(string name)
    {
        ConsoleWig.WriteLineColored(ConsoleColor.Green, name);
        var scriptFile = ThisSource.RelativeFile(name);
        return "dotnet".args("script", scriptFile.FullName, "--", "--no-interact").result().success();
    }

    await runScript("00.delete-data.csx");
    await runScript("01.restart.csx");
    await runScript("02.rewrite-api-key-for-debug.csx");
    await runScript("03.log-level-all-warn.csx");
    await runScript("10.init-test-entities.csx");
});
