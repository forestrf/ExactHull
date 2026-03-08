using System.Diagnostics;
using System.Globalization;
using ExactHull;

int[] sizes = [50, 100, 200, 500, 1000, 2000, 5000, 10000, 15000, 20000, 25000, 30000, 40000, 50000, 75000, 100000];
const int runsPerSize = 10;
const int warmupRuns = 2;

var random = new Random(42);

using var writer = new StreamWriter("misc/benchmark_results.csv");
writer.WriteLine("n,ms");

foreach (int n in sizes)
{
    double total = 0;

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
            total += sw.Elapsed.TotalMilliseconds;
    }

    double avg = total / runsPerSize;

    Console.WriteLine($"n={n,6}:  {avg,10:F2} ms");
    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1:F4}", n, avg));
}

Console.WriteLine();
Console.WriteLine("Results saved to benchmark_results.csv");
