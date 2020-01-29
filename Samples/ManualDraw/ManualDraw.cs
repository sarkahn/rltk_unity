using RLTK.Consoles;
using RLTK.MonoBehaviours;
using RLTK.Rendering;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.Samples
{
    public class ManualDraw : MonoBehaviour
    {
        SimpleConsole _console;

        [SerializeField]
        Material _mat = null;

        [SerializeField]
        int _width = 40;

        [SerializeField]
        int _height = 15;

        bool _doRebuild;

        private void Awake()
        {
            _console = new SimpleConsole(_mat);
            Rebuild();
        }

        private void OnDestroy()
        {
            _console.Dispose();
        }

        private void Update()
        {
            if (_doRebuild)
            {
                _doRebuild = false;
                Rebuild();
            }

            _console.ClearScreen();
            _console.Print(3, 5, "Manual Draw");
        }

        private void LateUpdate()
        {
            _console.Update();
            
            _console.Draw();
        }

        void Rebuild()
        {
            _console.Resize(_width, _height);
            var cam = FindObjectOfType<Camera>();

            RenderUtility.UploadPixelData(_console, _mat);
            RenderUtility.AdjustCameraToConsole(cam, _console);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled || !Application.isPlaying)
                return;

            _width = math.max(1, _width);
            _height = math.max(1, _height);

            if (_console.Width != _width || _console.Height != _height )
            {
                _doRebuild = true;
            }
        }
#endif
    }
}