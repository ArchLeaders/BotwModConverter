namespace BotwModConverter.Core
{
    internal interface IBotwConverter
    {
        public byte[] ConvertToWiiu(byte[] data);
        public byte[] ConvertToSwitch(byte[] data);
    }
}
