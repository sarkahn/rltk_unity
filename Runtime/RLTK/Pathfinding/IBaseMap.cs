using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.Pathfinding
{
    public interface IBaseMap
    {
        bool IsOpaque(int2 pos);

        // Returns an array of positions you can path to from the given point
        NativeArray<int2> GetAvailableExits(int2 pos);

        // Return the distance you would like to use for pathfinding
        // Gets called during pathfinding as the Hueristic function
        int GetPathindingDistance(int2 a, int2 b);
    }
}
