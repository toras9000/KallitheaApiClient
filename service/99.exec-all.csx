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
    static Task runScript(string name)
    {
        ConsoleWig.WriteLineColored(ConsoleColor.Green, name);
        return CmdProc.ExecAsync("dotnet", new[] { "script", ThisSource.RelativeFile(name).FullName, }, stdOut: Console.Out, stdErr: Console.Error).AsSuccessCode();
    }

    await runScript("00.delete-data.csx");
    await runScript("01.restart.csx");
    await runScript("02.rewrite-api-key-for-debug.csx");
    await runScript("03.log-level-all-warn.csx");
    await runScript("10.init-test-entities.csx");
});
