using RLTK.Consoles;
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
        
        IConsole _targetConsole;
        Transform _targetTransform = null;

        [SerializeField]
        SimpleConsoleProxy _consoleProxy = null;

        Coroutine _routine = null;
        
        public void SetTarget(IConsole console, Vector3 pos)
        {
            if(_routine != null)
                StopCoroutine(_routine);

            _targetConsole = console;
            _routine = StartCoroutine(VerifyCamera());
            SetPos(pos);
        }

        public void SetTarget(IConsole console, Transform targetTransform)
        {
            if( _routine != null )
                StopCoroutine(_routine);

            _targetConsole = console;
            _routine = StartCoroutine(VerifyCamera());
            _targetTransform = targetTransform;
            SetPos(targetTransform.position);
        }

        void SetPos(Vector3 p)
        {
            transform.position = new Vector3(p.x, p.y, transform.position.z);
        }

        private void OnEnable()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();

            if (_pixelCamera == null)
                _pixelCamera = GetComponent<PixelPerfectCamera>();


            _waitTime = new WaitForSeconds(.1f);
        }

        private void Start()
        {
            if(_consoleProxy == null )
                _consoleProxy = FindObjectOfType<SimpleConsoleProxy>();

            if (_consoleProxy != null)
                SetTarget(_consoleProxy, _consoleProxy.transform);
        }

        IEnumerator VerifyCamera()
        {
            while (_targetConsole != null && isActiveAndEnabled && Application.isPlaying)
            {
                int2 consoleDims = _targetConsole.Size;
                int2 consolePPU = _targetConsole.PixelsPerUnit;

                int2 targetRes = consoleDims * _targetConsole.PixelsPerUnit;

                int2 cameraDims = new int2(_pixelCamera.refResolutionX, _pixelCamera.refResolutionY);

                if (_pixelCamera.assetsPPU != consolePPU.y)
                    _pixelCamera.assetsPPU = consolePPU.y;

                int pixelScale = _pixelCamera.pixelRatio;

                _targetConsole.Material?.SetFloat("_PixelScaleCamera", pixelScale);
                
                if (targetRes.x != cameraDims.x || targetRes.y != cameraDims.y)
                {
                    //Debug.Log("Updating camera dims");
                    _pixelCamera.refResolutionX = targetRes.x;
                    _pixelCamera.refResolutionY = targetRes.y;
                }

                if (_targetTransform != null && transform.position != _targetTransform.position)
                    SetPos(_targetTransform.position);

                yield return _waitTime;
            }
        }

    }

}