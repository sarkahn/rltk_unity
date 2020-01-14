using Color = UnityEngine.Color;

namespace RLTK
{
    [System.Serializable]
    public struct Tile : System.IEquatable<Tile>
    {
        public byte glyph;
        public Color fgColor;
        public Color bgColor;
        

        public static Tile EmptyTile => new Tile
        {
            bgColor = Color.black,
            fgColor = Color.white,
            glyph = 0
        };

        public bool Equals(Tile other)
        {
            return glyph == other.glyph &&

                fgColor.r == other.fgColor.r &&
                fgColor.g == other.fgColor.g &&
                fgColor.b == other.fgColor.b &&
                fgColor.a == other.fgColor.a &&

                bgColor.r == other.bgColor.r &&
                bgColor.g == other.bgColor.g &&
                bgColor.b == other.bgColor.b &&
                bgColor.a == other.bgColor.a;
        }
    }
}