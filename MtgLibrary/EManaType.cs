using System;

namespace Mtg
{
    [Flags]
    public enum EManaType
    {
        White = 1,
        Blue = 2,
        Red = 4,
        Black = 8,
        Green = 16,
        Any = 32,
        None = 0,
    }
}
