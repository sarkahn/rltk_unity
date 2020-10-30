
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

/// An implementation of [Bresenham's circle algorithm].
/// [Bresenham's circle algorithm]: http://members.chello.at/~easyfilter/bresenham.html
public struct BresenhamCircle //: IEnumerable<int2>
{
    int2 center;
    int radius;

    public BresenhamCircle(int2 center, int radius)
    {
        this.center = center;
        this.radius = radius;
    }
    
    public NativeList<int2> GetPoints(Allocator allocator = Allocator.TempJob)
    {
        int x = -radius;
        int y = 0;
        int error = 2 - 2 * radius;
        int quadrant = 1;

        int count = (CountFromRadius(radius));

        NativeList<int2> points = new NativeList<int2>(count, allocator);
        
        while (x < 0)
        {
            points.Add(GetPoint(center, x, y, quadrant));

            // Update variables after each set of quadrants
            if (quadrant == 4)
            {
                radius = error;

                if (radius <= y)
                {
                    y++;
                    error += y * 2 + 1;
                }

                if (radius > x || error > y)
                {
                    x++;
                    error += x * 2 + 1;
                }
            }

            quadrant = (byte)(quadrant % 4 + 1);
        }
        
        return points;
    }

    static int CountFromRadius(int radius)
    {
        switch(radius)
        {
            case 1: return 4;
            case 2: return 12;
            case 3: return 16;
            case 4: return 20;
            case 5: return 28;
            case 6: return 32;
            case 7: return 40;
            case 8: return 44;
            case 9: return 52;
            case 10: return 56;
            case 11: return 60;
            case 12: return 68;
            case 13: return 72;
            case 14: return 80;
            case 15: return 84;
            case 16: return 92;
            case 17: return 96;
            case 18: return 100;
            case 19: return 108;
            case 20: return 112;
            case 21: return 120;
            case 22: return 124;
            case 23: return 132;
            case 24: return 136;
            case 25: return 140;
            case 26: return 148;
            case 27: return 152;
            case 28: return 160;
            case 29: return 164;

            default: return (4 * (radius / 2)) + ( 8 * radius / 2) + 8;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static int2 GetPoint(int2 center, int x, int y, int quadrant)
    {
        switch (quadrant)
        {
            case 1: return new int2(center.x - x, center.y + y);
            case 2: return new int2(center.x - y, center.y - x);
            case 3: return new int2(center.x + x, center.y - y);
            case 4: return new int2(center.x + y, center.y + x);
            default: return 1;
        }
    }

    // Foreach is not supported by Burst yet: https://docs.unity3d.com/Packages/com.unity.burst@1.2/manual/index.html#cnet-language-support
    /*
    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<int2> IEnumerable<int2>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<int2>
    {
        BresenhamCircle circle;
        int2 point;

        public int2 Current => point;

        object IEnumerator.Current => Current;

        public void Dispose()
        {}

        public Enumerator(BresenhamCircle c)
        {
            circle = c;
            point = 0;
        }

        public bool MoveNext()
        {
            if( circle.x < 0 )
            {
                point = GetPoint(circle.center, circle.x, circle.y, circle.quadrant);

                // Update variables after each set of quadrants
                if (circle.quadrant == 4)
                {
                    circle.radius = circle.error;

                    if (circle.radius <= circle.y)
                    {
                        circle.y++;
                        circle.error += circle.y * 2 + 1;
                    }

                    if (circle.radius > circle.x || circle.error > circle.y)
                    {
                        circle.x++;
                        circle.error += circle.x * 2 + 1;
                    }
                }

                circle.quadrant = (byte)(circle.quadrant % 4 + 1);

                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int2 GetPoint(int2 center, int x, int y, int quadrant)
        {
            switch(quadrant)
            {
                case 1: return new int2(center.x - x, center.y + y);
                case 2: return new int2(center.x - y, center.y - x);
                case 3: return new int2(center.x + x, center.y - y);
                case 4: return new int2(center.x + y, center.y + x);
                default: return 1;
            }
        }
        
        public void Reset() { }
    }
    */
}
