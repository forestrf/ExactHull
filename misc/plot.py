#!/usr/bin/env python3
import csv
import matplotlib.pyplot as plt

data = {"n": [], "brute_force_ms": [], "default_ms": []}

with open("misc/benchmark_results.csv") as f:
    for row in csv.DictReader(f):
        data["n"].append(int(row["n"]))
        data["brute_force_ms"].append(float(row["brute_force_ms"]))
        data["default_ms"].append(float(row["default_ms"]))

fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(13, 5))

ax1.plot(data["n"], data["brute_force_ms"], "o-", label="Brute Force")
ax1.plot(data["n"], data["default_ms"], "s-", label="Default (Outside Sets)")
ax1.set_xlabel("Number of points")
ax1.set_ylabel("Time (ms)")
ax1.set_title("Convex Hull Build Time")
ax1.legend()
ax1.grid(True, alpha=0.3)

ax2.loglog(data["n"], data["brute_force_ms"], "o-", label="Brute Force")
ax2.loglog(data["n"], data["default_ms"], "s-", label="Default (Outside Sets)")
ax2.set_xlabel("Number of points")
ax2.set_ylabel("Time (ms)")
ax2.set_title("Convex Hull Build Time (log-log)")
ax2.legend()
ax2.grid(True, alpha=0.3, which="both")

plt.tight_layout()
plt.savefig("media/benchmark_plot.png", dpi=150)
plt.show()
print("Saved benchmark_plot.png")
