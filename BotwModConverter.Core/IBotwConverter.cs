namespace BotwModConverter.Core
{
    public interface IBotwConverter
    {
        public Task<byte[]> ConvertToWiiu(byte[] data);
        public Task<byte[]> ConvertToSwitch(byte[] data);
    }
}
