using RLTK.Consoles;
using RLTK.Rendering;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

namespace RLTK.MonoBehaviours
{

    /// <summary>
    /// Locks the camera to the given console and sets the appropriate reference resolution in the 
    /// <see cref="PixelPerfectCamera"/>. The camera's orthographic size will automatically scale to
    /// show as much of the console as possible while mainting perfect pixel rendering.
    /// If the console is really small in the viewport try changing it's horizontal or vertical size
    /// to better fit inside the viewport so the pixel camera can adjust properly.
    /// </summary>
    [RequireComponent(typeof(Camera), typeof(PixelPerfectCamera))]
    public class LockCameraToConsole : MonoBehaviour
    {
        Camera _camera;
        PixelPerfectCamera _pixelCamera;
        WaitForSeconds _waitTime;
        
        [SerializeField]
        SimpleConsoleProxy _consoleProxy = null;

        Coroutine _routine = null;
        
        public void SetTarget(IConsole console, Vector3 pos)
        {
            if(_routine != null)
                StopCoroutine(_routine);
            
            _routine = StartCoroutine(VerifyCamera());
        }

        public void SetTarget(IConsole console, Transform targetTransform)
        {
            if( _routine != null )
                StopCoroutine(_routine);
            
            _routine = StartCoroutine(VerifyCamera());
        }

        private void Awake()
        {
            if (_consoleProxy == null)
                _consoleProxy = FindObjectOfType<SimpleConsoleProxy>();

            if (_camera == null)
                _camera = GetComponent<Camera>();

            if (_pixelCamera == null)
                _pixelCamera = GetComponent<PixelPerfectCamera>();

            _waitTime = new WaitForSeconds(.1f);
        }

        private void Start()
        {
            SetTarget(_consoleProxy, _consoleProxy.transform);
        }





        IEnumerator VerifyCamera()
        {
            while (_consoleProxy != null && isActiveAndEnabled && Application.isPlaying)
            {
                RenderUtility.AdjustCameraToConsole(_consoleProxy, _camera);

                yield return _waitTime;
            }
        }

    }

}