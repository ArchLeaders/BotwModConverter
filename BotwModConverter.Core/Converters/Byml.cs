﻿namespace BotwModConverter.Core.Converters
{
    public class Byml : IBotwConverter
    {
        public static Byml Shared { get; } = new();

        public Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }
    }
}
