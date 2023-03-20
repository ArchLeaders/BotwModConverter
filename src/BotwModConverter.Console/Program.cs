using BotwModConverter.Core;
using System.Diagnostics;

int count = -1;
ConverterLog.AddListener(new TextWriterTraceListener(Console.OpenStandardOutput()));
Stopwatch watch = Stopwatch.StartNew();

try {
    #if DEBUG
    count = BotwConverter.ConvertModDebug(args[0], args[1], ThreadMode.Single);
    #else
    count = await BotwConverter.ConvertMod(args[0], args[1], ThreadMode.Parallel);
    #endif

}
catch (Exception ex) {
    Console.WriteLine("\n\n" + ex);
    // if (Directory.Exists(args[1])) {
    //     Directory.Delete(args[1], true);
    // }
}

watch.Stop();
Console.WriteLine($"Processed {count} files");
Console.WriteLine($"Elapsed Ticks: {watch.ElapsedTicks}");
Console.WriteLine($"Elapsed Milliseconds: {watch.ElapsedMilliseconds}");