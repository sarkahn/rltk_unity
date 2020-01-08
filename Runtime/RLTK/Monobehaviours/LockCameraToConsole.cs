using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

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
        SimpleConsoleProxy _targetConsole;

        private void OnEnable()
        {
            if (_targetConsole == null)
            {
                _targetConsole = FindObjectOfType<SimpleConsoleProxy>();
                if( _targetConsole == null )
                    Debug.LogError($"Error initializing {this.GetType().Name}, unable to find a " +
                        $"console to target", gameObject);
            }

            if (_camera == null)
                _camera = GetComponent<Camera>();

            if (_pixelCamera == null)
                _pixelCamera = GetComponent<PixelPerfectCamera>();

            _waitTime = new WaitForSeconds(.1f);
        }

        private void Start()
        {
            StartCoroutine(VerifyCamera());
        }

        IEnumerator VerifyCamera()
        {
            while (_targetConsole != null && _targetConsole.isActiveAndEnabled && isActiveAndEnabled)
            {
                int2 consoleDims = _targetConsole.Size;
                int2 consolePPU = _targetConsole.PixelsPerUnit;

                int2 targetRes = consoleDims * _targetConsole.PixelsPerUnit;

                int2 cameraDims = new int2(_pixelCamera.refResolutionX, _pixelCamera.refResolutionY);

                if (_pixelCamera.assetsPPU != consolePPU.y)
                    _pixelCamera.assetsPPU = consolePPU.y;

                int pixelScale = _pixelCamera.pixelRatio;

                _targetConsole.GetComponent<Renderer>()?.sharedMaterial?.SetFloat("_PixelScaleCamera", pixelScale);

                if (targetRes.x != cameraDims.x || targetRes.y != cameraDims.y)
                {
                    //Debug.Log("Updating camera dims");
                    _pixelCamera.refResolutionX = targetRes.x;
                    _pixelCamera.refResolutionY = targetRes.y;
                }

                if (_pixelCamera.transform.position != _targetConsole.transform.position)
                    _pixelCamera.transform.position = _targetConsole.transform.position
                        + -_targetConsole.transform.forward * 10;

                yield return _waitTime;
            }
        }

    }

}