using BotwModConverter.Core;
using System.Diagnostics;

Stopwatch watch = Stopwatch.StartNew();

int count = await BotwConverter.ConvertMod(args[0], args[1]);

watch.Stop();
Console.WriteLine($"Processed {count} files");
Console.WriteLine($"Elapsed Ticks: {watch.ElapsedTicks}");
Console.WriteLine($"Elapsed Milliseconds: {watch.ElapsedMilliseconds}");