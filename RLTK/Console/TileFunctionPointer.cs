
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace RLTK
{
    public delegate Tile ConsoleTileFunc(int width, int height, int x, int y, Tile tile);

    public static class TileFunctionPointer
    {
        public static FunctionPointer<ConsoleTileFunc> Create(ConsoleTileFunc func)
        {
            return BurstCompiler.CompileFunctionPointer(func);
        }
    }
}