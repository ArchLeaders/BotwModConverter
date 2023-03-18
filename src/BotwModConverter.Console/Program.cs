using Cead;
using Cead.Interop;

//ConverterLog.AddListener(new TextWriterTraceListener(Console.OpenStandardOutput()));

//Stopwatch watch = Stopwatch.StartNew();

//int count = await BotwConverter.ConvertMod(args[0], args[1], ThreadMode.Single);

//watch.Stop();
//Console.WriteLine($"Processed {count} files");
//Console.WriteLine($"Elapsed Ticks: {watch.ElapsedTicks}");
//Console.WriteLine($"Elapsed Milliseconds: {watch.ElapsedMilliseconds}");

DllManager.LoadCead();

using FileStream fs = File.OpenRead(args[0]);
Span<byte> buffer = new byte[(int)fs.Length];
fs.Read(buffer);

Byml byml = Byml.FromBinary(buffer);
Console.WriteLine(byml.ToText());

Console.ReadLine();