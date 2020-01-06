
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace RLTK
{
    public struct IntRect : System.IEquatable<IntRect>, IEnumerable<int2>
    {
        public int xMin;
        public int yMin;

        public int xMax;
        public int yMax;

        public int2 Min
        {
            get => Position;
            set
            {
                xMin = value.x;
                yMin = value.y;
            }
        }

        public int2 Max
        {
            get => new int2(xMax, yMax);
            set
            {
                xMax = value.x;
                yMax = value.y;
            }
        }

        public int2 Position
        {
            get => new int2(xMin, yMin);
            // Moves position without affecting size
            set
            {
                int2 s = Size;
                xMin = value.x;
                yMin = value.y;
                Size = s;
            }
        }

        public int2 Size
        {
            get => new int2(xMax - xMin, yMax - yMin);
            set
            {
                xMax = xMin + value.x;
                yMax = yMin + value.y;
            }
        }

        public int2 Center
        {
            get => Position + Size / 2;
            set
            {
                Position = value - Size / 2;
            }
        }

        public static IntRect FromPositionSize(int x, int y, int width, int height) =>
            new IntRect { xMin = x, yMin = y, xMax = x + width, yMax = y + height };

        public static IntRect FromPositionSize(int2 pos, int2 size) =>
            new IntRect { Position = pos, Size = size };

        public static IntRect FromExtents(int xMin, int yMin, int xMax, int yMax) =>
            new IntRect { xMin = xMin, yMin = yMin, xMax = xMax, yMax = yMax };

        public static IntRect FromExtents(int2 min, int2 max) =>
            new IntRect { Position = min, Size = max - min };

        public bool Intersect(IntRect other)
        {
            return xMin <= other.xMax && xMax >= other.xMin && yMin <= other.yMax && yMax >= other.yMin;
        }

        public bool Intersect(int2 p)
        {
            return p.x >= xMin && p.x <= xMax && p.y >= yMin && p.y <= yMax;
        }

        public static IntRect operator +(IntRect lhs, IntRect rhs)
        {
            lhs.Position += rhs.Position;
            return lhs;
        }

        public bool Equals(IntRect other)
        {
            return xMin == other.xMin && xMax == other.xMax && yMin == other.yMin && yMax == other.yMax;
        }

        public IEnumerator<int2> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Struct iterator to avoid allocations during ForEach
        /// </summary>
        public struct Enumerator : IEnumerator<int2>
        {
            public int2 Current => curr;

            object IEnumerator.Current => Current;

            int2 curr;
            int2 min;
            int2 max;

            public Enumerator(IntRect r)
            {
                min = r.Min;
                max = r.Max;
                curr = min;
                curr.x--;
            }


            public bool MoveNext()
            {
                curr.x++;

                if (curr.x == max.x)
                {
                    curr.x = min.x;
                    curr.y++;
                }

                return curr.x < max.x && curr.y < max.y;
            }

            public void Dispose() { }
            public void Reset() { }
        }
    }
}