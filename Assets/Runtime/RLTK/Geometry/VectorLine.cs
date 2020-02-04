
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System.Linq;

public struct VectorLine //: IEnumerable<int2>
{
    int2 start;
    int2 end;
    

    public VectorLine(int startX, int startY, int endX, int endY) :
        this(new int2(startX, startY), new int2(endX, endY))
    { }

    public VectorLine(int2 start, int2 end)
    {
        this.start = start;
        this.end = end;
    }

    public NativeArray<int2> GetPoints(Allocator allocator = Allocator.TempJob)
    {
        float2 curr = start + new float2(.5f, .5f);
        float2 dest = end + new float2(.5f, .5f);
        var slope = math.normalize(dest - curr);
        
        int count = (int)math.distance(start, end) + 2;

        NativeList<int2> points = new NativeList<int2>(count, allocator);

        int2 p = new int2(curr);
        int2 last = p;
        points.Add(p);
        
        while (p.x != end.x || p.y != end.y)
        {
            curr += slope;
            p = new int2(math.floor(curr));

            if( p.x != last.x || p.y != last.y )
                points.Add(p);

            last = p;
        }

        //points.Add(p);

        return points;
    }

    // Foreach is not currently supported in Burst
    /*
    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<int2> IEnumerable<int2>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct Enumerator : IEnumerator<int2>
    {
        VectorLine line;

        public Enumerator(VectorLine line)
        {
            this.line = line;
        }

        public int2 Current => new int2(line.curr);
        object IEnumerator.Current => Current;
        
        public bool MoveNext()
        {
            int2 p = new int2(line.curr);

            if( p.x != line.end.x || p.y != line.end.y )
            {
                line.curr += line.slope;
                return true;
            }

            return false;
        }

        public void Dispose() { }
        public void Reset() { }
    }
    */
}
