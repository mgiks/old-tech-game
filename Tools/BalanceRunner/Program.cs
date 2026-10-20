using System;
using System.IO;

internal static class Program
{
    private static int Main()
    {
        string report = BalanceSimulator.Run();
        Console.WriteLine(report);
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "balance-sim-output.txt"));
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, report);
        Console.WriteLine("WROTE " + path);
        return report.Contains("VALIDATION: FAIL") ? 1 : 0;
    }
}
