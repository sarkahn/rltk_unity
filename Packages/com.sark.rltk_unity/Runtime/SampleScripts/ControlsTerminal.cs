
using System.Collections;
using UnityEngine;

using Sark.Common.CameraExtensions;
using Unity.Mathematics;
using Sark.Terminals;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Sark.RLTK.Samples
{
    public class ControlsTerminal : MonoBehaviour
    {
        [SerializeField]
        TerminalBehaviour _parent = null;
        TerminalBehaviour _term;
        int2 _lastSize;

        Controls _controls;

        string _displayText = null;
        float avg = 0F; //declare this variable outside Update
                        //run this in Update()

        [SerializeField]
        List<string> _strings = new List<string>();

        private void Awake()
        {
            _term = GetComponent<TerminalBehaviour>();
        }

        private void OnEnable()
        {
            if (_parent == null)
                Debug.LogError("Error enabling controls terminal - set a parent in the inspector");

            _controls = new Controls();
            _controls.Enable();

            _controls.Default.ToggleControls.performed += ToggleVisibility;
        }

        public void SetDisplayText(string str)
        {
            _displayText = str;
            Print();
        }

        // Start is called before the first frame update
        IEnumerator Start()
        {
            yield return null;
            yield return null;
            yield return null;
            Print();
            Align();
        }

        private void LateUpdate()
        {
            avg += ((Time.deltaTime / Time.timeScale) - avg) * 0.03f; //run this every frame
            if (!_parent.Size.Equals(_lastSize))
            {
                _lastSize = _parent.Size;
                Align();
            }

            if (Keyboard.current.vKey.wasPressedThisFrame)
                QualitySettings.vSyncCount = (QualitySettings.vSyncCount + 1) % 3; 

            Print();
        }

        void Print()
        {
            string vsync = QualitySettings.vSyncCount switch
            {
                1 => "60",
                2 => "30",
                _ => "OFF"
            };

            _term.Resize(_term.Width, _strings.Count + 3);
            Align();
            int y = _term.Size.y - 1;
            for(int i = 0; i < _strings.Count; ++i)
            {
                _term.Print(2, y--, _strings[i]);
            }

            _term.Print(2, y--, $"VSync {vsync} FPS {1F / avg}     ");
            --y;
            if (!string.IsNullOrEmpty(_displayText))
                _term.Print(1, y--, _displayText + "       ");

            //_term.Print(1, y--, "CONTROLS");
            //_term.Print(1, y--, "F1 - Toggle help");
            //_term.Print(1, y--, "LMB + Drag - Place/Remove walls");
            //_term.Print(1, y--, "N - Add noise to the map");
            //_term.Print(1, y--, "C - Clear");
            //_term.Print(1, y--, "Arrow Keys - Resize");
            //_term.Print(1, y--, "RMB - place start/end points");
            //_term.Print(1, y--, "Space - Find Path");
            //_term.Print(1, y--, $"VSync {vsync} FPS {1F / avg}     ");
            //--y;
            //if (!string.IsNullOrEmpty(_displayText))
            //    _term.Print(1, y--, _displayText + "       ");
        }


        void Align()
        {
            float3 p = transform.position;
            float2 viewportPosition = new float2(0, 1);
            float2 align = new float2(1, -1);
            float2 size = _term.GetWorldSize().xy;
            p.xy = Camera.main.GetAlignedViewportPosition(viewportPosition, align, size).xy;
            transform.position = p;
        }

        void ToggleVisibility(InputAction.CallbackContext ctx)
        {
            float3 p = transform.position;
            if (p.z == -1)
                p.z = 1;
            else
                p.z = -1;

            transform.position = p;
        }
    } 
}
