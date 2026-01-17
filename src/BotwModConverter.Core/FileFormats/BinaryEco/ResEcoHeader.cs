using System.Runtime.InteropServices;
using Entish.Attributes;

namespace BotwModConverter.Core.FileFormats.BinaryEco;

[Swappable]
[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0x10)]
public partial struct ResEcoHeader
{
    public uint Magic;
    public int RowCount;
    public uint Divisor;
}