using RLTK.Consoles;
using RLTK.MonoBehaviours;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.Samples
{
    public class ManualDraw : MonoBehaviour
    {
        NativeConsole _console;

        [SerializeField]
        Material _mat;

        [SerializeField]
        int _width = 40;

        [SerializeField]
        int _height = 15;

        bool _doRebuild;

        private void Awake()
        {
            if (_mat == null)
                _mat = Resources.Load<Material>("Materials/ConsoleMat");

            _doRebuild = true;
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
            _console.Print(10, 5, "Hello, World!");
        }

        private void LateUpdate()
        {
            _console.Update();

            if (_console.Material != _mat)
                _console.SetMaterial(_mat);

            _console.Draw();
        }

        void Rebuild()
        {
            _console?.Dispose();

            _console = new NativeConsole(_width, _height, _mat, new Mesh());
            var cam = FindObjectOfType<Camera>();

            var attach = cam.GetComponent<LockCameraToConsole>();
            if (attach == null)
                attach = cam.gameObject.AddComponent<LockCameraToConsole>();
            attach.SetTarget(_console, transform);
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