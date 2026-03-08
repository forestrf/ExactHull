using System.Diagnostics;
using System.Globalization;
using ExactHull;

int[] sizes = [50, 100, 200, 500, 1000, 2000, 5000, 10000, 15000, 20000, 25000, 30000, 40000, 50000, 75000, 100000];
const int runsPerSize = 10;
const int warmupRuns = 2;

var random = new Random(42);

using var writer = new StreamWriter("misc/benchmark_results.csv");
writer.WriteLine("n,cube_ms,sphere_ms");

foreach (int n in sizes)
{
    double totalCube = 0;
    double totalSphere = 0;

    for (int r = 0; r < warmupRuns + runsPerSize; r++)
    {
        // Cube: uniform random in [-10, 10]^3
        var cubePoints = new (double X, double Y, double Z)[n];
        for (int i = 0; i < n; i++)
            cubePoints[i] = (random.NextDouble() * 20 - 10,
                             random.NextDouble() * 20 - 10,
                             random.NextDouble() * 20 - 10);

        var sw = Stopwatch.StartNew();
        ExactHull3D.Build(cubePoints);
        sw.Stop();

        if (r >= warmupRuns)
            totalCube += sw.Elapsed.TotalMilliseconds;

        // Sphere: points on unit sphere surface
        var spherePoints = new (double X, double Y, double Z)[n];
        for (int i = 0; i < n; i++)
        {
            double x, y, z, lenSq;
            do
            {
                x = random.NextDouble() * 2.0 - 1.0;
                y = random.NextDouble() * 2.0 - 1.0;
                z = random.NextDouble() * 2.0 - 1.0;
                lenSq = x * x + y * y + z * z;
            } while (lenSq < 1e-12 || lenSq > 1.0);

            double invLen = 1.0 / Math.Sqrt(lenSq);
            spherePoints[i] = (x * invLen, y * invLen, z * invLen);
        }

        sw.Restart();
        ExactHull3D.Build(spherePoints);
        sw.Stop();

        if (r >= warmupRuns)
            totalSphere += sw.Elapsed.TotalMilliseconds;
    }

    double avgCube = totalCube / runsPerSize;
    double avgSphere = totalSphere / runsPerSize;

    Console.WriteLine($"n={n,6}:  cube={avgCube,10:F2} ms   sphere={avgSphere,10:F2} ms");
    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0},{1:F4},{2:F4}", n, avgCube, avgSphere));
}

Console.WriteLine();
Console.WriteLine("Results saved to benchmark_results.csv");
