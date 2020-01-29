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
        const string SHADER_PIXELCOUNT_NAME = "_PixelCount";
        const string SHADER_PIXELSPERUNIT_NAME = "_PixelsPerUnit";
        const string SHADER_PIXELRATIO_NAME = "_PixelRatio";

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
        /// Only needs to be called when the console is resized, and only if you intend to
        /// use the "screen" effects in the console shader (IE: screen burn, scanlines)
        /// </summary>
        public static void UpdatePixelEffectData(IConsole console)
        {
            int count = console.CellCount;

            NativeArray<float2> pixelUVs = new NativeArray<float2>(count * 4, Allocator.TempJob);

            var job = new PixelsJob
            {
                consoleSize = console.Size,
                pixelsPerUnit = PixelsPerUnit(console.Material),
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
        /// Adjusts the camera so it's viewport is sized properly to contain the given console.
        /// This will add a PixelPerfectCamera Component to the camera if one doesn't exist, and adjust
        /// it's settings automatically. Only needs to be called once each time the console is resized.
        /// </summary>
        /// <param name="cam">The camera to adjust. A default camera will be found automatically if null.</param>
        public static void AdjustCameraToConsole(IConsole console, Camera cam = null )
        {
            if (cam == null)
                cam = Camera;

            cam.orthographic = true;

            int2 consoleDims = console.Size;
            int2 consolePPU = console.PixelsPerUnit;

            int2 targetRes = consoleDims * console.PixelsPerUnit;

            var pixelCam = GetOrCreatePixelCamera(cam);

            int2 cameraDims = new int2(pixelCam.refResolutionX, pixelCam.refResolutionY);

            if (pixelCam.assetsPPU != consolePPU.y)
                pixelCam.assetsPPU = consolePPU.y;

            int pixelRatio = pixelCam.pixelRatio;

            console.Material.SetFloat(SHADER_PIXELRATIO_NAME, pixelRatio);

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
        /// <para>Sets material properties that will be used in the shader for rendering special
        /// effects in the console. Note this is NOT required for normal text rendering,
        /// you only need to call this if you want to use the shader's "screen" effects like
        /// scanlines and screen burn.</para>
        /// 
        /// <para>Important: Unity's PixelPerfectCamera component reports incorrect PixelRatio values
        /// for the first couple of frames. This means we can't reliably call this during Awake/OnEnable.
        /// An easy workaround is to call this every frame. It's fairly lightweight.</para>
        /// </summary>
        public static void UpdatePixelEffectProperties(IConsole console, Material mat = null)
        {
            mat = mat == null ? console.Material : mat;

            mat.SetFloat(SHADER_PIXELRATIO_NAME, PixelCamera.pixelRatio);

            int2 ppu = PixelsPerUnit(mat);

            int2 pixelCount = console.Size * ppu;

            mat.SetVector(SHADER_PIXELCOUNT_NAME, new Vector4(pixelCount.x, pixelCount.y));
        }
        
        /// <summary>
        /// Render the console using the given material
        /// </summary>
        /// <param name="console"></param>
        /// <param name="mat"></param>
        public static void DrawConsole(IConsole console, Material mat)
        {
            Graphics.DrawMesh(console.Mesh, Vector3.zero, Quaternion.identity, mat, 0);
        }

        static PixelPerfectCamera GetOrCreatePixelCamera(Camera cam)
        {
            var pixelCam = cam.GetComponent<PixelPerfectCamera>();
            if (pixelCam == null)
                pixelCam = cam.gameObject.AddComponent<PixelPerfectCamera>();
            return pixelCam;
        }

        /// <summary>
        /// Job for assigning pixel data to a console mesh. Only needed if using
        /// the "pixel" effects on the console (screen burn, scanlines)
        /// </summary>
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