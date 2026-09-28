import assert from "node:assert/strict";
import { ResizeArray_sort } from "./_js/Src/Module.js";
import { ints, floats, strings } from "./_js/TypedSort.js";

// Ordinary JS arrays throughout; copying is included for every implementation.
// Numeric inputs are finite, and strings are non-null.
const numeric = xs => xs.slice().sort((a, b) => a - b);
const ordinalStrings = xs => xs.slice().sort();
let consumed = 0;

function batch(count, sort, source) {
    const start = performance.now();
    for (let i = 0; i < count; i++) {
        consumed ^= sort(source).length;
    }
    return performance.now() - start;
}

function median(values) {
    return values.slice().sort((a, b) => a - b)[Math.floor(values.length / 2)];
}

function bench(name, source, native, typed) {
    const methods = [ResizeArray_sort, native, typed];
    const original = source.slice();
    const expected = ResizeArray_sort(source);
    for (const sort of methods) {
        const result = sort(source);
        assert.deepStrictEqual(result, expected, `${name}: ordering`);
        assert.deepStrictEqual(source, original, `${name}: input preserved`);
        assert.notStrictEqual(result, source, `${name}: new array`);
        const started = performance.now();
        while (performance.now() - started < 300) batch(1, sort, source);
    }
    const counts = methods.map(sort => {
        let count = 1;
        while (batch(count, sort, source) < 60) count *= 2;
        return count;
    });
    const samples = methods.map(() => []);
    for (let round = 0; round < 9; round++) {
        // Rotate which method runs first each round.
        for (let step = 0; step < methods.length; step++) {
            const m = (round + step) % methods.length;
            samples[m].push(batch(counts[m], methods[m], source) / counts[m]);
        }
    }
    const [genericMs, nativeMs, typedMs] = samples.map(median);
    console.log([name, source.length, genericMs.toFixed(6), nativeMs.toFixed(6),
        typedMs.toFixed(6), (genericMs / nativeMs).toFixed(3),
        (genericMs / typedMs).toFixed(3)].join(","));
}

console.log("type,count,fable_generic_ms,native_js_ms,fable_typed_ms,native_speedup,typed_speedup");
let state = 42;
for (const size of [16, 1024, 10000]) {
    const input = Array.from({ length: size }, () => {
        state = (state * 25173 + 13849) & 65535;
        return state - 32768;
    });
    bench("int/random", input, numeric, ints);
    bench("float/random", input.map(x => x / 7), numeric, floats);
    bench("string/random", input.map(String), ordinalStrings, strings);
}
console.error(`All ordering/copy checks passed. Consumed: ${consumed}`);
