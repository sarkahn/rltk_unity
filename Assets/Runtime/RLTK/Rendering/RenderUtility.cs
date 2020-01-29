using RLTK.Consoles;
using RLTK.MonoBehaviours;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace RLTK.Rendering
{

    public static class RenderUtility
    {
        const string PIXEL_COUNT_PROP_NAME = "_PixelCount";
        const string PPU_PROP_NAME = "_PixelsPerUnit";
        const string PIXEL_RATIO_PROP_NAME = "_PixelRatio";

        const string DEFAULT_MAT_PATH = "Materials/ConsoleMat";

        static Material _defaultMaterial = null;
        static Camera _camera;
        static PixelPerfectCamera _pixelCamera = null;

        internal static Camera Camera
        {
            get
            {
                if( _camera == null )
                {
                    _camera = Camera.main;
                    if (_camera == null)
                        _camera = Object.FindObjectOfType<Camera>();
                    if (_camera == null)
                        Debug.LogError("Unable to find a camera for rendering");
                }
                return _camera;
            }
        }

        internal static PixelPerfectCamera PixelCamera
        {
            get
            {
                if( _pixelCamera == null )
                {
                    var cam = Camera;
                    _pixelCamera = cam.GetComponent<PixelPerfectCamera>();
                    if (_pixelCamera == null)
                        _pixelCamera = cam.gameObject.AddComponent<PixelPerfectCamera>();
                }
                return _pixelCamera;
            }
        }

        public static Material DefaultMaterial
        {
            get
            {
                if (_defaultMaterial == null)
                    _defaultMaterial = Resources.Load<Material>(DEFAULT_MAT_PATH);
                return _defaultMaterial;
            }
        }

        /// <summary>
        /// Upload "pixel" uvs to the console mesh. Used for pixel effects in the shader.
        /// Should only be called when the console is resized.
        /// </summary>
        public static void UploadPixelData(IConsole console, Material mat)
        {
            int count = console.CellCount;

            NativeArray<float2> pixelUVs = new NativeArray<float2>(count * 4, Allocator.TempJob);

            var job = new PixelsJob
            {
                consoleSize = console.Size,
                pixelsPerUnit = PixelsPerUnit(mat),
                uvs = pixelUVs
            }.Schedule(count, 32);

            job.Complete();

            var mesh = console.Mesh;

            mesh.SetUVs(3, pixelUVs);

            pixelUVs.Dispose();
        }
        
        public static int2 PixelsPerUnit(Material mat)
        {
            int2 ppu = new int2(8, 8);

            if (mat.mainTexture != null)
            {
                var tex = mat.mainTexture;
                ppu.x = tex.width / 16;
                ppu.y = tex.height / 16;
            }

            return ppu;
        }

        /// <summary>
        /// Adjusts the given camera so it's viewport is sized properly to contain the given console.
        /// This will add a PixelPerfectCamera Component to the camera if one doesn't exist.
        /// </summary>
        public static void AdjustCameraToConsole(Camera cam, IConsole console)
        {
            cam.orthographic = true;

            int2 consoleDims = console.Size;
            int2 consolePPU = console.PixelsPerUnit;

            int2 targetRes = consoleDims * console.PixelsPerUnit;

            var pixelCam = GetOrCreatePixelCamera(cam);

            int2 cameraDims = new int2(pixelCam.refResolutionX, pixelCam.refResolutionY);

            if (pixelCam.assetsPPU != consolePPU.y)
                pixelCam.assetsPPU = consolePPU.y;

            int pixelRatio = pixelCam.pixelRatio;

            console.Material.SetFloat(PIXEL_RATIO_PROP_NAME, pixelRatio);

            if (targetRes.x != cameraDims.x || targetRes.y != cameraDims.y)
            {
                pixelCam.refResolutionX = targetRes.x;
                pixelCam.refResolutionY = targetRes.y;
            }

            var proxyConsole = console as SimpleConsoleProxy;

            if( proxyConsole != null )
            {
                var camTransform = cam.transform;
                var consoleTransform = proxyConsole.transform;

                if (consoleTransform != null && camTransform.position != consoleTransform.position)
                {
                    var p = proxyConsole.transform.position;
                    camTransform.position = new Vector3(p.x, p.y, p.z - 10);
                }
            }
        }

        /// <summary>
        /// Sets material properties that will be used in the shader for rendering the given console.
        /// Note: Unity's PixelPerfectCamera component reports incorrect PixelRatio values
        /// for the first couple of frames. This means we can't reliably call this during Awake/OnEnable.
        /// An easy workaround is to call it every frame. It's fairly lightweight.
        /// </summary>
        public static void SetMaterialProperties(IConsole console, Material mat)
        {
            mat.SetFloat(PIXEL_RATIO_PROP_NAME, PixelCamera.pixelRatio);

            int2 ppu = PixelsPerUnit(mat);

            int2 pixelCount = console.Size * ppu;

            mat.SetVector(PIXEL_COUNT_PROP_NAME, new Vector4(pixelCount.x, pixelCount.y));
        }
        
        public static void DrawConsole(IConsole console, Material mat)
        {
            SetMaterialProperties(console, mat);
            Graphics.DrawMesh(console.Mesh, Vector3.zero, Quaternion.identity, mat, 0);
        }

        static PixelPerfectCamera GetOrCreatePixelCamera(Camera cam)
        {
            var pixelCam = cam.GetComponent<PixelPerfectCamera>();
            if (pixelCam == null)
                pixelCam = cam.gameObject.AddComponent<PixelPerfectCamera>();
            return pixelCam;
        }

        [BurstCompile]
        struct PixelsJob : IJobParallelFor
        {
            public int2 consoleSize;
            public int2 pixelsPerUnit;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<float2> uvs;

            public void Execute(int index)
            {

                int x = index % consoleSize.x;
                int y = index / consoleSize.x;
                int2 p = new int2(x, y);

                index = index * 4;

                int2 bl = pixelsPerUnit * p;
                int2 right = new int2(pixelsPerUnit.x, 0);
                int2 up = new int2(0, pixelsPerUnit.y);

                // 0---1
                // | / | 
                // 2---3
                uvs[index + 0] = bl + up;
                uvs[index + 1] = bl + up + right;
                uvs[index + 2] = bl;
                uvs[index + 3] = bl + right;
            }
        }
    }
}