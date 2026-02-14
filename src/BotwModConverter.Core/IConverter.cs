namespace BotwModConverter.Core;

public interface IConverter
{
    ConvertResult ToSwitch<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct;

    ConvertResult ToWiiu<T>(ConverterEngine engine, ref T file) where T : IModFile, allows ref struct;
}