using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.Pathfinding
{
    public unsafe struct DijkstraMap
    {
        public NativeArray<int> values;
        public int2 size { get; private set; }


        public DijkstraMap(int startIndex, int2 size, IPathingMap map)
        {
            this.size = size;
            values = new NativeArray<int>();

            int len = math.csum(size);
            var openList = new NativeList<PathNode>(len, Allocator.Temp);
            var closedList = new UnsafeBitArray(len, Allocator.Temp);

            openList.Add(new PathNode { posIndex = startIndex } );

            while(openList.Length > 0)
            {
                var lastNode = openList[openList.Length - 1];
                int posIndex = lastNode.posIndex;
                openList.RemoveAtSwapBack(openList.Length - 1);

                
            }
        }
    }

}


/*
 * 
 * struct ParallelDM
 * {
 *  List<float> map
 *  int maxDepth; // 
 *  List<int> starts; // Indices of start positions
 * }
 * 
 * BuildParallel:
 * Pass in an array of start positions
 * 
 *
 * build_parallel(Map map, List<int> starts) - Starts = a list of indices representing start points on the map
 * {
 *  List<ParallelDM> layers = new List(starts.length);
 *  
 *  foreach( var start in starts)
 *  {
 *    
 *  }
 * }
 * 
 * 
 * */
