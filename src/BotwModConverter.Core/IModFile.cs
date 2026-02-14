using BotwModConverter.Core.Common;

namespace BotwModConverter.Core;

public interface IModFile
{
    public ReadOnlySpan<char> Canon { get; }
    
    public string RelativeFilePath { get; }

    static abstract void Peek<T>(T file, in Span<byte> view) where T : IModFile, allows ref struct;
    
    RentedBuffer Rent();
    
    Stream Stream();
    
    void Write(ArraySegment<byte> data);

    Stream OpenWrite<T>(out Action<T>? compress) where T : IModFile, allows ref struct;
    
    void WriteCopy();
}