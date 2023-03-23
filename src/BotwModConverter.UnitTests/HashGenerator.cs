using BotwModConverter.Core;
using Cead;
using Cead.Interop;
using System.Buffers.Binary;
using System.Collections.Concurrent;

namespace BotwModConverter.UnitTests;

[TestClass]
public class HashGenerator
{
    [TestInitialize]
    public void Setup()
    {
        DllManager.LoadCead();
    }

    [TestMethod]
    public async Task GenerateHashesWiiU()
    {
        ConcurrentBag<ulong> wiiu = new();

        await Parallel.ForEachAsync(Directory.EnumerateFiles(BotwConfig.Shared.GamePath, "*.*", SearchOption.AllDirectories), (file, cancellationToken) => {
            Console.WriteLine(file);
            Process(wiiu, File.ReadAllBytes(file));
            return ValueTask.CompletedTask;
        });
        
        await Parallel.ForEachAsync(Directory.EnumerateFiles(BotwConfig.Shared.UpdatePath, "*.*", SearchOption.AllDirectories), (file, cancellationToken) => {
            Console.WriteLine(file);
            Process(wiiu, File.ReadAllBytes(file));
            return ValueTask.CompletedTask;
        });
        
        await Parallel.ForEachAsync(Directory.EnumerateFiles(BotwConfig.Shared.DlcPath, "*.*", SearchOption.AllDirectories), (file, cancellationToken) => {
            Console.WriteLine(file);
            Process(wiiu, File.ReadAllBytes(file));
            return ValueTask.CompletedTask;
        });

        List<ulong> sorted = wiiu.Distinct().Order().ToList();
        Compile(sorted, Path.Combine($"wiiu-{sorted.Count}.xxhash"));
    }

    [TestMethod]
    public async Task GenerateHashesNX()
    {
        ConcurrentBag<ulong> nx = new();

        await Parallel.ForEachAsync(Directory.EnumerateFiles(BotwConfig.Shared.GamePathNx, "*.*", SearchOption.AllDirectories), (file, cancellationToken) => {
            Console.WriteLine(file);
            Process(nx, File.ReadAllBytes(file));
            return ValueTask.CompletedTask;
        });

        await Parallel.ForEachAsync(Directory.EnumerateFiles(BotwConfig.Shared.DlcPathNx, "*.*", SearchOption.AllDirectories), (file, cancellationToken) => {
            Console.WriteLine(file);
            Process(nx, File.ReadAllBytes(file));
            return ValueTask.CompletedTask;
        });

        List<ulong> sorted = nx.Distinct().Order().ToList();
        Compile(sorted, Path.Combine($"nx-{sorted.Count}.xxhash"));
    }

    public static void Process(ConcurrentBag<ulong> list, Span<byte> raw)
    {
        if (raw.Length <= 0) return;

        Span<byte> data = Utils.Decompress(raw, out bool _);
        if (data.Length > 4 && data[0..4].SequenceEqual("SARC"u8)) {
            ProcessSarc(list, data);
        }
            
        list.Add(Standart.Hash.xxHash.xxHash3.ComputeHash(data, data.Length));
    }
    public static void ProcessSarc(ConcurrentBag<ulong> list, Span<byte> raw)
    {
        Sarc sarc = Sarc.FromBinary(raw);
        foreach ((var key, var data) in sarc) {
            Console.WriteLine("   " + key);
            Process(list, data);
        }
    }

    public static void Compile(List<ulong> hashes, string output)
    {
        using FileStream fs = File.Create(output);
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];

        foreach (var hash in hashes) {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, hash);
            fs.Write(buffer);
        }
    }
}
