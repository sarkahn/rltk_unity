using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace RLTK.Consoles.Utilities
{
    public static class CameraUtil
    {
        /// <summary>
        /// Adjusts camera properties and adds PixelPerfectCameraComponent if necessary to allow
        /// for proper rendering of the given console.
        /// </summary>
        public static void SetUpCameraForConsole(IConsole console, Camera camera)
        {
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            
            var ppCamera = GetOrCreatePixelPerfectCamera(camera);

            int2 targetRes = console.PixelsPerUnit * console.Size;

            ppCamera.assetsPPU = console.PixelsPerUnit.y;
            ppCamera.refResolutionX = targetRes.x;
            ppCamera.refResolutionY = targetRes.y;

            Shader.SetGlobalFloat("_CameraPixelScale", ppCamera.pixelRatio);
        }

        static PixelPerfectCamera GetOrCreatePixelPerfectCamera(Camera cam)
        {
            var ppCam = cam.gameObject.AddComponent<PixelPerfectCamera>();

            if( ppCam = null)
                ppCam = cam.gameObject.AddComponent<PixelPerfectCamera>();

            return ppCam;
        }


    }
}
