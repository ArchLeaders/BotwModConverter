using BotwModConverter.Core;
using System.Diagnostics;

ConverterLog.AddListener(new TextWriterTraceListener(Console.OpenStandardOutput()));

Stopwatch watch = Stopwatch.StartNew();

int count = await BotwConverter.ConvertMod(args[0], args[1], ThreadMode.Single);

watch.Stop();
Console.WriteLine($"Processed {count} files");
Console.WriteLine($"Elapsed Ticks: {watch.ElapsedTicks}");
Console.WriteLine($"Elapsed Milliseconds: {watch.ElapsedMilliseconds}");