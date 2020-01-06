using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace RLTK.MonoBehaviours
{

    /// <summary>
    /// Locks the camera to the given console and sets the appropriate reference resolution in the 
    /// <see cref="PixelPerfectCamera"/>.
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
                Debug.LogError($"Error initializing {this.GetType().Name}, you must set a target " +
                    $"Console in the inspector", gameObject);

            if (_camera == null)
                _camera = GetComponent<Camera>();

            if (_pixelCamera == null)
                _pixelCamera = GetComponent<PixelPerfectCamera>();

            _waitTime = new WaitForSeconds(.1f);
        }

        private void Start()
        {
            StartCoroutine(VerifyCameraDims());
        }

        IEnumerator VerifyCameraDims()
        {
            while (isActiveAndEnabled)
            {
                int2 consoleDims = _targetConsole.Size;

                int2 targetRes = consoleDims * _targetConsole.PixelsPerUnit;

                int2 cameraDims = new int2(_pixelCamera.refResolutionX, _pixelCamera.refResolutionY);

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