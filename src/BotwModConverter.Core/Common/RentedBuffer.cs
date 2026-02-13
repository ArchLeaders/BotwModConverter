using System.Buffers;

namespace BotwModConverter.Core.Common;

public struct RentedBuffer : IDisposable
{
    private readonly byte[] _rented;
    private readonly int _size;
    private bool _isOwned = true;
    
    public Span<byte> Span => _rented.AsSpan(0, _size);
    
    public ArraySegment<byte> Segment => new(_rented, 0, _size);

    private RentedBuffer(byte[] rented, int size)
    {
        _rented = rented;
        _size = size;
    }
    
    public static RentedBuffer Rent(string filePath, int? size = null)
    {
        using var fs = File.OpenRead(filePath);
        return Rent(fs);
    }
    
    public static RentedBuffer Rent(Stream stream, int? size = null)
    {
        var buffer = Rent(size ?? (int)stream.Length);
        stream.ReadExactly(buffer.Span);
        return buffer;
    }
    
    public static RentedBuffer Rent(int size)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(size);
        return new RentedBuffer(buffer, size);
    }

    public void DelayReleaseTo(IRentHolder holder)
    {
        holder.Hold(this);
        _isOwned = false;
    }

    public void Release()
    {
        ArrayPool<byte>.Shared.Return(_rented);
    }
    
    public void Dispose()
    {
        if (_isOwned) {
            Release();
        }
    }
}