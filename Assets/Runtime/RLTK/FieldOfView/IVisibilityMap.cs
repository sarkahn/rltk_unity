
using RLTK.NativeContainers;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.FieldOfView
{
    public interface IVisibilityMap
    {
        bool IsOpaque(int2 p);
        bool IsInBounds(int2 p);
        void SetVisible(int2 p);
        float Distance(int2 a, int2 b);
    }
}