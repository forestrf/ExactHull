#!/usr/bin/env python3
import csv
import numpy as np
import matplotlib.pyplot as plt

cube_n, cube_ms = [], []
sphere_n, sphere_ms = [], []

with open("misc/benchmark_results.csv") as f:
    for row in csv.DictReader(f):
        n = int(row["n"])
        cube_n.append(n)
        cube_ms.append(float(row["cube_ms"]))
        sphere_n.append(n)
        sphere_ms.append(float(row["sphere_ms"]))

fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(13, 5))

ax1.plot(cube_n, cube_ms, "o-", label="Cube interior (uniform)")
ax1.plot(sphere_n, sphere_ms, "s-", label="Sphere surface (uniform, all points on hull)")
ax1.set_xlabel("Number of points")
ax1.set_ylabel("Time (ms)")
ax1.set_title("Convex Hull Build Time")
ax1.legend()
ax1.grid(True, alpha=0.3)

ax2.loglog(cube_n, cube_ms, "o-", label="Cube interior (uniform)")
ax2.loglog(sphere_n, sphere_ms, "s-", label="Sphere surface (uniform, all points on hull)")

# Reference lines for O(n log n) and O(n^2)
ref_n = np.array(cube_n, dtype=float)
nlogn = ref_n * np.log2(ref_n)
n2 = ref_n ** 2
# Scale to align with the data at a midpoint
mid = len(ref_n) // 2
nlogn_scaled = nlogn * (cube_ms[mid] / nlogn[mid])
n2_scaled = n2 * (cube_ms[mid] / n2[mid])
ax2.loglog(ref_n, nlogn_scaled, "--", color="green", alpha=0.6, label="O(n log n)")
ax2.loglog(ref_n, n2_scaled, "--", color="red", alpha=0.6, label="O(n²)")

ax2.set_xlabel("Number of points")
ax2.set_ylabel("Time (ms)")
ax2.set_title("Convex Hull Build Time (log-log)")
ax2.legend()
ax2.grid(True, alpha=0.3, which="both")

plt.tight_layout()
plt.savefig("media/benchmark_plot.png", dpi=150)
plt.show()
print("Saved benchmark_plot.png")
