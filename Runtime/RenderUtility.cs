using Sark.RenderUtils;
using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK
{
    public static class RenderUtility
    {
        static Material _defaultMat;
        static Material DefaultMat
        {
            get
            {
                if(_defaultMat == null)
                {
                    _defaultMat = Resources.Load<Material>("Terminal8x8");
                }
                return _defaultMat;
            }
        }

        public static void AdjustCameraToTerminal(SimpleTerminal term)
        {
            var cam = Object.FindObjectOfType<TiledCamera>();
            if(cam == null)
            {
                var mainCam = Camera.main;
                cam = mainCam.gameObject.AddComponent<TiledCamera>();
            }
            cam.TileSize = new int2(8, 8);
            cam.TileCount = term.Size;
        }

        public static void RenderTerminal(SimpleTerminal term, float3 pos = default)
        {
            var mat = DefaultMat;
            var matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
            Graphics.DrawMesh(term.Mesh, matrix, mat, 0);
        }
    } 
}
