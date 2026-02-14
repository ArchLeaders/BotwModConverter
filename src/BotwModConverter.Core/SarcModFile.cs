using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BotwModConverter.Core.Common;
using BotwModConverter.Core.Utils;
using CsYaz0;
using SarcLibrary;

namespace BotwModConverter.Core;

public ref struct SarcModFile(Sarc parent, string name, ArraySegment<byte> source) : IModFile
{
    private readonly Sarc _parent = parent;
    private readonly ArraySegment<byte> _source = source;
    private bool _isCompressed;

    public ReadOnlySpan<char> Canon { get; } = name.ToCanon();

    public string RelativeFilePath { get; } = name;

    public static void Peek<T>(T file, in Span<byte> view) where T : IModFile, allows ref struct
    {
        var sarcFile = Unsafe.As<T, SarcModFile>(ref file);
        sarcFile._source.AsSpan(0, view.Length).CopyTo(view);
    }

    public static SarcModFile FromEntry(Sarc parent, string name, ArraySegment<byte> source)
        => new(parent, name, source);

    public RentedBuffer Rent()
    {
        var data = _source.AsSpan();
        
        if (Unsafe.As<byte, int>(ref data[0]) != Yaz0.MAGIC) {
            return RentedBuffer.Virtual(_source);
        }
        
        _isCompressed = true;

        var decompressed = RentedBuffer.Rent(Yaz0.GetDecompressedSize(data));
        Yaz0.Decompress(data, decompressed.Span);
        return decompressed;
    }

    public Stream Stream()
    {
        ArgumentNullException.ThrowIfNull(_source.Array);
        
        var data = _source.AsSpan();
        
        if (Unsafe.As<byte, int>(ref data[0]) != Yaz0.MAGIC) {
            return new MemoryStream(_source.Array, _source.Offset, _source.Count, writable: false);
        }
        
        _isCompressed = true;
        
        var decompressed = new byte[Yaz0.GetDecompressedSize(data)];
        Yaz0.Decompress(data, decompressed);
        
        return new MemoryStream(decompressed, writable: false);
    }

    public void Write(ArraySegment<byte> data)
    {
        _parent[RelativeFilePath] = data;
    }

    public Stream OpenWrite<T>(out Action<T>? compress) where T : IModFile, allows ref struct
    {
        compress = _isCompressed ? Compress : null;
        return _parent.OpenWrite(RelativeFilePath);
    }

    private static void Compress<T>(T modFile) where T : IModFile, allows ref struct
    {
        var file = Unsafe.As<T, SarcModFile>(ref modFile);

        ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault(file._parent, file.RelativeFilePath, out bool exists);
        
        if (!exists) {
            throw new FileNotFoundException($"Error compressing '{file.RelativeFilePath}', an existing entry could not be found.");
        }

        using var compressed = Yaz0.Compress(data);
        data = compressed.ToArray();
    }

    public void WriteCopy()
    {
    }
}