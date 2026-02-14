using System.Buffers;

namespace BotwModConverter.Core.Common;

public struct RentedBuffer : IDisposable
{
    public readonly ArraySegment<byte> Segment;
    public readonly bool IsVirtual;
    public bool HasExternalHolder;
    
    public Span<byte> Span => Segment.AsSpan();

    private RentedBuffer(ArraySegment<byte> buffer, bool isVirtual = false)
    {
        Segment = buffer;
        IsVirtual = isVirtual;
    }
    
    public static RentedBuffer Rent(string filePath)
    {
        using var fs = File.OpenRead(filePath);
        return Rent(fs);
    }
    
    public static RentedBuffer Rent(Stream stream)
    {
        var buffer = Rent((int)stream.Length);
        stream.ReadExactly(buffer.Span);
        return buffer;
    }
    
    public static RentedBuffer Rent(int size)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(size);
        return new RentedBuffer(new ArraySegment<byte>(buffer, 0, size));
    }

    public static RentedBuffer Virtual(ArraySegment<byte> data) => new(data, isVirtual: true);

    public void DelayReleaseTo(IRentHolder holder)
    {
        holder.Hold(this);
        HasExternalHolder = true;
    }

    public void Release()
    {
        ArgumentNullException.ThrowIfNull(Segment.Array);
        ArrayPool<byte>.Shared.Return(Segment.Array);
    }
    
    public void Dispose()
    {
        if (HasExternalHolder || IsVirtual) {
            return;
        }
        
        Release();
    }
}