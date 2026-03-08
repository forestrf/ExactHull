using System.Diagnostics;
using System.Globalization;
using ExactHull;

int[] bothSizes = [50, 100, 200, 500, 1000, 2000, 5000, 10000, 15000, 20000, 25000];
int[] defaultOnlySizes = [30000, 40000, 50000, 75000, 100000];
const int runsPerSize = 10;
const int warmupRuns = 2;

var random = new Random(42);

using var writer = new StreamWriter("misc/benchmark_results.csv");
writer.WriteLine("n,brute_force_ms,default_ms");

foreach (int n in bothSizes)
{
    double totalOld = 0;
    double totalNew = 0;

    for (int r = 0; r < warmupRuns + runsPerSize; r++)
    {
        var points = new (double X, double Y, double Z)[n];
        for (int i = 0; i < n; i++)
            points[i] = (random.NextDouble() * 20 - 10,
                         random.NextDouble() * 20 - 10,
                         random.NextDouble() * 20 - 10);

        var sw = Stopwatch.StartNew();
        ExactHull3D.BuildBruteForce(points);
        sw.Stop();

        double oldMs = sw.Elapsed.TotalMilliseconds;

        sw.Restart();
        ExactHull3D.Build(points);
        sw.Stop();

        double newMs = sw.Elapsed.TotalMilliseconds;

        if (r >= warmupRuns)
        {
            totalOld += oldMs;
            totalNew += newMs;
        }
    }

    double avgOld = totalOld / runsPerSize;
    double avgNew = totalNew / runsPerSize;

    Console.WriteLine($"n={n,6}:  brute_force={avgOld,10:F2} ms   default={avgNew,10:F2} ms   speedup={avgOld / avgNew:F2}x");
    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1:F4},{2:F4}", n, avgOld, avgNew));
}

foreach (int n in defaultOnlySizes)
{
    double totalNew = 0;

    for (int r = 0; r < warmupRuns + runsPerSize; r++)
    {
        var points = new (double X, double Y, double Z)[n];
        for (int i = 0; i < n; i++)
            points[i] = (random.NextDouble() * 20 - 10,
                         random.NextDouble() * 20 - 10,
                         random.NextDouble() * 20 - 10);

        var sw = Stopwatch.StartNew();
        ExactHull3D.Build(points);
        sw.Stop();

        if (r >= warmupRuns)
            totalNew += sw.Elapsed.TotalMilliseconds;
    }

    double avgNew = totalNew / runsPerSize;

    Console.WriteLine($"n={n,6}:  brute_force={"N/A",10}   default={avgNew,10:F2} ms");
    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2:F4}", n, "", avgNew));
}

Console.WriteLine();
Console.WriteLine("Results saved to benchmark_results.csv");
