#!/usr/bin/env python3
import csv
import matplotlib.pyplot as plt

bf_n, bf_ms = [], []
default_n, default_ms = [], []

with open("misc/benchmark_results.csv") as f:
    for row in csv.DictReader(f):
        n = int(row["n"])
        default_n.append(n)
        default_ms.append(float(row["default_ms"]))
        if row["brute_force_ms"].strip():
            bf_n.append(n)
            bf_ms.append(float(row["brute_force_ms"]))

fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(13, 5))

ax1.plot(bf_n, bf_ms, "o-", label="Brute Force")
ax1.plot(default_n, default_ms, "s-", label="Default (Outside Sets)")
ax1.set_xlabel("Number of points")
ax1.set_ylabel("Time (ms)")
ax1.set_title("Convex Hull Build Time")
ax1.legend()
ax1.grid(True, alpha=0.3)

ax2.loglog(bf_n, bf_ms, "o-", label="Brute Force")
ax2.loglog(default_n, default_ms, "s-", label="Default (Outside Sets)")
ax2.set_xlabel("Number of points")
ax2.set_ylabel("Time (ms)")
ax2.set_title("Convex Hull Build Time (log-log)")
ax2.legend()
ax2.grid(True, alpha=0.3, which="both")

plt.tight_layout()
plt.savefig("media/benchmark_plot.png", dpi=150)
plt.show()
print("Saved benchmark_plot.png")
