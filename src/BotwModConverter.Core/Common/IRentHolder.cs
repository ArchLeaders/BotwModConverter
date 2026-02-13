namespace BotwModConverter.Core.Common;

public interface IRentHolder : IDisposable
{
    void Hold(RentedBuffer buffer);
}