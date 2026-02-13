namespace BotwModConverter.Core;

public interface IConverter
{
    public bool ToSwitch(ConverterEngine engine, ref ModFile file);
    
    public bool ToWiiu(ConverterEngine engine, ref ModFile file);
}