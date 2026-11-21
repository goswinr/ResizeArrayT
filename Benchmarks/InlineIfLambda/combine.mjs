// Combines several runs of the benchmark into one CSV: the median time per method,
// and the fastest and slowest of the runs. Usage:
// node combine.mjs run1.csv run2.csv run3.csv > results.csv
import { readFileSync } from "node:fs";

const files = process.argv.slice(2);
const lines = files.map(f => readFileSync(f, "utf8").split(/\r?\n/).filter(l => l));
const comments = lines[0].filter(l => l.startsWith("#") && !l.startsWith("# All correctness"));
const runs = lines.map(run => run.filter(l => !l.startsWith("#") && !l.startsWith("function,")).map(l => l.split(",")));
const median = xs => [...xs].sort((a, b) => a - b)[Math.floor(xs.length / 2)];

for (const c of comments) console.log(c);
console.log(`# Median of ${files.length} runs`);
console.log("function,call_site,count,method,ns,relative_to_current,ns_min,ns_max,bytes");
let currentNs = NaN;
runs[0].forEach((first, r) => {
    const key = first.slice(0, 4).join(",");
    for (const run of runs)
        if (run[r].slice(0, 4).join(",") !== key) throw new Error(`The runs differ at row ${r}: ${key}`);
    const ns = runs.map(run => Number(run[r][4]));
    const ms = median(ns);
    if (first[3] === "current") currentNs = ms;
    console.log([key, ms.toFixed(1), (ms / currentNs).toFixed(3), Math.min(...ns).toFixed(1), Math.max(...ns).toFixed(1), first[6]].join(","));
});
